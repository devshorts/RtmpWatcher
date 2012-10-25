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

// Import log4net classes.
using log4net;
using log4net.Config;

using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api;

namespace com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp
{
	/// <summary>
	/// Summary description for Scope.
	/// </summary>
	public class Scope : BasicScope, IScope
	{
		private ILog _log;

		private static string TypeName = "scope";
		public static string Separator = ":";
		private static string ServiceHandlers = AttributeStore.TransientPrefix + "_scope_service_handlers";


		private IContext _context;
		private IScopeHandler _handler;
		private bool _autoStart = true;
		private bool _enabled = true;
		private bool _running = false;
		/// <summary>
		/// String, IBasicScope
		/// </summary>
		private Hashtable _children = new Hashtable();
		/// <summary>
		/// IClient, Set(IConnection)
		/// </summary>
		private Hashtable _clients = new Hashtable();

		public Scope():this(null)
		{
			try
			{
				_log = LogManager.GetLogger(typeof(Scope));
			}
			catch{}
		}

		public Scope(string name) : base(null, TypeName, name, false)
		{
		}

		public bool IsEnabled
		{
			get{ return _enabled; }
			set{ _enabled = value; }
		}

		public bool IsRunning
		{
			get{ return _running; }
		}

		public bool AutoStart
		{
			get{ return _autoStart; }
			set{ _autoStart = value; }
		}

		public bool HasContext
		{
			get{ return _context != null; }
		}

		public void Init()
		{
			if(HasParent) 
			{
				if(!Parent.HasChildScope(this.Name)) 
				{
					if(!Parent.AddChildScope(this)) 
					{
						return;
					}
				}
			}
			if(AutoStart) 
			{
				Start();
			}
		}

		public bool Start() 
		{
			if(IsEnabled && !IsRunning) 
			{
				if(HasHandler && !this.Handler.Start(this)) 
					return false;
				else 
					return true;
			} 
			else 
				return false;
		}

		public void Stop() 
		{
		}

		protected override void Free()
		{
			if(HasParent) 
			{
				Parent.RemoveChildScope(this);
			}
			if(HasHandler) 
			{
				Handler.Stop(this);
				// TODO:  kill all child scopes
			}
		}

		#region IScope Members

		public bool Connect(IConnection connection)
		{
			return Connect(connection, null);
		}

		public bool Connect(IConnection connection, object[] parameters)
		{
			if(HasParent && !this.Parent.Connect(connection, parameters)) 
				return false;
			if(HasHandler && !this.Handler.Connect(connection, this, parameters))
				return false;
			IClient client = connection.Client;
			if(!_clients.ContainsKey(client))
			{
				if(HasHandler && !this.Handler.Join(client, this)) 
					return false;
				Hashtable connections = new Hashtable();
				connections.Add(connection, connection);
				_clients[connection.Client] = connections;
				if( _log != null && _log.IsDebugEnabled )
					_log.Debug("Adding client");
			}
			else
			{
				Hashtable connections = _clients[client] as Hashtable;
				connections.Add(connection, connection);
			}
			AddEventListener(connection);
			return true;
		}

		public void Disconnect(IConnection connection)
		{
			// We call the disconnect handlers in reverse order they were called
			// during connection, i.e. roomDisconnect is called before
			// appDisconnect.
			IClient client = connection.Client;
			if(_clients.ContainsKey(client)) 
			{
				Hashtable connections = _clients[client] as Hashtable;
				connections.Remove(connection);
				IScopeHandler handler = null;
				if(HasHandler) 
				{
					handler = this.Handler;
					try 
					{
						handler.Disconnect(connection, this);
					} 
					catch(Exception ex)
					{
						if( _log != null && _log.IsErrorEnabled )
							_log.Error("Error while executing \"disconnect\" for connection " + connection + " on handler " + handler, ex);
					}
				}

				if(connections.Count == 0)
				{
					_clients.Remove(client);
					if(handler != null) 
					{
						try 
						{
							// there may be a timeout here ?
							handler.Leave(client, this);
						} 
						catch (Exception ex)
						{
							if( _log != null && _log.IsErrorEnabled )
								_log.Error("Error while executing \"leave\" for client " + client + " on handler " + handler, ex);
						}
					}
				}
				RemoveEventListener(connection);
			}
			if(HasParent)
				this.Parent.Disconnect(connection);
		}

		public IContext Context
		{
			get
			{
				if(!HasContext && HasParent) 
					return Parent.Context;
				else
					return _context;
			}
			set
			{
				_context = value;
			}
		}

		public bool HasChildScope(string name)
		{
			return _children.ContainsKey(TypeName + Separator + name);
		}

		public bool HasChildScope(string type, string name)
		{
			return _children.ContainsKey(type + Separator + name);
		}

		public bool CreateChildScope(string name)
		{
			Scope scope = new Scope(name);
			scope.Parent = this;
			return AddChildScope(scope);
		}

		public bool AddChildScope(IBasicScope scope)
		{
			if(HasHandler && !Handler.AddChildScope(scope)) 
			{
				if( _log != null && _log.IsDebugEnabled )
					_log.Debug("Failed to add child scope: " + scope + " to " + this);
				return false;
			}
			if(scope is IScope) 
			{
				// Start the scope
				if(HasHandler && !Handler.Start((IScope) scope)) 
				{
					if( _log != null && _log.IsDebugEnabled )
						_log.Debug("Failed to start child scope: " + scope + " in " + this);
					return false;
				}
			}
			if( _log != null && _log.IsDebugEnabled )
				_log.Debug("Add child scope: " + scope + " to " + this);
			_children[scope.Type + Separator + scope.Name] = scope;
			return true;
		}

