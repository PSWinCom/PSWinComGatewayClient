using System;

namespace PSWinCom.Gateway.MMS.Client
{
    /// <summary>
    /// A DeliveryReport object holds all the settings and values of a Delivery Report.
    /// </summary>
    public class DeliveryReport
    {
        private string _State;
        private string _ReceiverNumber;
        private string _SenderNumber;
        private string _Reference;
        private string _Network;
        private DateTime _DeliveredDate;

        /// <summary>
        /// Default constructor
        /// </summary>
        public DeliveryReport()
        {
            _State = null;
            _ReceiverNumber = null;
            _SenderNumber = null;
            _Reference = null;
            _Network = null;
            _DeliveredDate = DateTime.Now;
        }

        #region Accessors
        /// <summary>
        /// This property indicates the final state of the message as given by the Network operator. Typical values are:
        /// RETRIEVED: Message was successfully delivered to destination. 
        /// REJECTED: Message was rejected by the receiver. 
        /// ... some others
        /// </summary>
        public string State { get { return _State; } set { _State = value; } }
        /// <summary>
        /// The number of the receiver that the message was originally sent to.
        /// </summary>
        public string ReceiverNumber { get { return _ReceiverNumber; } set { _ReceiverNumber = value; } }
        /// <summary>
        /// The sender number.
        /// </summary>
        public string SenderNumber { get { return _SenderNumber; } set { _SenderNumber = value; } }
        /// <summary>
        /// A unique reference value that corresponds to the Reference value from the Message object
        /// from the particular Message that this Delivery Report is related to.
        /// </summary>
        public string Reference { get { return _Reference; } set { _Reference = value; } }
        /// <summary>
        /// The Date and Time when the message was delivered to the handset (in local MMSC time)
        /// </summary>
        public DateTime DeliveredDate { get { return _DeliveredDate; } set { _DeliveredDate = value; } }
        #endregion
    }
}
