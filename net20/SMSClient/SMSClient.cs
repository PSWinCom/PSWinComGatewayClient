using System;
using System.Xml;
using System.Collections;
using System.Web;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text;

namespace PSWinCom.Gateway.Client
{
    public interface ISMSClient
    {
        string Username { get; set; }
        string Password { get; set; }
        string PrimaryGateway { get; set; }
        string SecondaryGateway { get; set; }
        string SessionData { get; set; }
        string AffiliateProgram { get; set; }
        IWebProxy Proxy { get; set; }
        int ConnectTimeout { get; set; }
        bool SendMessagesBySocket(string url, int port);
        void SendMessages();
        void HandleIncomingMessages(Stream inStream, Stream outStream);
        SMSClient.MessageCollection Messages { get; }
        SMSClient.MessageCollection ReceivedMessages { get; }
        SMSClient.DeliveryReportCollection DeliveryReports { get; }
    }

    /// <summary>
	/// SMS Client implementation for PSWinCom SMS Gateway
	/// </summary>
    public class SMSClient : ISMSClient
	{

		#region Nested Message collection class
		public class MessageCollection : DictionaryBase
		{
			public MessageCollection() : base() 
			{
			}

			public Message this[int key] { get { return( (Message) Dictionary[key] ); } set { Dictionary[key] = value; } }
			public ICollection Keys { get { return( Dictionary.Keys ); } }
			public ICollection Values { get { return( Dictionary.Values ); } }

			public void Add(int key, Message value)  
			{
				Dictionary.Add(key, value);
			}

			public bool Contains(int key)  
			{
				return( Dictionary.Contains(key));
			}

			public void Remove(int key)  
			{
				Dictionary.Remove(key);
			}

            internal MessageCollection Clone() {
                var clone = new MessageCollection();
                foreach (int i in Dictionary.Keys)
                {
                    clone.Add(i, (Message)Dictionary[i]);
                }
                return clone;
            }

		}

		#endregion

		#region Nested Delivery Report collection class
		public class DeliveryReportCollection : DictionaryBase
		{
			public DeliveryReportCollection() : base() 
			{
			}

			public DeliveryReport this[int key] { get { return( (DeliveryReport) Dictionary[key] ); } set { Dictionary[key] = value; } }
			public ICollection Keys { get { return( Dictionary.Keys ); } }
			public ICollection Values { get { return( Dictionary.Values ); } }

			public void Add(int key, DeliveryReport value)  
			{
				Dictionary.Add(key, value);
			}

			public bool Contains(int key)  
			{
				return( Dictionary.Contains(key));
			}

			public void Remove(int key)  
			{
				Dictionary.Remove(key);
			}

		}

		#endregion

		#region Nested Resolver
		
		public class XmlLocalResolver : XmlUrlResolver
		{
			override public object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
			{ 
				// Force same folder as assembly
				int i = absoluteUri.AbsolutePath.LastIndexOf("/");
				Uri localUri = null;
				if(i>0)
					localUri = new Uri(AppDomain.CurrentDomain.BaseDirectory + absoluteUri.AbsolutePath.Substring(i));
				else
					localUri = absoluteUri;
				return base.GetEntity(localUri, role, ofObjectToReturn);
			}
		}

		public class XmlEmbeddedResourceResolver : XmlResolver 
		{

			private XmlResolver fallbackResolver = new XmlUrlResolver();

			private ICredentials credentials;

			public override ICredentials Credentials 
			{
				set 
				{
					this.credentials = value;
				}
			}

			public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn) 
			{
				int slashIndex = absoluteUri.LocalPath.LastIndexOf('\\');
				String resourcePath = absoluteUri.LocalPath.Substring(slashIndex + 1);

				Object resourceStream = GetType().Assembly.GetManifestResourceStream("PSWinCom.Gateway.Client." + resourcePath);
				if (resourceStream == null)
					resourceStream = fallbackResolver.GetEntity(
						absoluteUri, role, ofObjectToReturn);

				return resourceStream;
			}

		}

		#endregion

		private int _batchSize = 100;
        private string _Username;
		private string _Password;
		private string _PrimaryGateway;
		private string _SecondaryGateway;
		private string _SessionData;
		private string _AffiliateProgram;
		private int	 _ConnectTimeout;
		private IWebProxy _Proxy;

