using System;
using System.Collections;
using System.IO;
using System.Text;

namespace PSWinCom.Gateway.Client.MessageFormatters
{
	// Allowed values of the action attribute of the indication tag
	public enum ServiceIndicationAction {NotSet, SignalNone, SignalLow, SignalMedium, SignalHigh, Delete};

	/// <summary>
	/// Encapsulates the Service Indication WAP Push instruction.
	/// Full documentation can be found at http://www.openmobilealliance.org/tech/affiliates/wap/wap-167-serviceind-20010731-a.pdf?doc=wap-167-serviceind-20010731-a.pdf
	/// </summary>
	public class ServiceIndication
	{
		// Well known DTD token
		public const byte DOCUMENT_DTD_ServiceIndication = 0x05;			// ServiceIndication 1.0 Public Identifier
	
		// Tag Tokens
		public const byte TAGTOKEN_si = 0x5;
		public const byte TAGTOKEN_indication = 0x6;
		public const byte TAGTOKEN_info = 0x7;
		public const byte TAGTOKEN_item = 0x8;
		
		// Attribute Tokens
		public const byte ATTRIBUTESTARTTOKEN_action_signal_none = 0x5;
		public const byte ATTRIBUTESTARTTOKEN_action_signal_low = 0x6;
		public const byte ATTRIBUTESTARTTOKEN_action_signal_medium = 0x7;
		public const byte ATTRIBUTESTARTTOKEN_action_signal_high = 0x8;
		public const byte ATTRIBUTESTARTTOKEN_action_signal_delete = 0x9;
		public const byte ATTRIBUTESTARTTOKEN_created = 0xA;
		public const byte ATTRIBUTESTARTTOKEN_href = 0xB;
		public const byte ATTRIBUTESTARTTOKEN_href_http = 0xC;			// http://
		public const byte ATTRIBUTESTARTTOKEN_href_http_www = 0xD;	// http://www.
		public const byte ATTRIBUTESTARTTOKEN_href_https = 0xE;			// https://
		public const byte ATTRIBUTESTARTTOKEN_href_https_www = 0xF;	// https://www.
		public const byte ATTRIBUTESTARTTOKEN_si_expires = 0x10;
		public const byte ATTRIBUTESTARTTOKEN_si_id = 0x11;
		public const byte ATTRIBUTESTARTTOKEN_class = 0x12;

		// Attribute Value Tokens
		public const byte ATTRIBUTEVALUETOKEN_com = 0x85;						// .com/
		public const byte ATTRIBUTEVALUETOKEN_edu = 0x86;						// .edu/
		public const byte ATTRIBUTEVALUETOKEN_net = 0x87;						// .net/
		public const byte ATTRIBUTEVALUETOKEN_org = 0x88;						// .org/

		private static Hashtable hrefStartTokens;

		public string Href;
		public string Text;
		public DateTime CreatedAt;
		public DateTime ExpiresAt;
		public ServiceIndicationAction Action;
		
		static ServiceIndication()
		{
			hrefStartTokens = new Hashtable();
			hrefStartTokens.Add("https://www.", ATTRIBUTESTARTTOKEN_href_https_www);
			hrefStartTokens.Add("http://www.", ATTRIBUTESTARTTOKEN_href_http_www);
			hrefStartTokens.Add("https://", ATTRIBUTESTARTTOKEN_href_https);
			hrefStartTokens.Add("http://", ATTRIBUTESTARTTOKEN_href_http);
		}

		public ServiceIndication(string href, string text, ServiceIndicationAction action)
		{
			this.Href = href;
			this.Text = text;
			this.Action = action;
		}	

		public ServiceIndication(string href, string text, DateTime createdAt, DateTime expiresAt)
			: this(href, text, ServiceIndicationAction.NotSet)
		{
			this.CreatedAt = createdAt;
			this.ExpiresAt = expiresAt;
		}	

		public ServiceIndication(string href, string text, DateTime createdAt, DateTime expiresAt, ServiceIndicationAction action)
			: this (href, text, action)
		{
			this.CreatedAt = createdAt;
			this.ExpiresAt = expiresAt;
		}