		public void RemoveChildScope(IBasicScope scope)
		{
			if (scope is IScope) 
			{
				if(HasHandler)
					this.Handler.Stop((IScope) scope);				
			}
			_children.Remove(scope.Type + Separator + scope.Name);
			if(HasHandler)
			{
				if( _log != null && _log.IsDebugEnabled )
					_log.Debug("Remove child scope");
				this.Handler.RemoveChildScope(scope);
			}
		}

		public ICollection GetScopeNames()
		{
			return _children.Keys;
		}

		public IEnumerator GetBasicScopeNames(string type)
		{
			if (type == null) 
				return _children.Keys.GetEnumerator();
			else
				return new PrefixFilteringStringEnumerator(_children.Keys, type + Separator);
		}

		public IBasicScope GetBasicScope(string type, string name)
		{
			return _children[type + Separator + name] as IBasicScope;
		}

		public IScope GetScope(string name)
		{
			return _children[TypeName + Separator + name] as IScope;
		}

		public ICollection GetClients()
		{
			return _clients.Keys;
		}

		public bool HasHandler
		{
			get
			{
				return (_handler != null || (this.HasParent && this.Parent.HasHandler));
			}
		}

		public IScopeHandler Handler
		{
			get
			{ 
				if(_handler != null) 
				{
					return _handler;
				} 
				else if (HasParent) 
				{
					return Parent.Handler;
				} 
				else 
					return null;
			}
			set
			{ 
				_handler = value; 
				if (_handler is IScopeAware)
					(_handler as IScopeAware).SetScope(this);
			}
		}

		public string ContextPath
		{
			get
			{
				if( HasContext )
				{
					return string.Empty;
				} else if (HasParent){
					return Parent.ContextPath + "/" + Name;				
				} else {
					return null;
				}
			}
		}

		#endregion

		#region IBasicScope Members

		#endregion

		protected Hashtable GetServiceHandlers()
		{
			return GetAttribute(ServiceHandlers, new Hashtable()) as Hashtable;
		}

		#region IServiceHandlerProvider Members

		/// <summary>
		/// Register an object that provides methods which can be called from a client.
		/// 
		/// <p>
		/// Example:<br/>
		/// If you registered a handler with the name "<code>one.two</code>" that
		/// provides a method "<code>callMe</code>", you can call a method
		/// "<code>one.two.callMe</code>" from the client.</p>
		/// </summary>
		/// <param name="name">The name of the handler.</param>
		/// <param name="handler">The handler object.</param>
		public void RegisterServiceHandler(string name, object handler)
		{
			Hashtable serviceHandlers = GetServiceHandlers();
			serviceHandlers.Add(name, handler);
		}
		/// <summary>
		/// Unregister service handler.
		/// </summary>
		/// <param name="name">The name of the handler.</param>
		public void UnregisterServiceHandler(string name)
		{
			Hashtable serviceHandlers = GetServiceHandlers();
			serviceHandlers.Remove(name);
		}
		/// <summary>
		/// Returns a previously registered service handler.
		/// </summary>
		/// <param name="name">The name of the handler to return.</param>
		/// <returns></returns>
		public object GetServiceHandler(string name)
		{
			Hashtable serviceHandlers = GetServiceHandlers();
			return serviceHandlers[name];
		}
		/// <summary>
		/// Gets a list of registered service handler names.
		/// </summary>
		/// <returns>Return the names of the registered handlers.</returns>
		public ICollection GetServiceHandlerNames()
		{
			Hashtable serviceHandlers = GetServiceHandlers();
			return serviceHandlers.Keys;
		}


		#endregion

	}

	public sealed class PrefixFilteringStringEnumerator : IEnumerator
	{
		private string _prefix;
		private int _index;
		private object[] _enumerable = null;
		private string _currentElement;


		internal PrefixFilteringStringEnumerator(ICollection enumerable, string prefix)
		{
			_prefix = prefix;
			_index = -1;
			enumerable.CopyTo(_enumerable, 0);
		}

		#region IEnumerator Members

		public void Reset()
		{
			_currentElement = null;
			_index = -1;
		}

		public string Current
		{
			get
			{
				if(_index == -1)
					throw new InvalidOperationException("Enum not started.");
				if(_index >= _enumerable.Length)
					throw new InvalidOperationException("Enumeration ended.");
				return _currentElement;
			}
		}

		object IEnumerator.Current
		{
			get
			{
				if(_index == -1)
					throw new InvalidOperationException("Enum not started.");
				if(_index >= _enumerable.Length)
					throw new InvalidOperationException("Enumeration ended.");
				return _currentElement;
			}
		}

		public bool MoveNext()
		{
			while(_index < _enumerable.Length - 1)
			{
				_index++;

				string element = _enumerable[_index] as string;
				if( element.StartsWith(_prefix) )
				{
					_currentElement = element;
					return true;
				}
			}
			_index = _enumerable.Length;
			return false;
		}

		#endregion
	}

}
