using System;

namespace PSWinCom.Gateway.MMS.Client
{
	/// <summary>
	/// SMSException class for SMS Client related exceptions.
	/// </summary>
	public class MMSException : Exception
	{
	
		public MMSException() : base () 
		{
		}

		public MMSException(string message) : base (message) 
		{
		}

		public MMSException(string message, Exception inner) : base (message, inner) 
		{
		}

	}
}