		/// <summary>
		/// Generates a byte array comprising the encoded Service Indication
		/// </summary>
		/// <returns>The encoded body</returns>
		public byte[] GetWBXMLBytes()
		{
			MemoryStream stream = new MemoryStream();

			// wbxml headers
			stream.WriteByte(WBXML.VERSION_1_1);
			stream.WriteByte(DOCUMENT_DTD_ServiceIndication);
			stream.WriteByte(WBXML.CHARSET_UTF_8);
			stream.WriteByte(WBXML.NULL);

			// start xml doc
			stream.WriteByte(WBXML.SetTagTokenIndications(TAGTOKEN_si, false, true));
			stream.WriteByte(WBXML.SetTagTokenIndications(TAGTOKEN_indication, true , true));

			// href attribute
			// this attribute has some well known start tokens that 
			// are contained within a static hashtable. Iterate through
			// the table and chose the token.
			int i = 0;
			byte hrefTagToken = ATTRIBUTESTARTTOKEN_href;
			foreach (string startString in hrefStartTokens.Keys)
			{
				if (this.Href.StartsWith(startString))
				{
					hrefTagToken = (byte)hrefStartTokens[startString];
					i = startString.Length;
					break;
				}
			}
			stream.WriteByte(hrefTagToken);

			WriteInlineString(stream, this.Href.Substring(i));

            if (this.Action != ServiceIndicationAction.NotSet)
				stream.WriteByte(GetActionToken(this.Action));

			stream.WriteByte(WBXML.TAGTOKEN_END);
			
			WriteInlineString(stream, this.Text);

			stream.WriteByte(WBXML.TAGTOKEN_END);
			stream.WriteByte(WBXML.TAGTOKEN_END);
			
			return stream.ToArray();
		}

		/// <summary>
		/// Gets the token for the action attribute
		/// </summary>
		/// <param name="action">Interruption level instruction to the handset</param>
		/// <returns>well known byte value for the action attribute</returns>
		protected byte GetActionToken(ServiceIndicationAction action)
		{
			switch (action)
			{
				case ServiceIndicationAction.Delete :
					return ATTRIBUTESTARTTOKEN_action_signal_delete;
				case ServiceIndicationAction.SignalHigh :
					return ATTRIBUTESTARTTOKEN_action_signal_high;
				case ServiceIndicationAction.SignalLow :
					return ATTRIBUTESTARTTOKEN_action_signal_low;
				case ServiceIndicationAction.SignalMedium : 
					return ATTRIBUTESTARTTOKEN_action_signal_medium;
				default :
					return ATTRIBUTESTARTTOKEN_action_signal_none;
			}
		}

		/// <summary>
		/// Encodes an inline string into the stream using UTF8 encoding
		/// </summary>
		/// <param name="stream">The target stream</param>
		/// <param name="text">The text to write</param>
		protected void WriteInlineString(MemoryStream stream, string text)
		{
			// indicate that the follow bytes comprise a string
			stream.WriteByte(WBXML.TOKEN_INLINE_STRING_FOLLOWS);

			// write character bytes
			byte[] buffer = Encoding.UTF8.GetBytes(text);
			stream.Write(buffer, 0, buffer.Length);

			// end is indicated by a null byte
			stream.WriteByte(WBXML.NULL);
		}
		/// <summary>
		/// Encodes the DateTime to the stream.
		/// DateTimes are encoded as Opaque Data with each number in the string represented
		/// by its 4-bit binary value
		/// eg: 1999-04-30 06:40:00
		/// is encoded as 199904300640.
		/// Trailing zero values are not included.
		/// </summary>
		/// <param name="stream">Target stream</param>
		/// <param name="date">DateTime to encode</param>
		protected void WriteDate(MemoryStream stream, DateTime date)
		{
			byte[] buffer = new byte[7];
			
			buffer[0] = (byte)(date.Year / 100);
			buffer[1] = (byte)(date.Year % 100);
			buffer[2] = (byte)date.Month;
			buffer[3] = (byte)date.Day;

			int dateLength = 4;

			if (date.Hour > 0)
			{
				buffer[4] = (byte)date.Hour;
				dateLength = 5;
			}

			if (date.Minute > 0)
			{
				buffer[5] = (byte)date.Minute;
				dateLength = 6;
			}

			if (date.Second > 0)
			{
				buffer[6] = (byte)date.Second;
				dateLength = 7;
			}
			
			// write to stream
			stream.WriteByte(WBXML.TOKEN_OPAQUEDATA_FOLLOWS);
			stream.WriteByte((byte)dateLength);
			stream.Write(buffer, 0, dateLength);
		}

	}
}