		/// <summary>
		/// Collection of outgoing Message objects. Add your Message objects to this collection before
		/// sending the messages.
		/// </summary>
		public MessageCollection Messages { get; private set; }
		/// <summary>
		/// Collection of incoming Message objects. This collection will contain the received messages
		/// after running HandleIncomingMessages. The collection is emptied by the HandleIncomingMessages() method.
		/// </summary>
        public MessageCollection ReceivedMessages { get; private set; }
		/// <summary>
		/// Collection of received DeliveryReport objects. This collection will contain the received delivery reports
		/// after running HandleIncomingMessages. The collection is emptied by the HandleIncomingMessages() method.
		/// </summary>
		public DeliveryReportCollection DeliveryReports { get; private set; }

        private MessageCollection _messagesToSend;

        /// <summary>
        /// Controls the size of batches sent to Gateway, defaults to 100. Set it to 0 or less to control batch size on your own.
        /// </summary>
        public int BatchSize
        {
            get
            {
                return _batchSize;
            }
            set
            {
                _batchSize = value;
            }
        }

		#region Accessors
		/// <summary>
		/// Username on SMS Gateway
		/// </summary>
		public string Username { get { return _Username; } set { _Username = value; } }		
		/// <summary>
		/// Password on SMS Gateway
		/// </summary>
		public string Password { get { return _Password; } set { _Password = value; } }		
		/// <summary>
		/// URL to PSWinCom SMS Gateway
		/// </summary>
		public string PrimaryGateway { get { return _PrimaryGateway; } set { _PrimaryGateway = value; } }		
		/// <summary>
		/// Backup URL to PSWinCom SMS Gateway
		/// </summary>
		public string SecondaryGateway { get { return _SecondaryGateway; } set { _SecondaryGateway = value; } }		
		/// <summary>
		/// SessionData. Leave empty if not required.
		/// </summary>
		public string SessionData { get { return _SessionData; } set { _SessionData = value; } }
		/// <summary>
		///  Affiliate program identificator. Leave empty if not required.
		/// </summary>
		public string AffiliateProgram { get { return _AffiliateProgram; } set { _AffiliateProgram = value; } }
		/// <summary>
		/// Set IWebProxy to be used when sending messages. Only required if you access internet through a Proxy.
		/// </summary>
		public IWebProxy Proxy { get { return _Proxy; } set { _Proxy = value; } }		
		/// <summary>
		/// Connect timeout value in seconds. If no response has been received within this time, SendMessages will try failover Gateway.
		/// If failover also times out it will exit. Default value is 10 seconds.
		/// </summary>
		public int ConnectTimeout { get { return _ConnectTimeout; } set { _ConnectTimeout = value; } }

		
		#endregion

		/// <summary>
		/// Default constructor. PrimaryGateway and SecondaryGateway will be default set to
		/// http://sms3.pswin.com/sms and http://sms3-backup.pswin.com/sms
		/// </summary>
		public SMSClient()
		{
			Messages = new MessageCollection();
			ReceivedMessages = new MessageCollection();
			DeliveryReports = new DeliveryReportCollection();
			_Username = null;
			_Password = null;
			_PrimaryGateway = "http://sms3.pswin.com/sms";
			_SecondaryGateway = "http://sms3-backup.pswin.com/sms";
			_ConnectTimeout = 20;
		}

        /// <summary>
        /// Sends messages using socket protocol. NOTE! Send status and reference will not be updated when you use this method!
        /// </summary>
        /// <param name="hostname">Host name of server you are sending to</param>
        /// <param name="port">Port to connect to</param>
        /// <returns></returns>
        public bool SendMessagesBySocket(string hostname, int port)
        {
            bool result = false;
            var Client = new TcpClient(hostname, port);
            Client.SendTimeout = 10000;
            Client.ReceiveTimeout = 10000;
            StringWriter sw = new StringWriter(); 
            XmlTextWriter tx = new XmlTextWriter(sw); 
            GetDocumentXml().WriteTo(tx);
            string messageData = sw.ToString();
                
            var bytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(messageData);
            var stream = Client.GetStream();

            stream.Write(bytes, 0, bytes.Length);
            /*
            var reader = new StreamReader(stream, Encoding.GetEncoding("iso-8859-1"));
            string response = null;
            try
            {
                response = reader.ReadToEnd();
                result = true;
            }
            finally
            {
                reader.Close();
            }
            */
            Client.Close();

            return result;
        }


