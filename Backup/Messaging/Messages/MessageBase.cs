/*
	Fluorine .NET Flash Remoting Gateway open source library 
	Copyright (C) 2005 Zoltan Csibi, zoltan@TheSilentGroup.com
	
	This library is free software; you can redistribute it and/or
	modify it under the terms of the GNU Lesser General Public
	License as published by the Free Software Foundation; either
	version 2.1 of the License, or (at your option) any later version.
	
	This library is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
	Lesser General Public License for more details.
	
	You should have received a copy of the GNU Lesser General Public
	License along with this library; if not, write to the Free Software
	Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/

using System;
using System.Collections;

namespace com.TheSilentGroup.Fluorine.Messaging.Messages
{
	/// <summary>
	/// Base class for all messages. Messages have two customizable sections; headers and body. The headers property provides access to specialized meta information for a specific message instance. The headers property is an associative array with the specific header name as the key.
	/// <br/><br/>
	/// The body of a message contains the instance specific data that needs to be delivered and processed by the remote destination. The body is an object and is the payload for a message. 
	/// </summary>
	public class MessageBase : IMessage
	{
		protected Hashtable _headers;
		protected long _timestamp;
		protected object _clientId;
		protected string _destination;
		protected string _messageId;
		protected long _timeToLive;
		protected object _body;

		/// <summary>
		/// Messages pushed from the server may arrive in a batch, with messages in the batch potentially targeted to different Consumer instances.
		/// </summary>
		public const string DestinationClientIdHeader = "DSDstClientId";
		/// <summary>
		/// Messages are tagged with the endpoint id for the Channel they are sent over.
		/// </summary>
		public const string EndpointHeader = "DSEndpoint";
		/// <summary>
		/// Messages that need to set remote credentials for a destination carry the Base64 encoded credentials in this header.
		/// </summary>
		public const string RemoteCredentialsHeader = "DSRemoteCredentials";
		/// <summary>
		/// Messages sent with a defined request timeout use this header.
		/// </summary>
		public const string RequestTimeoutHeader = "DSRequestTimeout";
		
		/// <summary>
		/// Initializes a new instance of the MessageBase class.
		/// </summary>
		public MessageBase()
		{
			_headers = new ASObject();
		}

		#region IMessage Members

		/// <summary>
		/// Gets or sets the clientId (which MessageAgent sent the message).
		/// </summary>
		public object clientId
		{
			get{ return _clientId; }
			set{ _clientId = value; }
		}
		/// <summary>
		/// Gets or sets the message destination.
		/// </summary>
		public string destination
		{
			get{ return _destination; }
			set{ _destination = value; }
		}
		/// <summary>
		/// Gets or sets the unique id for the message.
		/// </summary>
		/// <remarks>
		/// The message id can be used to correlate a response to the original request message in request-response messaging scenarios.
		/// </remarks>
		public string messageId
		{
			get{ return _messageId; }
			set{ _messageId = value; }
		}
		/// <summary>
		/// Gets or sets the time stamp for the message.
		/// </summary>
		/// <remarks>
		/// A time stamp is the date and time that the message was sent. The time stamp is used for tracking the message through the system, ensuring quality of service levels and providing a mechanism for message expiration. 
		/// </remarks>
		public long timestamp
		{
			get{ return _timestamp; }
			set{ _timestamp = value; }
		}
		/// <summary>
		/// Gets or sets the time to live value of a message.
		/// </summary>
		/// <remarks>
		/// The time to live value of a message indicates how long the message should be considered valid and deliverable.
		/// This value works in conjunction with the timestamp value. Time to live is the number of 
		/// milliseconds that this message remains valid starting from the specified timestamp value. 
		/// For example, if the timestamp value is 04/05/05 1:30:45 PST and the timeToLive value 
		/// is 5000, then this message will expire at 04/05/05 1:30:50 PST. 
		/// Once a message expires it will not be delivered to any other clients. 
		/// </remarks>
		public long timeToLive
		{
			get{ return _timeToLive; }
			set{ _timeToLive = value; }
		}
		/// <summary>
		/// Gets or sets the body of a message.
		/// </summary>
		/// <remarks>The body contains the specific data that needs to be delivered by the remote destination.</remarks>
		public object body
		{
			get{ return _body; }
			set{ _body = value; }
		}
		/// <summary>
		/// Gets or sets the headers of a message.
		/// </summary>
		/// <remarks>
		/// The headers of a message are an associative array where the key is the header name and the value is the header value.
		/// This property provides access to the specialized meta information for the specific message instance. 
		/// Core header names begin with a 'DS' prefix. Custom header names should start with a unique prefix to avoid name collisions.
		/// </remarks>
		public Hashtable headers
		{
			get{ return _headers; }
			set{ _headers = value; }
		}

		public object GetHeader(string name)
		{
			return _headers[name];
		}

		public void SetHeader(string name, object value)
		{
			_headers[name] = value;
		}

		public bool HeaderExists(string name)
		{
			return _headers.Contains(name);
		}

		public object Clone()
		{
			// TODO:  Add MessageBase.Clone implementation
			return null;
		}

		public virtual string GetMessageSource()
		{
			return null;
		}

		#endregion
	}
}
