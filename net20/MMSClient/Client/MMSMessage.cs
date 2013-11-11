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
	/// Enumeration of the various status values a message may have. Only applies to outgoing messages
	/// </summary>
	public enum MessageStatus
	{
		/// <summary>
		/// Indicates a newly added message that has not yet been sent.
		/// </summary>
		New,
		/// <summary>
		/// Indicates a message that has been successfully sent
		/// </summary>
		Sent,
		/// <summary>
		/// Indicates a message that has been tried sent, but failed
		/// </summary>
		Failed
	}


	/// <summary>
	/// An MMSMessage object holds all the settings and values of an MMS message. This class is used both for incoming
	/// outgoing messages, but not all properties are valid for incoming messages.
	/// </summary>
	public class MMSMessage
	{

		#region Nested MMSMessagePart collection class
		public class MessagePartCollection : DictionaryBase
		{
			public MessagePartCollection() : base() 
			{
			}

			public MMSMessagePart this[int key] { get { return( (MMSMessagePart) Dictionary[key] ); } set { Dictionary[key] = value; } }
			public ICollection Keys { get { return( Dictionary.Keys ); } }
			public ICollection Values { get { return( Dictionary.Values ); } }

			public void Add(int key, MMSMessagePart value)  
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
			
		private int		_MessageId;
		private string	_Subject;
		private string	_ReceiverNumber;
		private string	_SenderNumber;
		internal  MessageStatus	_Status;
		private bool		_RequestReceipt;
		internal string	_Reference;
		private string	_Network;
		private int		_Tariff;
		internal  string	_FailedReason;
		internal string	_Address;

		public MessagePartCollection Parts;
		
		/// <summary>
		/// Default constructor
		/// </summary>
		public MMSMessage()
		{
			_Subject = null;
			_ReceiverNumber = null;
			_SenderNumber = null;
			_MessageId = -1;
			_RequestReceipt = false;	
			_Status = MessageStatus.New;
			_Reference = null;
			_Network = null;
			_Tariff = 0;
			_FailedReason = null;
			Parts = new MessagePartCollection();
		}

		#region Accessors
		/// <summary>
		/// Set to true to request a receipt (Delivery Report) for this message. The delivery report
		/// will either be available on the account web or forwarded to your application depending
		/// on your account settings on the MMS Gateway.
		/// </summary>
		public bool RequestReceipt { get { return _RequestReceipt; } set { _RequestReceipt = value; } }
		/// <summary>
		/// The subject of the MMS message to send
		/// </summary>
		public string Subject { get { return _Subject; } set { _Subject = value; } }		
		/// <summary>
		/// The number of the receiver. The number must be an internationally formatted number. That is
		/// a number that includes country prefix. Any spaces or leading "00" and "+" will be removed and should thus
		/// be avoided.
		/// </summary>
		public string ReceiverNumber { get { return _ReceiverNumber; } set { _ReceiverNumber = value; } }	
		/// <summary>
		/// The sender number of the MMS. This will normally be overriden by MMS Gateway to reflect the CPA shortnumer
		/// in use.
		/// </summary>
		public string SenderNumber { get { return _SenderNumber; } set { _SenderNumber = value; } }	
		/// <summary>
		/// Indicates the status of the message. The MessageStatus enumeration holds the various states that
		/// may occur. This property is read-only and is updated only as a result of a SendMessages() operation.
		/// </summary>
		public MessageStatus Status { get { return _Status; } }	
		/// <summary>
		/// If RequestReceipt is set to true and your Gateway account has been enabled for delivery report
		/// forwarding, then the Reference property will hold a unique Reference value for this particular
		/// message after successfully issuing a SendMessages() operation. This Reference value can
		/// later be used to correlate the Message with a Delivery Report received at a later time.
		/// This property is read-only.
		/// </summary>
		public string Reference { get { return _Reference; } }	
		/// <summary>
		/// Can be set to indicate a specific routing of this message. Should only be used for
		/// sending of Premium MMS messages where specific routing is required. Other usage of
		/// this may result in the message not being delivered.
		/// </summary>
		public string Network { get { return _Network; } set { _Network = value; } }
		/// <summary>
		/// Indicated the Premium Price in cents/ører for a message sent as a Premium MMS. This
		/// requires that the account has been enabled for CPA/Premium MMS usage.
		/// </summary>
		public int Tariff { get { return _Tariff; } set { _Tariff = value; } }
		/// <summary>
		/// If a message was not accepted by the MMS Gateway you may find a more specific reason
		/// for this by checking this property. It will be empty for successfully submitted messages.
		/// This property is read-only and is updated only as a result of a SendMessages() operation.
		/// </summary>
		public string FailedReason { get { return _FailedReason; } }	
		/// <summary>
		/// For incoming messages the Gateway may include information about the name and address of the
		/// person that is registered to the SenderNumber. This is a value-added feature that will
		/// require a separate agreement with PSWinCom.
		/// </summary>
		public string Address { get { return _Address; } }	
		#endregion


		/// <summary>
		/// Pack up the content of this MMSMessage as a zip file for suitable transmission
		/// to the MMS Gateway
		/// </summary>
		/// <returns>Byte array containing zipped data</returns>
		internal byte[] GetCompressed()
		{
            using (MemoryStream msZipOutput = new MemoryStream())
            {
                ZipOutputStream os = new ZipOutputStream(msZipOutput) { IsStreamOwner = false, UseZip64 = UseZip64.Off };
                foreach (int i in Parts.Keys)
                {
                    MMSMessagePart part = Parts[i];
                    os.PutNextEntry(new ZipEntry(part.Name));
                    os.Write(part.Data, 0, part.Data.Length);
                    os.CloseEntry();
                }
                os.Finish();
                os.Close();
                byte[] b = new byte[msZipOutput.Length];
                msZipOutput.Position = 0;
                msZipOutput.Read(b, 0, b.Length);
                msZipOutput.Close();
                return b;
            }
		}

		/// <summary>
		/// Save all content parts to the given path using original filenames for each part
		/// </summary>
		/// <param name="path">A valid path to a directory</param>
		public void SaveParts(string path)
		{
			foreach(int i in Parts.Keys) 
				Parts[i].Save(path);
		}

		/// <summary>
		/// Save all MMS message parts as one compressed file. Suitable for packing content
		/// for storing and later sending.
		/// </summary>
		/// <param name="filename">Filename to save as.</param>
		public void SaveCompressed(string filename)
		{
            if (!String.IsNullOrEmpty(filename))
                using (FileStream streamWriter = File.Create(filename))
                    SaveCompressed(streamWriter);
		}

        /// <summary>
        /// Save all MMS message parts as a zip to a stream. Suitable for packing content
        /// for storing in database or such,
        /// </summary>
        /// <param name="stream">Stream to save to.</param>
        public void SaveCompressed(System.IO.Stream stream)
        {
            if (Parts.Count == 0)
                throw new MMSException("No content to save.");

            byte[] data = GetCompressed();

            if (data.Length > 0)
                stream.Write(data, 0, data.Length);
        }

		/// <summary>
		/// Loads compressed MMS message parts from file. Existing content in
		/// MMSMessage object will be overwritten.
		/// </summary>
		/// <param name="filename">Filename to load from</param>
        public void LoadCompressed(string filename)
        {
            try
            {
                using (FileStream fileStream = File.Open(filename, FileMode.Open, FileAccess.Read))
                    LoadCompressed(fileStream);
            }
            catch (Exception e)
            {
                throw new MMSException("Failed to load content from file", e);
            }
        }

        public void LoadCompressed(Stream stream)
        {
            Parts.Clear();
            ZipInputStream s = new ZipInputStream(stream); // Do NOT dispose, as it will close stream aswell, wich might not be what user wants
            ZipEntry theEntry;
            int i = 0;
            while ((theEntry = s.GetNextEntry()) != null)
            {
                MMSMessagePart part = new MMSMessagePart { 
                    PartId = i 
                };

                string fileName = theEntry.Name;

                if (fileName != String.Empty)
                {
                    part.Name = fileName;
                    int size = 2048;
                    int len = 0;
                    using (MemoryStream mspart = new MemoryStream(size))
                    {
                        byte[] partData = new byte[size];
                        while (true)
                        {
                            size = s.Read(partData, 0, partData.Length);
                            len += size;
                            if (size > 0)
                                mspart.Write(partData, 0, size);
                            else
                                break;
                        }
                        part.Data = new byte[len];
                        mspart.Position = 0;
                        // Reset position and read into byte buffer
                        mspart.Read(part.Data, 0, len);
                        mspart.Close();
                    }
                }
                Parts.Add(i, part);
                i++;
            }
            s.Close();
        }
    }
}
