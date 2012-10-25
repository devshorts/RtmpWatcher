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

using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api.Event;
using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api.Service;

namespace com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api
{
	/// <summary>
	/// Summary description for IScopeHandler.
	/// </summary>
	public interface IScopeHandler : IEventHandler
	{
		/// <summary>
		/// Called when a scope is created for the first time.
		/// </summary>
		/// <param name="scope">The new scope object.</param>
		/// <returns><code>true</code> to allow, <code>false</code> to deny.</returns>
		bool Start(IScope scope);
		/// <summary>
		/// Called just before a scope is disposed.
		/// </summary>
		/// <param name="scope"></param>
		void Stop(IScope scope);
		/// <summary>
		/// Called just before every connection to a scope. You can pass additional
		/// parameters from client using <code>NetConnection.connect</code> method (see
		/// below).
		/// </summary>
		/// <param name="connection">Connection object.</param>
		/// <param name="scope"></param>
		/// <param name="parameters">List of params passed from client via <code>NetConnection.connect</code> method. All parameters but the first one passed to <code>NetConnection.connect</code> method are available as parameters array.</param>
		/// <returns><code>true</code> to allow, <code>false</code> to deny.</returns>
		bool Connect(IConnection connection, IScope scope, object[] parameters);
		/// <summary>
		/// Called just after the a connection is disconnected.
		/// </summary>
		/// <param name="connection">Connection object.</param>
		/// <param name="scope">Scope object.</param>
		void Disconnect(IConnection connection, IScope scope);
		/// <summary>
		/// Called just before a child scope is added.
		/// </summary>
		/// <param name="scope">Scope that will be added.</param>
		/// <returns></returns>
		bool AddChildScope(IBasicScope scope);
		/// <summary>
		/// Called just after a child scope has been removed.
		/// </summary>
		/// <param name="scope">Scope that has been removed.</param>
		void RemoveChildScope(IBasicScope scope);
		/// <summary>
		/// Called just before a client enters the scope.
		/// </summary>
		/// <param name="client">Client object.</param>
		/// <param name="scope"></param>
		/// <returns><code>true</code> to allow, <code>false</code> to deny connection.</returns>
		bool Join(IClient client, IScope scope);
		/// <summary>
		/// Called just after the client leaves the scope.
		/// </summary>
		/// <param name="client">Client object.</param>
		/// <param name="scope">Scope object.</param>
		void Leave(IClient client, IScope scope);
		/// <summary>
		/// Called when a service is called.
		/// </summary>
		/// <param name="connection">Connection object.</param>
		/// <param name="call">Call object.</param>
		/// <returns><code>true</code> to allow, <code>false</code> to deny.</returns>
		bool ServiceCall(IConnection connection, IServiceCall call);
	}
}
