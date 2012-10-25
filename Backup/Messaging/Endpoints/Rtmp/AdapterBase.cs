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

using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api;
using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api.Event;

namespace com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp
{
	/// <summary>
	/// Summary description for AdapterBase.
	/// </summary>
	public class AdapterBase : IScopeHandler, IScopeAware, IAttributeStore
	{
		protected IScope _scope;

		public AdapterBase()
		{
		}

		public IScope Scope
		{
			get{ return _scope; }
		}

		#region IScopeHandler Members

		public virtual bool Start(IScope scope)
		{
			// TODO:  Add AdapterBase.Start implementation
			return false;
		}

		public virtual void Stop(IScope scope)
		{
			// TODO:  Add AdapterBase.Stop implementation
		}

		public virtual bool Connect(IConnection connection, IScope scope, object[] parameters)
		{
			return true;
		}

		public virtual void Disconnect(IConnection connection, IScope scope)
		{
			// TODO:  Add AdapterBase.Disconnect implementation
		}

		public virtual bool AddChildScope(IBasicScope scope)
		{
			// TODO:  Add AdapterBase.AddChildScope implementation
			return false;
		}

		public virtual void RemoveChildScope(IBasicScope scope)
		{
			// TODO:  Add AdapterBase.RemoveChildScope implementation
		}

		public virtual bool Join(IClient client, IScope scope)
		{
			// TODO:  Add AdapterBase.Join implementation
			return false;
		}

		public virtual void Leave(IClient client, IScope scope)
		{
			// TODO:  Add AdapterBase.Leave implementation
		}

		public virtual bool ServiceCall(IConnection connection, com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api.Service.IServiceCall call)
		{
			// TODO:  Add AdapterBase.ServiceCall implementation
			return false;
		}

		#endregion

		#region IEventHandler Members

		public bool HandleEvent(IEvent evt)
		{
			// TODO:  Add AdapterBase.HandleEvent implementation
			return false;
		}

		#endregion

		#region IScopeAware Members

		public void SetScope(IScope scope)
		{
			_scope = scope;
		}

		#endregion

		#region IAttributeStore Members

		public ICollection GetAttributeNames()
		{
			return _scope.GetAttributeNames();
		}

		public bool SetAttribute(string name, object value)
		{
			return _scope.SetAttribute(name, value);;
		}

		public void SetAttributes(StringDictionary values)
		{
			_scope.SetAttributes(values);
		}

		public void SetAttributes(IAttributeStore values)
		{
			_scope.SetAttributes(values);
		}

		public object GetAttribute(string name)
		{
			return _scope.GetAttribute(name);
		}

		public object GetAttribute(string name, object defaultValue)
		{
			return _scope.GetAttribute(name, defaultValue);
		}

		public bool HasAttribute(string name)
		{
			return _scope.HasAttribute(name);
		}

		public bool RemoveAttribute(string name)
		{
			return _scope.RemoveAttribute(name);
		}

		public void RemoveAttributes()
		{
			_scope.RemoveAttributes();
		}

		#endregion
	}
}
