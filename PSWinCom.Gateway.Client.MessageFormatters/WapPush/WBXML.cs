using System;

namespace PSWinCom.Gateway.Client.MessageFormatters
{
	/// <summary>
	/// Series of well known constants and static byte values used when encoding
	/// a document to WBXML
	/// </summary>
	public class WBXML
	{
		public const byte NULL = 0x00;

		public const byte VERSION_1_1 = 0x01;
		public const byte VERSION_1_2 = 0x02;

		public const byte CHARSET_UTF_8 = 0x6A;

		public const byte TAGTOKEN_END = 0x01;
		public const byte TOKEN_INLINE_STRING_FOLLOWS = 0x03;
		public const byte TOKEN_OPAQUEDATA_FOLLOWS = 0xC3;

		public static byte SetTagTokenIndications(byte token, bool hasAttributes, bool hasContent)
		{
			if (hasAttributes)
				token |= 0xC0;
			if (hasContent)
				token |= 0x40;

			return token;
		}
	}
}