		/// <summary>
		/// Send all messages in the Messages collection. This operation will block while communicating
		/// with the Gateway. After it has been completed, the Message objects in the Messages collection
		/// is updated with the Status, (and if applicable, also the Reference or FailedReason properties).
		/// If both PrimaryGateway and SecondaryGateway is specified, then the SendMessage() method will
		/// failover to SecondaryGateway if there is any communication exception from the main Gateway.
		/// </summary>
		public void SendMessages()
		{
            if (_messagesToSend == null || _messagesToSend.Count == 0)
            {
                _messagesToSend = Messages.Clone();
            }

            while (_messagesToSend.Count > 0)
            {
                
			    XmlDocument doc = GetDocumentXml();
			    XmlDocument docResponse = null;
			    try
			    {
				    docResponse = HttpPost(doc, _PrimaryGateway);
			    } 
			    catch(Exception e)
			    {
				    // Failed to post using primary gateway, let's try secondary if given
				    if(_SecondaryGateway != null && _SecondaryGateway.Length > 0)
				    {
					    docResponse = HttpPost(doc, _SecondaryGateway);
				    } 
				    else 
				    {
					    throw;
				    }
			    }
			    CheckResponse(docResponse);
            }
		}

		/// <summary>
		/// Reads Xml from given stream and retrieves incoming messages or delivery reports
		/// within the XML document. The Messages or DeliveryReport are stored in the IncomingMessages
		/// or DeliveryReports collection.
		/// </summary>
		/// <param name="inStream"></param>
		/// <param name="outStream"></param>
		public void HandleIncomingMessages(Stream inStream, Stream outStream)
		{
			ReceivedMessages.Clear();
			DeliveryReports.Clear();
			XmlDocument docRequest = new XmlDocument();
			XmlLocalResolver res = new XmlLocalResolver();
			XmlEmbeddedResourceResolver res2 = new XmlEmbeddedResourceResolver();
			docRequest.XmlResolver = res2;
			docRequest.Load(inStream);
			XmlDocument docResponse = CheckRequest(docRequest);
			string sResponse = "<?xml version=\"1.0\"?>\n" + docResponse.OuterXml;
			StreamWriter sw = new StreamWriter(outStream, System.Text.Encoding.GetEncoding("ISO-8859-1"));
			sw.Write(sResponse);
			sw.Flush();
			outStream.Flush();
		}

		/// <summary>
		/// Build XmlDocument with messages to send.
		/// </summary>
		/// <returns>XmlDocument according to DTD for SMS Gateway</returns>
		private XmlDocument GetDocumentXml()
		{
			XmlDocument doc = new XmlDocument();
			XmlElement elmSession = doc.CreateElement("SESSION");
			elmSession.AppendChild(CreateElement(doc, "CLIENT", _Username));
			elmSession.AppendChild(CreateElement(doc, "PW", _Password));
			if(_SessionData != null && _SessionData.Length > 0)
				elmSession.AppendChild(CreateElement(doc, "SD", _SessionData));
			if(_AffiliateProgram != null && _AffiliateProgram.Length > 0)
				elmSession.AppendChild(CreateElement(doc, "AP", _AffiliateProgram));

            int _currentBatchSize = 0;
            XmlElement elmMsgList = doc.CreateElement("MSGLST");

            int[] keys = new int[_messagesToSend.Keys.Count];
            _messagesToSend.Keys.CopyTo(keys, 0);

            foreach (int i in keys)
            {
                elmMsgList.AppendChild(GetMessageXml(doc, _messagesToSend[i], i));
                _messagesToSend.Remove(i);
                _currentBatchSize++;
                if (_batchSize > 0 && _currentBatchSize == _batchSize)
                    break;
            }
			elmSession.AppendChild(elmMsgList);
			doc.AppendChild(elmSession);

			return doc;

		}

