using System;

namespace PSWinCom.Gateway.Client.MessageFormatters
{
	/// <summary>
	/// Well known values used when generating WSP (Wireless Session Protocol) headers
	/// </summary>
	public class WSP
	{
		public const byte TRANSACTIONID_CONNECTIONLESSWSP = 0x25;

		public const byte PDUTYPE_PUSH = 0x06;

		public const byte HEADER_CONTENTLENGTH = 0x8D;

		public static byte[] HEADER_CONTENTTYPE_application_vnd_wap_sic_utf_8 = new byte[] {0x03,0xAE,0x81,0xEA};

		public const byte HEADER_APPLICATIONTYPE = 0xaf;
		public const byte HEADER_APPLICATIONTYPE_x_wap_application_id_w2 = 0x82;

		public static byte[] HEADER_PUSHFLAG = new byte[] {0xB4, 0x84};

	}
}
