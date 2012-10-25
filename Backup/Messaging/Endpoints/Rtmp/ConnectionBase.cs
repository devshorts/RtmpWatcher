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
using System.Collections.Specialized;

// Import log4net classes.
using log4net;
using log4net.Config;

using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api;
using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api.Event;

namespace com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp
{
	/// <summary>
	/// Summary description for ConnectionBase.
	/// </summary>
	public class ConnectionBase : AttributeStore, IConnection
	{
		private static ILog _log = LogManager.GetLogger(typeof(ConnectionBase));

		protected ObjectEncoding _objectEncoding;
		protected IScope		_scope;
		protected string _type;
		protected string _host;
		protected string _remoteAddress;
		protected int	_remotePort;
		protected string _path;
		protected string _sessionId;
		protected long _readMessages = 0;
		protected long _writtenMessages = 0;
		protected long _droppedMessages = 0;
		protected Hashtable _parameters = null;
		protected IClient _client = null;
		protected Hashtable _basicScopes;

		public ConnectionBase(string type, string host, string remoteAddress,
			int remotePort, string path, string sessionId)
		{
			_type = type;
			_host = host;
			_remoteAddress = remoteAddress;
			_remotePort = remotePort;
			_path = path;
			_sessionId = sessionId;
			_basicScopes = new Hashtable();
			_objectEncoding = ObjectEncoding.AMF0;
		}

		#region IConnection Members

		public ObjectEncoding ObjectEncoding
		{
			get{ return _objectEncoding; }
			set{ _objectEncoding = value; }
		}

		public string Type
		{
			get
			{
				return _type;
			}
		}

		public void Initialize(IClient client) 
		{
			if(this.Client != null && this.Client is Client) 
			{
				// Unregister old client
				(this.Client as Client).Unregister(this);
			}
			_client = client;
			if(this.Client is Client) 
			{
				// Register new client
				(_client as Client).Register(this);
			}
		}

		public bool Connect(IScope scope)
		{
			return Connect(scope, null);
		}

		public bool Connect(IScope scope, object[] parameters)
		{
			IScope oldScope = _scope;
			_scope = scope;
			if(_scope.Connect(this, parameters)) 
			{
				if(oldScope != null)
				{
					oldScope.Disconnect(this);
				}
				return true;
			} 
			else 
			{
				_scope = oldScope;
				return false;
			}
		}

		public bool IsConnected
		{
			get{ return _scope != null; }
		}

		public void Close()
		{
			if(_scope != null) 
			{
				try 
				{
					//Close, disconnect from scope, and children
					foreach(IBasicScope basicScope in _basicScopes.Keys)
					{
						UnregisterBasicScope(basicScope);
					}
				}
				catch (Exception ex) 
				{
					_log.Error("Error while unregistering basic scopes.", ex);
				}

				try 
				{
					_scope.Disconnect(this);
				} 
				catch (Exception ex) 
				{
					_log.Error("Error while disconnecting from scope " + _scope, ex);
				}

				if(_client != null && _client is Client)
				{
					(_client as Client).Unregister(this);
					_client = null;
				}
				_scope = null;
			}
			else 
			{
				_log.Debug("Close, not connected nothing to do.");
			}
		}

		public Hashtable ConnectionParameters
		{
			get
			{
				return _parameters;
			}
		}

		public IClient Client
		{
			get
			{
				return _client;
			}
		}

		public IScope Scope
		{
			get
			{
				return _scope;
			}
		}

		public string Host
		{
			get
			{
				return _host;
			}
		}

		public string RemoteAddress
		{
			get
			{
				return _remoteAddress;
			}
		}

		public int RemotePort
		{
			get
			{
				return _remotePort;
			}
		}

		public string Path
		{
			get
			{
				return _path;
			}
		}

		public ICollection BasicScopes
		{
			get
			{
				return _basicScopes.Keys;
			}
		}

		public string SessionId
		{ 
			get
			{
				return _sessionId; 
			}
		}

		#endregion

		public void RegisterBasicScope(IBasicScope basicScope) 
		{
			_basicScopes.Add(basicScope, basicScope);
			basicScope.AddEventListener(this);
		}

		public void UnregisterBasicScope(IBasicScope basicScope) 
		{
			_basicScopes.Remove(basicScope);
			basicScope.RemoveEventListener(this);
		}
		
		#region IEventListener Members

		public void NotifyEvent(IEvent evt) 
		{
		}

		#endregion

		#region IEventDispatcher Members
		
		public void DispatchEvent(IEvent evt) 
		{
		}

		#endregion

		#region IEventHandler Members
		
		public bool HandleEvent(IEvent evt) 
		{
			return this.Scope.HandleEvent(evt);
		}
		
		#endregion

	}
}
