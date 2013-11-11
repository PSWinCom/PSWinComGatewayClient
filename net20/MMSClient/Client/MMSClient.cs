using System;
using System.Xml;
using System.Collections;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using ICSharpCode.SharpZipLib.GZip;

namespace PSWinCom.Gateway.MMS.Client
{
	/// <summary>
	/// MMS Client implementation for PSWinCom MMS Gateway
	/// </summary>
	public class MMSClient
	{

		#region Nested Message collection class
		public class MessageCollection : DictionaryBase
		{
			public MessageCollection() : base() 
			{
			}

			public MMSMessage this[int key] { get { return( (MMSMessage) Dictionary[key] ); } set { Dictionary[key] = value; } }
			public ICollection Keys { get { return( Dictionary.Keys ); } }
			public ICollection Values { get { return( Dictionary.Values ); } }

			public void Add(int key, MMSMessage value)  
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

				Object resourceStream = GetType().Assembly.GetManifestResourceStream("PSWinCom.Gateway.MMS.Client." + resourcePath);
				if (resourceStream == null)
					resourceStream = fallbackResolver.GetEntity(
						absoluteUri, role, ofObjectToReturn);

				return resourceStream;
			}
		}

		#endregion
		
		
		private string _Username;
		private string _Password;
		private string _PrimaryGateway;
		private string _SecondaryGateway;
		private string _SessionData;
		private string _AffiliateProgram;
		private int	 _ConnectTimeout;
		private IWebProxy _Proxy;
		private bool	_BulkWithIdenticalContent;

		/// <summary>
		/// Collection of outgoing Message objects. Add your Message objects to this collection before
		/// sending the messages.
		/// </summary>
		public MessageCollection Messages;
		/// <summary>
		/// Collection of incoming Message objects. This collection will contain the received messages
		/// after running HandleIncomingMessages. The collection is emptied by the HandleIncomingMessages() method.
		/// </summary>
		public MessageCollection ReceivedMessages;
		/// <summary>
		/// Collection of received DeliveryReport objects. This collection will contain the received delivery reports
		/// after running HandleIncomingMessages. The collection is emptied by the HandleIncomingMessages() method.
		/// </summary>
		public DeliveryReportCollection DeliveryReports;

		#region Accessors
		/// <summary>
		/// Username on MMS Gateway
		/// </summary>
		public string Username { get { return _Username; } set { _Username = value; } }		
		/// <summary>
		/// Password on MMS Gateway
		/// </summary>
		public string Password { get { return _Password; } set { _Password = value; } }		
		/// <summary>
		/// PSWinCom MMS Gateway Primary address
		/// </summary>
		public string PrimaryGateway { get { return _PrimaryGateway; } set { _PrimaryGateway = value; } }		
		/// <summary>
		/// PSWinCom MMS Gateway Secondary address
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
		/// If this property is set, the MMSClient will send the content to the MMS Gateway only once per session. That
		/// will result in substantial lower transmission time for large sessions. When sending bulk MMS with identical
		/// content you should therefore set this property to true. Default value is false.
		/// </summary>
		public bool BulkWithIdenticalContent { get { return _BulkWithIdenticalContent; } set { _BulkWithIdenticalContent = value; } }	
		#endregion

		/// <summary>
		/// Default constructor
		/// </summary>
		public MMSClient()
		{
			Messages = new MessageCollection();
			ReceivedMessages = new MessageCollection();
			DeliveryReports = new DeliveryReportCollection();
			_Username = null;
			_Password = null;
			_PrimaryGateway = null;
			_SecondaryGateway = null;
			_ConnectTimeout = 10;
			_BulkWithIdenticalContent = false;
		}