		/// <summary>
		/// Build Xml for a particular message
		/// </summary>
		/// <param name="doc">Root Xml Document</param>
		/// <param name="m">Message object to transform</param>
		/// <param name="id">index in collection</param>
		/// <returns>Message as XmlElement</returns>
		private XmlElement GetMessageXml(XmlDocument doc, Message m, int id)
		{
			XmlElement elmMsg = doc.CreateElement("MSG");
			elmMsg.AppendChild(CreateElement(doc, "ID", id.ToString()));
			if(m.Network != null && m.Network.Length > 0)
				elmMsg.AppendChild(CreateElement(doc, "NET", m.Network));
			if(m.Tariff > 0)
				elmMsg.AppendChild(CreateElement(doc, "TARIFF", m.Tariff.ToString()));
			elmMsg.AppendChild(CreateElement(doc, "TEXT", m.Text));
			if(m.MessageClass != 2)
				elmMsg.AppendChild(CreateElement(doc, "CLASS", m.MessageClass.ToString()));
			if(m.MessageType != MessageType.Text)
				elmMsg.AppendChild(CreateElement(doc, "OP", "" + (short)m.MessageType));
			if(m.RequestReceipt)
				elmMsg.AppendChild(CreateElement(doc, "RCPREQ", "Y"));
			elmMsg.AppendChild(CreateElement(doc, "SND", m.SenderNumber));
			elmMsg.AppendChild(CreateElement(doc, "RCV", m.ReceiverNumber));
			if(m.TimeToLive > 0)
				elmMsg.AppendChild(CreateElement(doc, "TTL", m.TimeToLive.ToString()));
			if(m.CPATag != null && m.CPATag.Length > 0)
				elmMsg.AppendChild(CreateElement(doc, "CPATAG", m.CPATag));
			if(m.AgeLimit > 0)
				elmMsg.AppendChild(CreateElement(doc, "AGELIMIT", m.AgeLimit.ToString()));
			if(m.ShortCode != null && m.ShortCode.Length > 0)
				elmMsg.AppendChild(CreateElement(doc, "SHORTCODE", m.ShortCode));
			if(m.MessageReplaceSet != MessageReplaceSet.NoReplace)
				elmMsg.AppendChild(CreateElement(doc, "REPLACE", "" + (short)m.MessageReplaceSet));
			if(m.DeferredDelivery)
				elmMsg.AppendChild(CreateElement(doc, "DELIVERYTIME", m.DeliveryTime.ToString("yyyyMMddHHmm")));
            if (!string.IsNullOrEmpty(m.ServiceCode))
                elmMsg.AppendChild(CreateElement(doc, "SERVICECODE", m.ServiceCode));
            return elmMsg;
		}

		/// <summary>
		/// Create a XmlElement with given name and value
		/// </summary>
		/// <param name="doc">Xml Document context</param>
		/// <param name="name">Name of element</param>
		/// <param name="val">Content/value</param>
		/// <returns>XmlElement as requested</returns>
		private XmlElement CreateElement(XmlDocument doc, string name, string val)
		{
			XmlElement elm = doc.CreateElement(name);
			if(val != null)
				elm.InnerText = val;
			return elm;
		}

		/// <summary>
		/// Check response from Gateway, update Message collection with status
		/// </summary>
		/// <param name="doc">XmlDocument with response from Gateway</param>
		private void CheckResponse(XmlDocument doc)
		{
			// Was login ok?
			string login = GetNodeValue(doc, "SESSION/LOGON");
			if(login != null)
			{
				if(!login.Equals("OK"))
				{
					// Not a valid login, fail all msgs
					foreach(int i in Messages.Keys) 
						Messages[i]._Status = MessageStatus.Failed;

					// Throw an appropriate exception
					string reason = GetNodeValue(doc, "SESSION/REASON");
					if(reason != null)
						throw new SMSException(reason);
					else 
						throw new SMSException("General error while processing response from SMS Gateway");
				} 
				else 
				{
					// Login OK

					XmlNode nodMsgList = doc.SelectSingleNode("SESSION/MSGLST");
					if(nodMsgList != null)
					{
						// Loop through msg list
						foreach(XmlNode n in nodMsgList.ChildNodes)
						{
							if(n.NodeType == XmlNodeType.Element && n.Name.Equals("MSG")) 
							{
								string id = GetNodeValue(n, "ID");
								int i = int.Parse(id);
								string status = GetNodeValue(n, "STATUS");
								if(status.Equals("OK")) 
								{
									Messages[i]._Status = MessageStatus.Sent;
									Messages[i]._Reference = GetNodeValue(n, "REF");
								}
								else
								{
									string info = GetNodeValue(n, "INFO");
									Messages[i]._Status = MessageStatus.Failed;
									Messages[i]._FailedReason = info;
								}
							}
						}
					}
				}			
			}
		}
		

