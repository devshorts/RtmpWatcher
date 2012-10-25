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

namespace com.TheSilentGroup.Fluorine.SystemHelpers.Net
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	interface ISocketService
	{
		/// <summary>
		/// Fired when connected.
		/// </summary>
		/// <param name="e">
		/// Information about the connection.
		/// </param>
		void OnConnected(ConnectionEventArgs e);
		/// <summary>
		/// Fired when data arrives.
		/// </summary>
		/// <param name="e">
		/// Information about the Message.
		/// </param>
		void OnReceived(MessageEventArgs e);
		/// <summary>
		/// Fired when data is sent.
		/// </summary>
		/// <param name="e">
		/// Information about the Message.
		/// </param>
		void OnSent(MessageEventArgs e);
		/// <summary>
		/// Fired when disconnected.
		/// </summary>
		/// <param name="e">
		/// Information about the connection.
		/// </param>
		void OnDisconnected(DisconnectedEventArgs e);
	}

	/// <summary>
	/// Base event arguments for connection events.
	/// </summary>
	class ConnectionEventArgs : EventArgs
	{
		private ISocket _connection;

		public ConnectionEventArgs(ISocket connection)
		{
			_connection = connection;
		}

		#region Properties

		public ISocket Connection
		{
			get { return _connection; }
		}

		#endregion
	}

	/// <summary>
	/// Disconnect event arguments for disconnected event.
	/// </summary>
	class DisconnectedEventArgs : ConnectionEventArgs
	{
		private Exception _exception;

		public DisconnectedEventArgs(ISocket connection, Exception exception) : base(connection)
		{
			_exception = exception;
		}

		#region Properties

		public Exception Exception
		{
			get { return _exception; }
		}

		#endregion
	}

	/// <summary>
	/// Message event arguments for message events.
	/// </summary>
	class MessageEventArgs : ConnectionEventArgs
	{
		byte[] _buffer;
		object _tag;

		public MessageEventArgs(ISocket connection, byte[] buffer) : base(connection)
		{
			_buffer = buffer;
		}

		#region Properties

		public byte[] Buffer
		{
			get { return _buffer; }
			set { _buffer = value; }
		}

		public object Tag
		{
			get { return _tag; }
			set { _tag = value; }
		}

		#endregion
	}
}
