using System;

namespace PSWinCom.Gateway.Client
{
	/// <summary>
	/// SMSException class for SMS Client related exceptions.
	/// </summary>
	public class SMSException : Exception
	{
	
		/// <summary>
		/// Default constructor
		/// </summary>
		public SMSException() : base () 
		{
		}

		/// <summary>
		/// Constructor taking string with error message as input
		/// </summary>
		/// <param name="message">Error message</param>
		public SMSException(string message) : base (message) 
		{
		}

		/// <summary>
		/// Constructor taking string with error message and an inner exception as input
		/// </summary>
		/// <param name="message">Error message</param>
		/// <param name="inner">Inner exception</param>
		public SMSException(string message, Exception inner) : base (message, inner) 
		{
		}

	}
}
