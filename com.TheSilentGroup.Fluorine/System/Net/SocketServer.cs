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
using System.Net;
using System.Net.Sockets;
using System.Threading;
using com.TheSilentGroup.Fluorine.SystemHelpers.Threading;

namespace com.TheSilentGroup.Fluorine.SystemHelpers.Net
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	sealed class SocketServer : DisposableBase
	{
		event ErrorHandler _onErrorEvent;

		int				_socketBufferSize;
		ISocketService	_socketService;
		Hashtable		_connections;
		ArrayList		_socketListeners;

		public SocketServer(ISocketService socketService)
		{
			_socketService = socketService;
			_connections = new Hashtable();
			_socketListeners = new ArrayList();
			_socketBufferSize = 2048;
		}

		public event ErrorHandler OnError
		{
			add { _onErrorEvent += value; }
			remove { _onErrorEvent -= value; }
		}

		protected override void Free()
		{
			Stop();
		}

		public void Start()
		{
			if(!this.IsDisposed)
			{
				foreach(SocketListener socketListener in _socketListeners)
				{
					socketListener.Start();
				}

				Threading.ThreadPoolEx.QueueUserWorkItem( new WaitCallback(this.ThreadProc) );
			}
		}

		private void StopListeners()
		{
			if(!this.IsDisposed)
			{
				SocketListener[] socketListeners = GetSocketListeners();
				if( socketListeners != null )
				{
					foreach(SocketListener socketListener in socketListeners)
					{
						try
						{
							socketListener.Stop();
							RemoveListener(socketListener);
						}
						catch { }
					}
				}
			}
		}

		private void StopServerSockets()
		{
			if( !this.IsDisposed )
			{
				ServerSocketConnection[] serverSockets = GetSocketConnections();
				if( serverSockets != null )
				{
					foreach(ServerSocketConnection serverSocket in serverSockets)
					{
						try
						{
							serverSocket.BeginDisconnect();
						}
						catch { }
					}
				}
			}
		}

		public void Stop()
		{
			if( this.IsDisposed )
			{
				StopListeners();
				StopServerSockets();
			}
		}

		internal SocketListener[] GetSocketListeners()
		{
			SocketListener[] socketListeners = null;
			if(!this.IsDisposed)
			{
				lock(_socketListeners)
				{
					socketListeners = new SocketListener[_socketListeners.Count];
					_socketListeners.CopyTo(socketListeners, 0);
				}

			}
			return socketListeners;
		}

		internal ServerSocketConnection[] GetSocketConnections()
		{
			ServerSocketConnection[] serverSockets = null;
			if(!this.IsDisposed)
			{
				lock(_connections)
				{
					serverSockets = new ServerSocketConnection[_connections.Count];
					_connections.CopyTo(serverSockets, 0);
				}

			}
			return serverSockets;
		}

		internal void AddSocketConnection(ServerSocketConnection socket)
		{
			if(!this.IsDisposed)
			{
				lock(_connections)
				{
					_connections[socket.ConnectionId] = socket;
				}
			}

		}

		internal void RemoveSocketConnection(ServerSocketConnection socket)
		{
			if(!this.IsDisposed)
			{
				lock(_connections)
				{
					_connections.Remove(socket.ConnectionId);
				}
			}
		}

		public ServerSocketConnection GetConnectionById(string connectionId)
		{
			ServerSocketConnection result = null;
			if( !this.IsDisposed )
			{
				lock(_connections)
				{
					result = _connections[connectionId] as ServerSocketConnection;
				}
			}
			return result;
		}


		public void AddListener(IPEndPoint localEndPoint)
		{
			if(!this.IsDisposed)
			{
				lock(_socketListeners)
				{
					SocketListener socketListener = new SocketListener(this, localEndPoint, 2, 2);
					_socketListeners.Add(socketListener);
				}
			}
		}

		public void RemoveListener(SocketListener socketListener)
		{
			if(!this.IsDisposed)
			{
				lock(_socketListeners)
				{
					_socketListeners.Remove(socketListener);
				}
			}
		}



		/// <summary>
		/// Receive data from connetion.
		/// </summary>
		internal void BeginReceive(ServerSocketConnection connection)
		{
			if(!IsDisposed)
			{
				try
				{
					if(connection.IsActive)
					{
						lock (connection.SyncRead)
						{
							if (connection.ReadCount == 0)
							{
								MessageBuffer readMessage = new MessageBuffer(_socketBufferSize);
								connection.Socket.BeginReceive(readMessage.PacketBuffer, readMessage.PacketOffSet, readMessage.PacketRemaining, SocketFlags.None, new AsyncCallback(BeginReadCallback), new CallbackData(connection, readMessage));
							}
							connection.ReadCount++;
						}
					}
				}
				catch(Exception ex)
				{
					try
					{
						connection.BeginDisconnect(ex);
					}
					catch(Exception ex2)
					{
						RaiseOnError(ex2);
					}
				}
			}
		}

		private void BeginReadCallback(IAsyncResult ar)
		{
			if(!IsDisposed)
			{
				ServerSocketConnection connection = null;
				MessageBuffer readMessage = null;
				byte[] received = null;
				try
				{
					CallbackData callbackData = (CallbackData)ar.AsyncState;
					connection = callbackData.Connection;
					readMessage = callbackData.Buffer;

					if (connection.IsActive)
					{
						int readBytes = 0;
						readBytes = connection.Socket.EndReceive(ar);
						if (readBytes > 0)
						{
							byte[] rawBuffer = null;
							rawBuffer = readMessage.GetRawBuffer(readBytes, 0);

							received = new byte[rawBuffer.Length];
							Array.Copy(rawBuffer, 0, received, 0, rawBuffer.Length);
							RaiseOnReceived(connection, received, true);

							//Check Queue
							lock(connection.SyncRead)
							{
								connection.ReadCount--;
								if(connection.ReadCount > 0)
								{
									connection.Socket.BeginReceive(readMessage.PacketBuffer, readMessage.PacketOffSet, readMessage.PacketRemaining, SocketFlags.None, new AsyncCallback(BeginReadCallback), callbackData);
								}
							}
						}
						else
						{
							// No data to read
							connection.BeginDisconnect();
						}
					}
				}
				catch(Exception ex)
				{
					try
					{
						connection.BeginDisconnect(ex);
					}
					catch(Exception ex2)
					{
						RaiseOnError(ex2);
					}
				}
			}
		}

		private void RaiseOnReceived(ServerSocketConnection connection, byte[] buffer, bool readCanEnqueue)
		{
        
			MessageEventArgs e = new MessageEventArgs(connection, buffer);
			e.Tag = readCanEnqueue;
			ThreadPoolEx.QueueUserWorkItem(new WaitCallback(OnReceivedCallback), e);
		}

		private void OnReceivedCallback(object state)
		{

			MessageEventArgs e = (MessageEventArgs)state;
			ServerSocketConnection connection = (ServerSocketConnection) e.Connection;
			bool readCanEnqueue = (bool)e.Tag;

			try
			{
				_socketService.OnReceived(e);
			}
			catch(Exception ex)
			{
				RaiseOnError(ex);
			}
			e.Buffer = null;
			state = null;
		}

		internal void BeginSendTo(ServerSocketConnection connection, byte[] buffer)
		{
			if(!this.IsDisposed)
			{
				BeginSend(connection, buffer);
			}
		}

		internal void BeginSendToAll(ServerSocketConnection connection, byte[] buffer)
		{
			if(!this.IsDisposed)
			{
				ServerSocketConnection[] serverSockets = GetSocketConnections();
				if(serverSockets != null)
				{
					foreach(ServerSocketConnection connectionTmp in serverSockets)
					{
						if(connection != connectionTmp)
						{
							BeginSend(connectionTmp, buffer);
						}
					}
				}
			}
		}

		/// <summary>
		/// Begin send the data.
		/// </summary>
		internal void BeginSend(ServerSocketConnection connection, byte[] buffer)
		{
			if (!IsDisposed)
			{
				try
				{
					if (connection.IsActive)
					{
						MessageBuffer writeMessage = MessageBuffer.GetPacketMessage(connection, ref buffer);
						lock(connection.WriteQueue)
						{
							if(connection.WriteQueueHasItems)
							{
								//Enqueue the message
								connection.WriteQueue.Enqueue(writeMessage);
							}
							else
							{
								connection.WriteQueueHasItems = true;
								connection.Socket.BeginSend(writeMessage.PacketBuffer, writeMessage.PacketOffSet, writeMessage.PacketRemaining, SocketFlags.None, new AsyncCallback(BeginSendCallback), new CallbackData(connection, writeMessage));
							}
						}
					}
				}
				catch(Exception ex)
				{
					try
					{
						connection.BeginDisconnect(ex);
					}
					catch(Exception ex2)
					{
						RaiseOnError(ex2);
					}
				}
			}
		}

		/// <summary>
		/// Send Callback.
		/// </summary>
		private void BeginSendCallback(IAsyncResult ar)
		{
			if(!IsDisposed)
			{
				ServerSocketConnection connection = null;
				MessageBuffer writeMessage = null;
				byte[] sent = null;
				try
				{
					CallbackData callbackData = (CallbackData)ar.AsyncState;
					connection = callbackData.Connection;
					writeMessage = callbackData.Buffer;
					if (connection.IsActive)
					{
						int writeBytes = connection.Socket.EndSend(ar);
						if (writeBytes < writeMessage.PacketBuffer.Length)
						{
							//Continue to send until all bytes are sent
							writeMessage.PacketOffSet += writeBytes;
							connection.Socket.BeginSend(writeMessage.PacketBuffer, writeMessage.PacketOffSet, writeMessage.PacketRemaining, SocketFlags.None, new AsyncCallback(BeginSendCallback), callbackData);
						}
						else
						{
							sent = new byte[writeMessage.RawBuffer.Length];
							Array.Copy(writeMessage.RawBuffer, 0, sent, 0, writeMessage.RawBuffer.Length);
							RaiseOnSent(connection, sent);
						}

						//Check Queue
						lock(connection.WriteQueue)
						{
							if (connection.WriteQueue.Count > 0)
							{
								//----- If has items, send it!
								MessageBuffer dequeueWriteMessage = connection.WriteQueue.Dequeue() as MessageBuffer;
								connection.Socket.BeginSend(dequeueWriteMessage.PacketBuffer, dequeueWriteMessage.PacketOffSet, dequeueWriteMessage.PacketRemaining, SocketFlags.None, new AsyncCallback(BeginSendCallback), new CallbackData(connection, dequeueWriteMessage));
							}
							else
							{
								connection.WriteQueueHasItems = false;
							}
						}
					}
				}
				catch(Exception ex)
				{
					try
					{
						connection.BeginDisconnect(ex);
					}
					catch(Exception ex2)
					{
						RaiseOnError(ex2);
					}
				}
			}
		}

		internal void RaiseOnError(Exception exception)
		{
			if(_onErrorEvent != null)
			{
				_onErrorEvent(this, new ServerErrorEventArgs(exception));
			}
		}

		internal void RaiseOnConnected(ServerSocketConnection connection)
		{
			ConnectionEventArgs e = new ConnectionEventArgs(connection);
			ThreadPoolEx.QueueUserWorkItem(new WaitCallback(OnConnectedCallback), e);
		}

		internal void RaiseOnSent(ServerSocketConnection connection, byte[] buffer)
		{
			MessageEventArgs e = new MessageEventArgs(connection, buffer);
			ThreadPoolEx.QueueUserWorkItem(new WaitCallback(OnSentCallback), e);
		}

		private void OnConnectedCallback(object state)
		{
			_socketService.OnConnected((ConnectionEventArgs)state);
			state = null;
		}

		private void OnSentCallback(object state)
		{
			MessageEventArgs e = (MessageEventArgs)state;
			_socketService.OnSent(e);
			e.Buffer = null;
			state = null;
		}
		
		private void ThreadProc(object stateInfo) 
		{
		}
	}

	delegate void ErrorHandler(object sender, ServerErrorEventArgs e); 

	/// <summary>
	/// Base event arguments for connection events.
	/// </summary>
	class ServerErrorEventArgs : EventArgs
	{
		Exception _exception;

		public ServerErrorEventArgs(Exception exception)
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
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class CallbackData
	{
		#region Fields

		private ServerSocketConnection _connection;
		private MessageBuffer _buffer;

		#endregion

		#region Constructor

		public CallbackData(ServerSocketConnection connection, MessageBuffer buffer)
		{
			_connection = connection;
			_buffer = buffer;
		}

		#endregion

		#region Properties

		public ServerSocketConnection Connection
		{
			get { return _connection; }
		}

		public MessageBuffer Buffer
		{
			get { return _buffer; }
		}

		#endregion
	}
}
