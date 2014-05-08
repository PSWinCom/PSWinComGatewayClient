using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSWinCom.Gateway.Client
{
    /// <summary>
    /// Representation of the MSG element in a XML API request to PSWinCom Gateway. https://wiki.pswin.com/Gateway%20XML%20API.ashx#Message:_MSG_11
    /// </summary>
    public abstract class Message
    {
        public Message()
        {

        }

        private MessageType? _type;
        private string _useryReference;
        internal int NumInSession { get; set; }

        /// <summary>
        /// Contents of the message. For MMS this will be used as subject. Characters in this
        /// text should exist in the Latin1 (ISO-8859-1) characterset, any other characters may
        /// not be viewed properly on the receivers handset.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The receiver of the message. Must be a valid msIsdn including countrycode without leading zeros or + sign.
        /// </summary>
        public string ReceiverNumber { get; set; }

        /** 
         * <summary>
         * <para>This is the sender of the message.</para>
         * </summary>
         * <value>
         * <list type="bullet">
         * <item>
         * <description>4 or 5 digit short code (digits only)</description>
         * </item>
         * <item>
         * <description>a valid msIsdn including countrycode without leading zeros or + sign</description>
         * </item>
         * <item>
         * <description>an up to 11 characters long alpha numeric string only containing international characters and space</description>
         * </item>
         * </list>
         * </value>
        */
        public string SenderNumber { get; set; }

        public string ShortCode { get; set; }
        public int Tariff { get; set; }
        public string CpaTag { get; set; }

        public bool RequestReceipt { get; set; }
        
        public TimeSpan? TimeToLive { get; set; }
        public DateTime? DeliveryTime { get; set; }

        public virtual MessageType? Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
            }
        }

        public string UserReference
        {
            get
            {
                if (String.IsNullOrEmpty(_useryReference))
                    return NumInSession.ToString();
                return _useryReference;
            }
            set
            {
                _useryReference = value;
            }
        }
    }

    public enum MessageType
    {
        Text = 1,
        Ringtone = 2,
        OperatorLogo = 3,
        CallerGroupGraphic = 4,
        Picture = 5,
        vCard = 6,
        vCalendar = 7,
        RawBinaryUDH = 8,
        Unicode = 9,
        WapPush = 10,
        OTABookmark = 11,
        OTASettings = 12,
        MMSMessage = 13
    }
}