		/// <summary>
		/// Check request for IncomingMessages/DeliveryReports from Gateway, 
		/// update Message collection or DeliveryReport collection
		/// </summary>
		/// <param name="doc">XmlDocument containing request from gateway</param>
		private XmlDocument CheckRequest(XmlDocument doc)
		{
			XmlDocument docResponse = new XmlDocument();
			XmlNode nodMsgList = doc.SelectSingleNode("MSGLST");
			if(nodMsgList != null)
			{
				// Loop through msg list
				foreach(XmlNode n in nodMsgList.ChildNodes)
				{
					if(n.NodeType == XmlNodeType.Element && n.Name.Equals("MSG")) 
					{
						string id = GetNodeValue(n, "ID");
						int i = int.Parse(id);

						// is this delivery report or incoming msg?
						string state = GetNodeValue(n, "STATE");
						if(state == null)
						{
							// Incoming Message
							ReceivedMessages.Add(i, new Message());
							ReceivedMessages[i].Text = GetNodeValue(n, "TEXT");
							ReceivedMessages[i].SenderNumber = GetNodeValue(n, "SND");
							ReceivedMessages[i].ReceiverNumber = GetNodeValue(n, "RCV");
							ReceivedMessages[i].Network = GetNodeValue(n, "NET");
							ReceivedMessages[i]._Address = GetNodeValue(n, "ADDRESS");

							// Check if position data is included
							XmlNode nodPosition = n.SelectSingleNode("POSITION");
							if(nodPosition != null)
							{
								// Get position request results
								ReceivedMessages[i]._Position = new PositionResult();
								switch(GetNodeValue(nodPosition, "STATUS"))
								{
									case "OK":
										ReceivedMessages[i]._Position._Status = PositionRequestStatus.Ok;
										XmlNode nodPosData = nodPosition.SelectSingleNode("POS");
										if(nodPosData != null)
										{
											ReceivedMessages[i]._Position._PositionData = new GSMPosition();
											ReceivedMessages[i]._Position._PositionData._Latitude = GetNodeValue(nodPosData, "LATITUDE");
											ReceivedMessages[i]._Position._PositionData._Longitude = GetNodeValue(nodPosData, "LONGITUDE");
											ReceivedMessages[i]._Position._PositionData._City = GetNodeValue(nodPosData, "CITY");
											ReceivedMessages[i]._Position._PositionData._Council = GetNodeValue(nodPosData, "COUNCIL");
											ReceivedMessages[i]._Position._PositionData._CouncilNumber = GetNodeValue(nodPosData, "COUNCILNUMBER", "").Equals("") ? -1 : int.Parse(GetNodeValue(nodPosData, "COUNCILNUMBER"));
											ReceivedMessages[i]._Position._PositionData._County = GetNodeValue(nodPosData, "COUNTY");
											ReceivedMessages[i]._Position._PositionData._Place = GetNodeValue(nodPosData, "PLACE");
											ReceivedMessages[i]._Position._PositionData._Radius = GetNodeValue(nodPosData, "RADIUS", "").Equals("") ? -1 : int.Parse(GetNodeValue(nodPosData, "RADIUS"));
											ReceivedMessages[i]._Position._PositionData._SubPlace = GetNodeValue(nodPosData, "SUBPLACE");
											ReceivedMessages[i]._Position._PositionData._ZipCode = GetNodeValue(nodPosData, "ZIPCODE", "").Equals("") ? -1 : int.Parse(GetNodeValue(nodPosData, "ZIPCODE"));
										}
										break;
									default:
										ReceivedMessages[i]._Position._Status = PositionRequestStatus.Failed;
										ReceivedMessages[i]._Position._FailedReason = GetNodeValue(nodPosition, "INFO");
										break;
								}
							}

						} 
						else 
						{
							DeliveryReports.Add(i, new DeliveryReport());
							DeliveryReports[i].State = GetNodeValue(n, "STATE");
							DeliveryReports[i].Reference = GetNodeValue(n, "REF");
							DeliveryReports[i].ReceiverNumber = GetNodeValue(n, "RCV");
							string sDeliveryTime = GetNodeValue(n, "DELIVERYTIME");
							if(sDeliveryTime!= null && sDeliveryTime.Length >0)
								DeliveryReports[i].DeliveredDate = Convert.ToDateTime(sDeliveryTime);
						}
					}
				}

				// Build Response
				XmlElement elmMsgList = docResponse.CreateElement("MSGLST");
				foreach(int i in ReceivedMessages.Keys) 
				{
					XmlElement elmMsg = docResponse.CreateElement("MSG");
					elmMsg.AppendChild(CreateElement(docResponse, "ID", i.ToString()));
					elmMsg.AppendChild(CreateElement(docResponse, "STATUS", "OK"));
					elmMsgList.AppendChild(elmMsg);
				}
				foreach(int i in DeliveryReports.Keys) 
				{
					XmlElement elmMsg = docResponse.CreateElement("MSG");
					elmMsg.AppendChild(CreateElement(docResponse, "ID", i.ToString()));
					elmMsg.AppendChild(CreateElement(docResponse, "STATUS", "OK"));
					elmMsgList.AppendChild(elmMsg);
				}
				docResponse.AppendChild(elmMsgList);
			} 
			return docResponse;
		}

