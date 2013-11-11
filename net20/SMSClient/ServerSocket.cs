using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;

namespace PSWinCom.Gateway.Client
{

	/// <summary>
	/// Delegate for ConnectionReceived event
	/// </summary>
	public delegate void ConnectionReceivedHandler(Stream inStream, Stream outStream);

	/// <summary>
	/// ServerSocket is a simple asynchronous Socket Server that can be configured to listen for
	/// incoming connections on a given port. New connections generates an Event suitable for
	/// use with the SMSClient for handling incoming messages and delivery reports.
	/// </summary>
	public class ServerSocket 
	{

		// State object for reading client data asynchronously
		private class StateObject 
		{
			// Client  socket.
			public Socket workSocket = null;
			// Size of receive buffer.
			public const int BufferSize = 1024;
			// Receive buffer.
			public byte[] buffer = new byte[BufferSize];
			// Received data string.
			public StringBuilder sb = new StringBuilder();  
		}
    		
		/// <summary>
		/// This event is fired whenever a connection is established on the port that the socket server
		/// is listening on. The event has two parameters, one input stream and one output stream. They
		/// should be used directly as parameters into the HandleIncomingMessages() method on your
		/// SMSClient object.
		/// </summary>
		public static event ConnectionReceivedHandler ConnectionReceived;

		private static int _Port = 1112;
		private static bool _IsListening;
		private static Socket listener = null;

		/// <summary>
		/// Local port to listen on.
		/// </summary>
		public int Port { get { return _Port; }  set { _Port = value; } }

		/// <summary>
		/// Flag indicating wether socket is listening or not.
		/// </summary>
		public bool IsListening { get { return _IsListening; }  set { _IsListening = value; } }


		/// <summary>
		/// Default constructor
		/// </summary>
		public ServerSocket() 
		{
		}

		/// <summary>
		/// Initiate listening on chosen local port. This is a non-blocking operation, and the
		/// socket will keep listening until StopListening() is called. Each new connection will
		/// generate ConnectionReceived events.
		/// </summary>
		public void StartListening() 
		{
			if(_IsListening)
				return;

			// Data buffer for incoming data.
			byte[] bytes = new Byte[1024];

			// Establish the local endpoint for the socket.
			// The DNS name of the computer
			// running the listener is "host.contoso.com".
			IPAddress ipAddress = IPAddress.Any;
			IPEndPoint localEndPoint = new IPEndPoint(ipAddress, _Port);

			// Create a TCP/IP socket.
			listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );

			// Bind the socket to the local endpoint and listen for incoming connections.
			try 
			{
				listener.Bind(localEndPoint);
				listener.Listen(10);

				// Start an asynchronous socket to listen for connections.
				listener.BeginAccept( 
					new AsyncCallback(AcceptCallback),
					listener );
				_IsListening = true;
			} 
			catch (Exception e) 
			{
				_IsListening = false;
			}
        
		}

		/// <summary>
		/// Terminate the listening.
		/// </summary>
		public void StopListening()
		{
			if(_IsListening)
				listener.Close();
			_IsListening = false;
		}


		private static void AcceptCallback(IAsyncResult ar) 
		{

			try
			{
				// Get the socket that handles the client request.
				Socket listener = (Socket) ar.AsyncState;
				Socket handler = listener.EndAccept(ar);

				// Create the state object.
				StateObject state = new StateObject();
				state.workSocket = handler;
				handler.BeginReceive( state.buffer, 0, StateObject.BufferSize, 0,
					new AsyncCallback(ReadCallback), state);
			} 
			catch(Exception e) {}
		}

		private static void ReadCallback(IAsyncResult ar) 
		{
			String content = String.Empty;
        
			// Retrieve the state object and the handler socket
			// from the asynchronous state object.
			StateObject state = (StateObject) ar.AsyncState;
			Socket handler = state.workSocket;

			// Read data from the client socket. 
			int bytesRead = handler.EndReceive(ar);

			if (bytesRead > 0) 
			{
				// There  might be more data, so store the data received so far.
				state.sb.Append(Encoding.GetEncoding("ISO-8859-1").GetString(state.buffer, 0, bytesRead));

				// Check for end-of-file tag. If it is not there, read 
				// more data.
				content = state.sb.ToString();
				if (content.IndexOf("</MSGLST>") > -1) 
				{
					// Fire event with in and out stream objects
					MemoryStream inMS = new MemoryStream(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(content));
					MemoryStream outMS = new MemoryStream();
					OnConnectionReceived(inMS, outMS);
					// Write back response
					byte[] buf = new byte[outMS.Length];
					outMS.Read(buf, 0, (int)outMS.Length);
					Send(handler, System.Text.Encoding.GetEncoding("ISO-8859-1").GetString(buf));

					// Done receiving/sending, start next one
					listener.BeginAccept( 
						new AsyncCallback(AcceptCallback),
						listener );

				} 
				else 
				{
					// Not all data received. Get more.
					handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
						new AsyncCallback(ReadCallback), state);
				}
			}
		}
    
		private static void OnConnectionReceived(Stream inStream, Stream outStream)
		{
			if(ConnectionReceived != null)
				ConnectionReceived(inStream, outStream);

		}

		private static void Send(Socket handler, String data) 
		{
			// Convert the string data to byte data using ASCII encoding.
			byte[] byteData = Encoding.ASCII.GetBytes(data);

			// Begin sending the data to the remote device.
			handler.BeginSend(byteData, 0, byteData.Length, 0,
				new AsyncCallback(SendCallback), handler);
		}

		private static void SendCallback(IAsyncResult ar) 
		{
			try 
			{
				// Retrieve the socket from the state object.
				Socket handler = (Socket) ar.AsyncState;

				// Complete sending the data to the remote device.
				int bytesSent = handler.EndSend(ar);

				handler.Shutdown(SocketShutdown.Both);
				handler.Close();

			} 
			catch (Exception e) 
			{
			}
		}
	}
}
