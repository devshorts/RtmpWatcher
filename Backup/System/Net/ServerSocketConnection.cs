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
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace com.TheSilentGroup.Fluorine.SystemHelpers.Net
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	sealed class ServerSocketConnection : DisposableBase, IServerSocket
	{
		object			_syncRead = new object();
		int				_readCount;
		
		Socket			_socket;
		Stream			_stream;
		SocketServer	_socketServer;

		object			_tag;
		private byte[]	_header;
		private string	_connectionId;

		private Queue	_writeQueue;
		private bool	_writeQueueHasItems;

		public ServerSocketConnection(SocketServer socketServer, Socket socket)
		{
			_connectionId = Guid.NewGuid().ToString();
			_socketServer = socketServer;
			_socket = socket;

			_writeQueue = new Queue();
			_writeQueueHasItems = false;
		}

		protected override void Free()
		{
			_writeQueue.Clear();
			if( _stream != null )
			{
				_stream.Close();
			}
			base.Free();
		}


		#region IServerSocket Members

		public void BeginSendToAll(byte[] buffer)
		{
			_socketServer.BeginSendToAll(this, buffer);
		}

		public void BeginSendTo(ISocket connection, byte[] buffer)
		{
			_socketServer.BeginSendTo((ServerSocketConnection)connection, buffer);
		}

		public ISocket GetConnectionById(string connectionId)
		{
			return _socketServer.GetConnectionById(connectionId);
		}

		#endregion

		#region ISocket Members

		public object Tag
		{
			get{ return _tag; }
			set{ _tag = value; }
		}

		public byte[] Header
		{
			get{ return _header; }
			set{ _header = value; }
		}

		public string ConnectionId
		{
			get{ return _connectionId; }
		}

		public HostType HostType
		{
			get{ return HostType.Server; }
		}

		public IntPtr SocketHandle
		{
			get{ return _socket.Handle; }
		}

		public IPEndPoint LocalEndPoint
		{
			get{ return _socket.LocalEndPoint as IPEndPoint; }
		}

		public IPEndPoint RemoteEndPoint
		{
			get{ return _socket.RemoteEndPoint as IPEndPoint; }
		}

		public void BeginSend(byte[] buffer)
		{
			_socketServer.BeginSend(this, buffer);
		}

		public void BeginReceive()
		{
			_socketServer.BeginReceive(this);
		}

		public void BeginDisconnect()
		{
			// TODO:  Add ServerSocket.BeginDisconnect implementation
		}

		#endregion

		public bool IsActive
		{
			get { return !this.IsDisposed; }
		}

		public object SyncRead
		{
			get { return _syncRead; }
		}

		public int ReadCount
		{
			get { return _readCount; }
			set { _readCount = value; }
		}

		public Stream Stream
		{
			get { return _stream; }
			set { _stream = value; }
		}

		public Socket Socket
		{
			get { return _socket; }
			set { _socket = value; }
		}

		public  Queue WriteQueue
		{
			get { return _writeQueue; }
		}

		public bool WriteQueueHasItems
		{
			get { return _writeQueueHasItems; }
			set { _writeQueueHasItems = value; }
		}

		public void Start()
		{
		}

		public void Stop()
		{
		}

		public void BeginDisconnect(Exception ex)
		{
			//_socketServer.BeginDisconnect(this, ex);
		}
	}
}
