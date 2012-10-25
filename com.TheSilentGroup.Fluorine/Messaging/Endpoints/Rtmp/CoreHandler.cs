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

using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api;
using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api.Service;

namespace com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp
{
	/// <summary>
	/// Summary description for CoreHandler.
	/// </summary>
	class CoreHandler : IScopeHandler
	{
		public CoreHandler()
		{
	}

		#region IScopeHandler Members

		public bool Start(IScope scope)
		{
			return true;
		}

		public void Stop(IScope scope)
		{
			//NA
		}

		public bool Connect(IConnection connection, IScope scope, object[] parameters)
		{
			//log.debug("Connect to core handler ?");
			string id = connection.SessionId;

			// Use client registry from scope the client connected to.
			//IScope connectionScope = Red5.getConnectionLocal().getScope();
			IScope connectionScope = connection.Scope;
			IClientRegistry clientRegistry = connectionScope.Context.GetClientRegistry();
			IClient client = clientRegistry.HasClient(id) ? clientRegistry.LookupClient(id) : clientRegistry.NewClient(parameters);
			// We have a context, and a client object.. time to init the conneciton.
			connection.Initialize(client);
			// we could checked for banned clients here 
			return true;
		}

		public void Disconnect(IConnection connection, IScope scope)
		{
			//NA
		}

		public bool AddChildScope(IBasicScope scope)
		{
			return true;
		}

		public void RemoveChildScope(IBasicScope scope)
		{
			//NA
		}

		public bool Join(IClient client, IScope scope)
		{
			return true;
		}

		public void Leave(IClient client, IScope scope)
		{
			//NA
		}

		public bool ServiceCall(IConnection connection, IServiceCall call)
		{
			IContext context = connection.Scope.Context;
			if(call.ServiceName != null) 
			{
				context.GetServiceInvoker().Invoke(call, context);
			} 
			else 
			{
				context.GetServiceInvoker().Invoke(call, connection.Scope.Handler);
			}
			return true;
		}

		#endregion

		#region IEventHandler Members

		public bool HandleEvent(com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api.Event.IEvent evt)
		{
			return false;
		}

		#endregion
	}
}
