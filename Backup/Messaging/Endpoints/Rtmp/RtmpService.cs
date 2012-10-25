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
using com.TheSilentGroup.Fluorine.SystemHelpers.IO;
using com.TheSilentGroup.Fluorine.SystemHelpers.Net;
using flex.messaging.endpoints;
using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Event;
using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api.Service;
using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api;
using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api.Stream;
using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api.Event;
using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Exceptions;

namespace com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp
{
	/// <summary>
	/// Summary description for RtmpService.
	/// </summary>
	internal class RtmpService : ISocketService
	{
		public const string ACTION_CONNECT = "connect";
		public const string ACTION_DISCONNECT = "disconnect";
		public const string ACTION_CREATE_STREAM = "createStream";
		public const string ACTION_DELETE_STREAM = "deleteStream";
		public const string ACTION_CLOSE_STREAM = "closeStream";
		public const string ACTION_PUBLISH = "publish";
		public const string ACTION_PAUSE = "pause";
		public const string ACTION_SEEK = "seek";
		public const string ACTION_PLAY = "play";
		public const string ACTION_STOP = "disconnect";
		public const string ACTION_RECEIVE_VIDEO = "receiveVideo";
		public const string ACTION_RECEIVE_AUDIO = "receiveAudio";

		RTMPEndpoint	_endpoint;
		Hashtable		_connections;
		IServer			_server;

		public RtmpService(RTMPEndpoint endpoint, IServer server)
		{
			_connections = new Hashtable();
			_endpoint = endpoint;
			_server = server;
		}

		#region ISocketService Members

		public void OnConnected(ConnectionEventArgs e)
		{
			if( e.Connection.HostType == HostType.Server )
			{
				e.Connection.BeginReceive();
			}
		}

		public void OnReceived(MessageEventArgs e)
		{
			if(e.Connection.HostType == HostType.Server)
			{
				byte[] buffer = e.Buffer;

				RtmpConnection connection = null;
				if( !_connections.Contains(e.Connection.ConnectionId) )
				{
					connection = new RtmpConnection(this, RtmpConnection.Persistent, e.Connection.ConnectionId);
					_connections[connection.ConnectionId] = connection;
				}
				else
					connection = _connections[e.Connection.ConnectionId] as RtmpConnection;

				ByteBuffer inputStream = ByteBuffer.Wrap(buffer);
				ArrayList result = RtmpProtocolDecoder.DecodeBuffer(connection.Context, inputStream);
				foreach(object obj in result)
				{
					if( obj is ByteBuffer )
					{
						ByteBuffer buf = obj as ByteBuffer;
						byte[] bufferOut = buf.ToArray();
						e.Connection.BeginSend(bufferOut);
					}
					else
					{
						MessageReceived(connection, obj);
					}
				}
			}		
		}

		public void OnSent(MessageEventArgs e)
		{
			if(e.Connection.HostType == HostType.Server)
			{
				e.Connection.BeginReceive();
			}
		}

		public void OnDisconnected(DisconnectedEventArgs e)
		{
			// TODO:  Add RtmpService.OnDisconnected implementation
		}

		#endregion


		private string GetHostname(string url) 
		{
			string[] parts = url.Split(new char[]{'/'});
			if(parts.Length == 2) 
				return "";
			else 
				return parts[2];
		}

		private void MessageReceived(RtmpConnection connection, object obj)
		{
			RtmpPacket packet = obj as RtmpPacket;
			RtmpMessage message = packet.Message;
			RtmpHeader header = packet.Header;
			RtmpChannel channel = connection.GetChannel(header.ChannelId);
			IClientStream stream = connection.GetStreamById(header.StreamId);

			message.SetSource(connection);

			switch(header.DataType)
			{
				case DataType.TypeInvoke:
					OnInvoke(connection, channel, header, message as Invoke);
					if(message.Header.StreamId != 0  
						&& (message as Invoke).ServiceCall.ServiceName == null
						&& (message as Invoke).ServiceCall.ServiceMethodName == RtmpService.ACTION_PUBLISH )
					{
						IClientStream clientStream = connection.GetStreamById(header.StreamId);
						(clientStream as IEventDispatcher).DispatchEvent(message);
					}
					break;
				case DataType.TypeNotify:
					break;
				case DataType.TypePing:
					break;
				default:
					break;
			}
		}

		public void Encode(RtmpConnection connection, object message)
		{
			ByteBuffer outputStream = RtmpProtocolEncoder.Encode(connection.Context, message);
			ServerSocketConnection serverSocket = _endpoint.SocketServer.GetConnectionById( connection.ConnectionId );
			_endpoint.SocketServer.BeginSendTo(serverSocket, outputStream.ToArray());
		}

