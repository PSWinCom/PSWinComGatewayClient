using System;
using System.Globalization;
using System.Xml.Serialization;

namespace PSWinCom.Gateway.Client
{

	/// <summary>
	/// Enumeration of the various message types that are supported.
	/// </summary>
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
		OTASettings = 12
	}

	/// <summary>
	/// Enumeration of the various status values a message may have. Only applies to outgoing messages
	/// </summary>
	public enum MessageStatus
	{
		New,
		Sent,
		Failed
	}

	/// <summary>
	/// Enumeration of settings for replacing SMS messages in Inbox.
	/// </summary>
	public enum MessageReplaceSet
	{
		NoReplace = 0,
		ReplaceSet1 = 1,
		ReplaceSet2 = 2,
		ReplaceSet3 = 3,
		ReplaceSet4 = 4,
		ReplaceSet5 = 5,
		ReplaceSet6 = 6,
		ReplaceSet7 = 7
	}

	/// <summary>
	/// Enumeration of the various status values a position request may have. Applies only to incoming
	/// messages with positioning enabled og Gateway.
	/// </summary>
	public enum PositionRequestStatus
	{
		Ok,
		Failed
	}

	/// <summary>
	/// A Message object holds all the settings and values of an SMS message. This class is used both for incoming
	/// outgoing messages, but not all properties are valid for incoming messages.
	/// </summary>
	public class Message
	{
			
		private string	_Text;
		private string	_ReceiverNumber;
		private string	_SenderNumber;
		internal  MessageStatus	_Status;
		private short		_MessageClass;
		private bool		_RequestReceipt;
		private MessageType _MessageType;
		internal string	_Reference;
		private string	_Network;
		internal string	_Address;
		private int		_Tariff;
		internal  string	_FailedReason;
		private int		_AgeLimit;
		private int		_TTL;
		private string	_CPATag;
		internal PositionResult _Position;
		private MessageReplaceSet _MessageReplaceSet;
		private string _ShortCode;
		private DateTime _DeliveryTime;
		private bool	_DeferredDelivery;
        private string _ServiceCode;
        
		/// <summary>
		/// Default constructor
		/// </summary>
		public Message()
		{
			_Text = null;
			_ReceiverNumber = null;
			_MessageClass = 2;
			_SenderNumber = null;
			_RequestReceipt = false;	
			_MessageType = MessageType.Text;
			_Status = MessageStatus.New;
			_Reference = null;
			_Network = null;
			_Tariff = 0;
			_AgeLimit = 0;
			_Address = null;
			_FailedReason = null;
			_TTL = 0;
			_CPATag = null;
			_Position = null;
			_MessageReplaceSet = MessageReplaceSet.NoReplace;
			_ShortCode = null;
			_DeferredDelivery = false;
            _ServiceCode = null;
		}

		#region Accessors
		/// <summary>
		/// Set to true to request a receipt (Delivery Report) for this message. The delivery report
		/// will either be available on the account web or forwarded to your application depending
		/// on your account settings on the SMS Gateway.
		/// </summary>
		public bool RequestReceipt { get { return _RequestReceipt; } set { _RequestReceipt = value; } }
		/// <summary>
		/// Use the MessageClass property to send Flash messages by setting it to 0 (zero). A Flash message
		/// is displayed immediately on the receivers handset. Default MessageClass is 2.
		/// </summary>
		public short MessageClass { get { return _MessageClass; } set { _MessageClass = value; } }
		/// <summary>
		/// The message/text to be sent. One SMS can be up to 160 characters long. If the text is longer,
		/// it will be divided into several SMS messages by the Gateway and delivered and a concatinated
		/// SMS to the receivers handset. When MessageType is a binary type, then this property holds
		/// the hexadecimal string representation of the binary message.
		/// </summary>
		public string Text { get { return _Text; } set { _Text = value; } }		
		/// <summary>
		/// The number of the receiver. The number must be an internationally formatted number. That is
		/// a number that includes country prefix. Any spaces or leading "00" and "+" will be removed and should thus
		/// be avoided.
		/// </summary>
		public string ReceiverNumber { get { return _ReceiverNumber; } set { _ReceiverNumber = value; } }	
		/// <summary>
		/// The number or id of the sender. This should either be an internationally formatted number like
		/// the ReceiverNumber or an alphanumeric string of up to 11 characters. Please note that no special
		/// characters are allowed, only 7 bit ASCII.
		/// </summary>
		public string SenderNumber { get { return _SenderNumber; } set { _SenderNumber = value; } }	
		/// <summary>
		/// The type of message to be sent. The MessageType enumeration holds the various types that are
		/// supported. Default is Text.
		/// </summary>
		public MessageType MessageType { get { return _MessageType; } set { _MessageType = value; } }
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
		/// sending of Premium SMS messages where specific routing is required. Other usage of
		/// this may result in the message not being delivered.
		/// </summary>
		public string Network { get { return _Network; } set { _Network = value; } }
		/// <summary>
		/// Indicated the Premium Price in cents/ører for a message sent as a Premium SMS. This
		/// requires that the account has been enabled for CPA/Premium SMS usage.
		/// </summary>
		public int Tariff { get { return _Tariff; } set { _Tariff = value; } }
		/// <summary>
		/// If a message was not accepted by the SMS Gateway you may find a more specific reason
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

		/// <summary>
		/// You may specify an age limit when sending Premium SMS messages. If you set this property to a 
		/// value larger than 0 (zero), then the value will be matched against the age of the subscriber.
		/// The subscriber must then be at least the given age in order to receive the message.
		/// </summary>
		public int AgeLimit { get { return _AgeLimit; } set { _AgeLimit = value; } }

		/// <summary>
		/// It is possible to enable positioning services on the Gateway for incoming messages. When this is 
		/// enabled for a given keyword or access number, the Gateway will retrieve position data for
		/// the subscriber that sent the SMS. This property is read-only.
		/// This is a value-added feature that will require a separate agreement with PSWinCom.
		/// </summary>
		public PositionResult Position { get { return _Position; } }	

		/// <summary>
		/// Premium SMS messages may have an additional description associated with itself. This value
		/// will be shown on the subscribers phone bill to help identify the service purchased.
		/// Please note that not all operators support this property, and it may be restrictions to
		/// how it is formatted. Pelase consult PSWinCom technical support if you intend to use this
		/// feature. Others should set it to null.
		/// </summary>
		public string CPATag { get { return _CPATag; } set { _CPATag = value; } }

		/// <summary>
		/// Specifies the number of minutes this message will be valid. The time is counted from the moment
		/// the message has been received and stored on PSWinCom Gateway. Set to 0 to use default value.
		/// </summary>
		public int TimeToLive { get { return _TTL; } set { _TTL = value; } }

		/// <summary>
		/// Specifies if the message should replace a previous message in the Inbox on the handset with same ReplaceSet and SenderNumber.
		/// Default is NoReplace.
		/// </summary>
		public MessageReplaceSet MessageReplaceSet { get { return _MessageReplaceSet; } set { _MessageReplaceSet = value; } }

		/// <summary>
		/// Specifies a specific shortcode/accessnumber to use when sending this message. This is only
		/// valid and required for MTU CPA usage.
		/// </summary>
		public string ShortCode { get { return _ShortCode; } set { _ShortCode = value; } }

		/// <summary>
		/// Specifies if the message should be queued for delivery at a later time instead of being forwarded
		/// to operator immediately. When set to true, the DeliveryTime parameter must also be set.
		/// </summary>
		public bool DeferredDelivery { get { return _DeferredDelivery; } set { _DeferredDelivery = value; } }

		/// <summary>
		/// When the DeferredDelivery parameter is set to true, the message will be queued for delivery at
		/// this given time. The DeliveryTime must be between now and 7 days ahead. If outside this interval, the
		/// message will be rejected. The delivery time must be given in CET time. The Gateway will queue the message
		/// for delivery until the given date and time occurs. However the accuracy of delivery will depend on traffic
		/// load and operator accessiblity at the time of delivery.
		/// </summary>
		public DateTime DeliveryTime { get { return _DeliveryTime; } set { _DeliveryTime = value; } }

        /// <summary>
        /// ServiceCode must be specified when the message is a Premium CPA Goods and Services (GAS) message.
        /// It is a 5-digit code specifying the type of service or goods that is being charged.
        /// It should only be used together with Tariff, and also requires a separate CPA GAS agreement
        /// with PSWinCom.
        /// </summary>
        public string ServiceCode { get { return _ServiceCode; } set { _ServiceCode = value; } }

        #endregion
	}

	/// <summary>
	/// A DeliveryReport object holds all the settings and values of a Delivery Report.
	/// </summary>
	public class DeliveryReport
	{
		private string	_State;
		private string	_ReceiverNumber;
		private string	_SenderNumber;
		private string	_Reference;
		private DateTime	_DeliveredDate;
		
		/// <summary>
		/// Default constructor
		/// </summary>
		public DeliveryReport()
		{
			_State = null;
			_ReceiverNumber = null;
			_SenderNumber = null;
			_Reference = null;
			_DeliveredDate = DateTime.Now;
		}

		#region Accessors
		/// <summary>
		/// This property indicates the final state of the message as given by the Network operator. Typical values are:
		/// DELIVRD: Message was successfully delivered to destination. 
		/// EXPIRED: Message validity period has expired. 
		/// DELETED: Message has been deleted. 
		/// UNDELIV: The SMS was undeliverable (not a valid number or no available route to destination).
		/// ACCEPTD: Message was accepted (i.e. has been manually read on behalf of the subscriber by customer service). 
		/// UNKNOWN: No information of delivery status available. 
		/// REJECTD: Message was rejected. 
		/// FAILED:  The SMS failed to be delivered because no operator accepted the message or due to internal Gateway error.
		/// The following status codes will apply specially for Premium messages:
		/// BARRED:  The receiver number is barred/blocked/not in use. Do not retry message, and remove number from any subscriber list. 
		/// BARREDT: The receiver number is temporarily blocked. May be an empty pre-paid account or a subscriber that has extended his credit limit. 
		/// BARREDC: The receiver has blocked for Premium (CPA) messages. Send a non-Premium message to inform the customer about this
		/// BARREDA: The receiver could not receive the message because his/her age is below the specified AgeLimit.
		/// ZERO_BAL: The receiver has an empty prepaid account.
		/// INV_NET: Invalid network. Receiver number is not recognized by the target operator.
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
		/// The Date and Time when the message was delivered to the handset (in local SMSC time)
		/// </summary>
		public DateTime DeliveredDate { get { return _DeliveredDate; } set { _DeliveredDate = value; } }	
		#endregion
	}


	/// <summary>
	/// The result of a position request.
	/// </summary>
	public class PositionResult
	{
		internal PositionRequestStatus _Status = PositionRequestStatus.Failed;
		internal string _FailedReason = null;
		internal GSMPosition _PositionData = null;

		#region Accessors

		/// <summary>
		/// Indicates the status of the position request. It will either be Ok og Failed depending on if the
		/// Gateway was enable to retrieve the GSM Position information for the sender. Applies only to incoming
		/// messages.
		/// </summary>
		public PositionRequestStatus Status { get { return _Status; } }

		/// <summary>
		/// If a message was not accepted by the SMS Gateway you may find a more specific reason
		/// for this by checking this property. It will be empty for successfully submitted messages.
		/// This property is read-only and is updated only as a result of a SendMessages() operation.
		/// </summary>
		public string FailedReason { get { return _FailedReason; } }

		/// <summary>
		/// The position data as a GSMPosition object. This will be null unless PositionRequestStatus equals Ok.
		/// </summary>
		public GSMPosition PositionData { get { return _PositionData; } }

		#endregion
	}


	/// <summary>
	/// Represents a geographical position/area as given by the GSM network
	/// </summary>
	public class GSMPosition
	{
		internal string _Longitude;
		internal string _Latitude;
		internal int _Radius;
		internal string _County;
		internal string _Council;
		internal int _CouncilNumber;
		internal string _Place;
		internal string _SubPlace;
		internal int _ZipCode;
		internal string _City;

		public GSMPosition()
		{
			_Longitude = null;
			_Latitude = null;
			_Radius = -1;
			_County = null;
			_Council = null;
			_CouncilNumber = -1;
			_Place = null;
			_SubPlace = null;
			_ZipCode = -1;
			_City = null;
		}

		#region Accessors

		/// <summary>
		/// Weighted center of gravity (COG) of the area where the subscriber is located to. 
		/// The value is in Degree decimal (sample: -2.43121)
		/// </summary>
		public string Longitude { get { return _Longitude; } }

		/// <summary>
		/// Weighted center of gravity of the area where the subscriber is located to. 
		/// The value is in Degree decimal (sample: 50.123732)
		/// </summary>
		public string Latitude { get { return _Latitude; } }

		/// <summary>
		/// The radius from the COG where the subscriber may probably be. Distance is given in meters. Will be set to -1 if not supported by the subsribers operator.
		/// </summary>
		public int Radius { get { return _Radius; } }

		/// <summary>
		/// County name (Fylke)
		/// </summary>
		public string County { get { return _County; } }
		
		/// <summary>
		/// Council name (Kommune)
		/// </summary>
		public string Council { get { return _Council; } }

		/// <summary>
		/// Council number (Kommunenummer), a 4 digit code. Will be set to -1 if not supported by the subsribers operator.
		/// </summary>
		public int CouncilNumber { get { return _CouncilNumber; } }

		/// <summary>
		/// Place-name of the area where the subscriber is located to.
		/// </summary>
		public string Place { get { return _Place; } }

		/// <summary>
		/// An optional specification of a more precise sub-area.
		/// </summary>
		public string SubPlace { get { return _SubPlace; } }

		/// <summary>
		/// Zip-code of the area where the subscriber is located to. Will be set to -1 if not supported by the subsribers operator.
		/// </summary>
		public int ZipCode { get { return _ZipCode; } }

		/// <summary>
		/// Name of the City/town where the subscriber is located to.
		/// </summary>
		public string City { get { return _City; } }

		#endregion
	}

}
