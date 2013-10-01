using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace PSWinCom.Gateway.Client
{
    [XmlRoot("MSG")]
    public class Message
    {
        internal int NumInSession { get; set; }

        [XmlElement("TEXT")]
        public string Text { get; set; }
        [XmlElement("RCV")]
        public string ReceiverNumber { get; set; }
        [XmlElement("SND")]
        public string SenderNumber { get; set; }

        public int Tariff { get; set; }

        private string _useryReference;
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
}