		private void OnInvoke(RtmpConnection connection, RtmpChannel channel, RtmpHeader header, Notify invoke)
		{
			IServiceCall serviceCall = invoke.ServiceCall;
			if(serviceCall.ServiceMethodName.Equals("_result") || serviceCall.ServiceMethodName.Equals("_error"))
			{
				IPendingServiceCall pendingCall = connection.GetPendingCall(invoke.InvokeId);
				if (pendingCall != null) 
				{
					// The client sent a response to a previously made call.
					object[] args = serviceCall.Arguments;
					if((args != null) && (args.Length > 0)) 
					{
						pendingCall.Result = args[0];
					}
					IPendingServiceCallback[] callbacks = pendingCall.GetCallbacks();
					if( callbacks.Length == 0 ) 
						return;
					foreach(IPendingServiceCallback callback in callbacks)
					{
						try 
						{
							callback.ResultReceived(pendingCall);
						} 
						catch(Exception) 
						{
						}
					}
				}
				return;
			}

			// Make sure we don't use invoke ids that are used by the client
			lock(connection) 
			{
				if (connection.InvokeId <= invoke.InvokeId )
				{
					connection.InvokeId = invoke.InvokeId + 1;
				}
			}

			bool disconnectOnReturn = false;
			if(serviceCall.ServiceName == null) 
			{
				string action = serviceCall.ServiceMethodName;
				if(!connection.IsConnected)
				{
					switch(action)
					{
						case ACTION_CONNECT:
						{
							Hashtable parameters = invoke.ConnectionParameters;
							string host = GetHostname(parameters["tcUrl"] as string);
							if(host.EndsWith(":1935")) 
							{
								// Remove default port from connection string
								host = host.Substring(0, host.Length - 5);
							}
							string path = parameters["app"] as string;
							string sessionId = null;
							connection.Setup(host, path, sessionId, parameters);
							try
							{
								IGlobalScope global = _server.LookupGlobal(host, path);
								if (global == null) 
								{
									serviceCall.Status = ServiceCall.STATUS_SERVICE_NOT_FOUND;
									if(serviceCall is IPendingServiceCall) 
										(serviceCall as IPendingServiceCall).Result = StatusASO.GetStatusObject(StatusASO.NC_CONNECT_FAILED);
									connection.Close();
								}
								else
								{
									IContext context = global.Context;
									IScope scope = null;
									try 
									{
										scope = context.ResolveScope(path);
									} 
									catch (ScopeNotFoundException /*exception*/) 
									{
										serviceCall.Status = ServiceCall.STATUS_SERVICE_NOT_FOUND;
										if(serviceCall is IPendingServiceCall) 
											(serviceCall as IPendingServiceCall).Result = StatusASO.GetStatusObject(StatusASO.NC_CONNECT_FAILED);
										disconnectOnReturn = true;
									}
									if (scope != null) 
									{
										//log.info("Connecting to: " + scope);
										bool okayToConnect;
										try 
										{
											if(serviceCall.Arguments != null) 
											{
												okayToConnect = connection.Connect(scope, serviceCall.Arguments);
											} 
											else 
											{
												okayToConnect = connection.Connect(scope);
											}
											if (okayToConnect) 
											{
												//log.debug("connected");
												//log.debug("client: " + conn.getClient());
												serviceCall.Status = ServiceCall.STATUS_SUCCESS_RESULT;
												if(serviceCall is IPendingServiceCall) 
													(serviceCall as IPendingServiceCall).Result = StatusASO.GetStatusObject(StatusASO.NC_CONNECT_SUCCESS);
												// Measure initial roundtrip time after connecting
												//conn.getChannel((byte) 2).write(new Ping((short) 0, 0, -1));
												//conn.ping();
											}
											else
											{
												//log.debug("connect failed");
												serviceCall.Status = ServiceCall.STATUS_ACCESS_DENIED;
												if(serviceCall is IPendingServiceCall) 
													(serviceCall as IPendingServiceCall).Result = StatusASO.GetStatusObject(StatusASO.NC_CONNECT_REJECTED);
												disconnectOnReturn = true;
											}
										}
										catch(ClientRejectedException rejected)
										{
											//log.debug("connect rejected");
											serviceCall.Status = ServiceCall.STATUS_ACCESS_DENIED;
											if(serviceCall is IPendingServiceCall)
											{
												StatusASO statusASO = StatusASO.GetStatusObject(StatusASO.NC_CONNECT_REJECTED);
												statusASO.Application = rejected.Reason;
												(serviceCall as IPendingServiceCall).Result = statusASO;
											}
											disconnectOnReturn = true;
										}
									}
								}
							}
							catch(Exception /*ex*/)
							{
								serviceCall.Status = ServiceCall.STATUS_GENERAL_EXCEPTION;
								if(serviceCall is IPendingServiceCall) 
									(serviceCall as IPendingServiceCall).Result = StatusASO.GetStatusObject(StatusASO.NC_CONNECT_FAILED);
								//log.error("Error connecting", e);
								disconnectOnReturn = true;
							}
						}
							break;
						case ACTION_DISCONNECT:
							break;
						default:
							InvokeCall(connection, serviceCall);
							break;
					}
				}
				else if (connection.IsConnected)
				{
					// Service calls, must be connected.
					InvokeCall(connection, serviceCall);
				}
				else 
				{
					// Warn user attemps to call service without being connected
					//"Not connected, closing connection");
					connection.Close();
				}
			}

			if(invoke is Invoke) 
			{
				if((header.StreamId != 0)
					&& (serviceCall.Status == ServiceCall.STATUS_SUCCESS_VOID || serviceCall.Status == ServiceCall.STATUS_SUCCESS_NULL)) 
				{
					//Method does not have return value, do not reply
					return;
				}

				// The client expects a result for the method call.
				Invoke reply = new Invoke();
				reply.ServiceCall = serviceCall;
				reply.InvokeId = invoke.InvokeId;
				//sending reply
				channel.Write(reply);
				if (disconnectOnReturn) 
				{
					connection.Close();
				}
			}
		}

		public void InvokeCall(RtmpConnection connection, IServiceCall serviceCall) 
		{
			IScope scope = connection.Scope;
			if(scope.HasHandler) 
			{
				IScopeHandler handler = scope.Handler;
				//log.debug("Scope: " + scope);
				//log.debug("Handler: " + handler);
				if(!handler.ServiceCall(connection, serviceCall)) 
				{
					// XXX: What do do here? Return an error?
					return;
				}
			}
			IContext context = scope.Context;
			//log.debug("Context: " + context);
			context.GetServiceInvoker().Invoke(serviceCall, scope);
		}
	}
}