        /// <summary>
        /// Return value of given node as by xpath expression, or null if not found
        /// </summary>
        /// <param name="doc">XmlNode to search from</param>
        /// <param name="xpath">XPath expression of desired node</param>
        /// <returns>Content of node as string or null if not found</returns>
        private string GetNodeValue(XmlNode doc, string xpath)
        {
            return GetNodeValue(doc, xpath, null);

        }

        private string GetNodeValue(XmlNode doc, string xpath, string defaultValue)
        {
            XmlNode node = doc.SelectSingleNode(xpath);
            if (node != null)
                return node.InnerText;
            else
                return defaultValue;
        }

		/// <summary>
		/// Send a HTTP Post request
		/// </summary>
		/// <param name="doc">XmlDocument to send</param>
		/// <param name="url">Destination URL</param>
		/// <returns>XmlDocument with response</returns>
		private XmlDocument HttpPost(XmlDocument doc, string url)
		{
			try
			{
				HttpWebRequest httpReq = (HttpWebRequest) WebRequest.Create(url);

				httpReq.ContentType = "application/xml";
				httpReq.Timeout = 1000 * _ConnectTimeout;
				httpReq.Method = "POST";
				if(_Proxy == null)
					_Proxy =(IWebProxy)httpReq.Proxy;
				else
					httpReq.Proxy = _Proxy;
				httpReq.KeepAlive = false;
				
				UTF8Encoding encoding=new UTF8Encoding();
				byte[] bytes=encoding.GetBytes("<?xml version=\"1.0\"?>" + doc.OuterXml + "\n");

				Encoding ISO8859 = Encoding.GetEncoding("ISO-8859-1"); 
				Encoding UTF8 = Encoding.GetEncoding("UTF-8"); 
				bytes = Encoding.Convert(UTF8, ISO8859 ,bytes); 
				httpReq.ContentLength = bytes.Length+1;

				string s = ISO8859.GetString(bytes, 0, bytes.Length);
                
				HttpWebResponse httpResponse = null;				
				XmlDocument xmlResult = null;
				try
				{
					Stream dataStream = null;
					try
					{
						dataStream = httpReq.GetRequestStream();
						dataStream.Write(bytes,0,bytes.Length);
						dataStream.WriteByte(0x00);
					}
					finally
					{
						if(dataStream != null)
							dataStream.Close();
					}
					httpResponse = (HttpWebResponse) httpReq.GetResponse();
					/*
										byte[] buffer = new byte[2000];
										Stream str = httpResponse.GetResponseStream();
										int i = str.Read(buffer, 0, 2000);
										Encoding enc = Encoding.GetEncoding("ISO-8859-1");
										string xml = enc.GetString(buffer, 0, i);
					*/

					XmlTextReader xmlResultReader  = new XmlTextReader(httpResponse.GetResponseStream());
					xmlResult = new XmlDocument();
					xmlResult.Load(xmlResultReader);
//					string s = xmlResult.OuterXml;
				}
				catch(Exception e)
				{
					String error = e.Message;
					throw;
				}
				finally
				{
					if(httpResponse != null)
						httpResponse.Close();
				}
				return xmlResult;
			}
			catch(Exception)
			{
				throw;
			}
		}

	}
}