		/// <summary>
		/// Send all messages in the Messages collection. This operation will block while communicating
		/// with the Gateway. After it has been completed, the MMSMessage objects in the Messages collection
		/// is updated with the Status, (and if applicable, also the Reference or FailedReason properties)
		/// </summary>
		public void SendMessages()
		{

			XmlDocument doc = GetDocumentXml();
			XmlDocument docResponse = HttpPost(doc, PrimaryGateway);
			CheckResponse(docResponse);
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
			XmlEmbeddedResourceResolver res = new XmlEmbeddedResourceResolver();
			docRequest.XmlResolver = res;
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
			if(_AffiliateProgram != null && _AffiliateProgram.Length > 0)
				elmSession.AppendChild(CreateElement(doc, "AP", _AffiliateProgram));
			if(_SessionData != null && _SessionData.Length > 0)
				elmSession.AppendChild(CreateElement(doc, "SD", _SessionData));

			XmlElement elmMsgList = doc.CreateElement("MSGLST");
			bool bFirstElement = true;
			foreach(int i in Messages.Keys)
			{
				if(_BulkWithIdenticalContent && !bFirstElement)
					elmMsgList.AppendChild(GetMessageXml(doc, Messages[i], i, false));
				else
					elmMsgList.AppendChild(GetMessageXml(doc, Messages[i], i, true));
				bFirstElement = false;
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
		private XmlElement GetMessageXml(XmlDocument doc, MMSMessage m, int id, bool includeContent)
		{
			XmlElement elmMsg = doc.CreateElement("MSG");
			elmMsg.AppendChild(CreateElement(doc, "ID", id.ToString()));
			if(m.Network != null && m.Network.Length > 0)
				elmMsg.AppendChild(CreateElement(doc, "NET", m.Network));
			elmMsg.AppendChild(CreateElement(doc, "TARIFF", m.Tariff.ToString()));
			elmMsg.AppendChild(CreateElement(doc, "TEXT", m.Subject));
			elmMsg.AppendChild(CreateElement(doc, "OP", "13"));
			if(m.RequestReceipt)
				elmMsg.AppendChild(CreateElement(doc, "RCPREQ", "Y"));
			elmMsg.AppendChild(CreateElement(doc, "SND", m.SenderNumber));
			elmMsg.AppendChild(CreateElement(doc, "RCV", m.ReceiverNumber));
			if(includeContent)
				elmMsg.AppendChild(CreateElement(doc, "MMSFILE", Convert.ToBase64String(m.GetCompressed())));
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
						throw new MMSException(reason);
					else 
						throw new MMSException("General error while processing response from SMS Gateway");
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
							ReceivedMessages.Add(i, new MMSMessage());
							ReceivedMessages[i].Subject = GetNodeValue(n, "TEXT");
							ReceivedMessages[i].SenderNumber = GetNodeValue(n, "SND");
							ReceivedMessages[i].ReceiverNumber = GetNodeValue(n, "RCV");
							ReceivedMessages[i].Network = GetNodeValue(n, "NET");
							ReceivedMessages[i]._Address = GetNodeValue(n, "ADDRESS");
							ReceivedMessages[i].Parts = GetParts(GetNodeValue(n, "MMSFILE"));
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

			XmlNode node = doc.SelectSingleNode(xpath);
			if(node != null)
				return node.InnerText;
			else
				return null;

		}

		/// <summary>
		/// Decode base 64 data, unzip binary stream and retrieve each part
		/// </summary>
		/// <param name="base64data">Base 64 encoded data containing MMS message in ZIP file format</param>
		/// <returns></returns>
		private MMSMessage.MessagePartCollection GetParts(string base64data)
		{

			MMSMessage.MessagePartCollection coll = new MMSMessage.MessagePartCollection();

			MemoryStream ms = new MemoryStream(Convert.FromBase64String(base64data));

			// Extract files and add them
			ZipInputStream s = new ZipInputStream(ms);
			ZipEntry theEntry;
			int i = 0;
			while ((theEntry = s.GetNextEntry()) != null) 
			{
				MMSMessagePart part = new MMSMessagePart();
				part.PartId = i;
				string fileName = theEntry.Name;

				if (fileName != String.Empty) 
				{
					part.Name = fileName;
					//Read Part into memorystream
					int size = 2048;
					int len = 0;
					MemoryStream mspart = new MemoryStream(size);
					byte[] partData = new byte[size];
					while (true) 
					{
						size = s.Read(partData, 0, partData.Length);
						len += size;
						if (size > 0) 
						{
							mspart.Write(partData, 0, size);
						} 
						else 
						{
							break;
						}
					}
					part.Data = new byte[len];
					mspart.Position = 0; // Reset position and read into byte buffer
					mspart.Read(part.Data, 0, len);
					mspart.Close();
				}
				coll.Add(i, part);
				i++;
			}
			s.Close();

			return coll;
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
					string s = xmlResult.OuterXml;
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
