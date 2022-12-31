using System;

namespace GrpcRemoting.RpcMessaging
{

	/// <summary>
	/// Serializable message to transport RPC invocation details and their results over the wire. 
	/// </summary>
	[Serializable]
	public class WireCallMessage
	{
		/// <summary>
		/// Gets or sets the raw data of the message content and its RSA signatures (only if message encryption is enabled).
		/// </summary>
		public object Data { get; set; }
	}

	public enum ResponseType
	{
		/// <summary>
		/// Result
		/// </summary>
		Result,
		/// <summary>
		/// Delegate
		/// </summary>
		Delegate,
	}

	[Serializable]
	public class WireResponseMessage
	{
		/// <summary>
		/// Gets or sets the type of the message.
		/// </summary>
		public ResponseType ResponseType { get; set; }

		/// <summary>
		/// Gets or sets the raw data of the message content and its RSA signatures (only if message encryption is enabled).
		/// </summary>
		public object Data { get; set; }
	}
}
