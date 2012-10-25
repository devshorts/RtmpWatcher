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
using System.Net;
using System.Net.Sockets;

namespace com.TheSilentGroup.Fluorine.SystemHelpers.Net
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	sealed class SocketListener : DisposableBase
	{
		private IPEndPoint	_endPoint;
		private Socket		_socket;
		private int			_backLog;
		private int			_acceptCount;
		SocketServer		_socketServer;

		public SocketListener(SocketServer socketServer, IPEndPoint endPoint, int backLog, int	acceptCount)
		{
			_socketServer = socketServer;
			_endPoint = endPoint;
			_backLog = backLog;
			_acceptCount = acceptCount;
		}

		protected override void Free()
		{
			_socket.Close();
		}

		public Socket Socket
		{
			get { return _socket; }
		}

		public SocketServer SocketServer
		{
			get { return _socketServer; }
		}

		public void Start()
		{
			try
			{
				_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				_socket.Bind(_endPoint);
				_socket.Listen(_backLog);
				
				for(int i = 0; i < _acceptCount; i++)
				{
					_socket.BeginAccept(new AsyncCallback(BeginAcceptCallback), this);
				}
			}
			catch
			{
				Stop();
			}
		}

		public void Stop()
		{
			Dispose();
		}

		private void InitializeConnection(ServerSocketConnection connection)
		{
			if( !IsDisposed )
			{
				_socketServer.RaiseOnConnected(connection);
			}
		}

		internal void BeginAcceptCallback(IAsyncResult ar)
		{
			if(!this.IsDisposed)
			{
				SocketListener listener = null;
				Socket acceptedSocket = null;
				ServerSocketConnection serverSocket = null;
				try
				{
					listener = (SocketListener)ar.AsyncState;
					//Get accepted socket
					acceptedSocket = listener.Socket.EndAccept(ar);
					//Adjust buffer size
					//acceptedSocket.ReceiveBufferSize = _socketServer.SocketBufferSize;
					//acceptedSocket.SendBufferSize = _socketServer.SocketBufferSize;

					//Continue to accept
					listener.Socket.BeginAccept(new AsyncCallback(BeginAcceptCallback), listener);
					serverSocket = new ServerSocketConnection(this.SocketServer, acceptedSocket);
					//Initialize
					InitializeConnection(serverSocket);
					_socketServer.AddSocketConnection(serverSocket);

				}
				catch(Exception ex)
				{
					if(serverSocket != null)
					{
						try
						{
							serverSocket.BeginDisconnect(ex);
						}
						catch(Exception ex2)
						{
							_socketServer.RaiseOnError(ex2);
						}
					}
					else
					{
						_socketServer.RaiseOnError(ex);
					}
				}
			}
		}
	}
}
