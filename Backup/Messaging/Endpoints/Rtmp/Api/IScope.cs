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

using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api.Service;

namespace com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api
{
	/// <summary>
	/// Summary description for IScope.
	/// </summary>
	public interface IScope : IBasicScope, IServiceHandlerProvider
	{
		/// <summary>
		/// Adds given connection to the scope.
		/// </summary>
		/// <param name="connection"></param>
		/// <returns></returns>
		bool Connect(IConnection connection);
		bool Connect(IConnection connection, object[] parameters);
		/// <summary>
		/// Removes given connection from list of scope connections. This disconnects
		/// all clients of given connection from the scope.
		/// </summary>
		/// <param name="conn"></param>
		void Disconnect(IConnection conn);
		/// <summary>
		/// Returns scope context.
		/// </summary>
		IContext Context{ get; }
		/// <summary>
		/// Check to see if this scope has a child scope matching a given name.
		/// </summary>
		/// <param name="name">The name of the child scope.</param>
		/// <returns><code>true</code> if a child scope exists, otherwise <code>false</code></returns>
		bool HasChildScope(string name);
		/// <summary>
		/// Checks whether scope has a child scope with given name and type.
		/// </summary>
		/// <param name="type">Child scope type.</param>
		/// <param name="name">Child scope name.</param>
		/// <returns><code>true</code> if a child scope exists, otherwise <code>false</code></returns>
		bool HasChildScope(string type, string name);
		/// <summary>
		/// Creates child scope with name given and returns success value.
		/// </summary>
		/// <param name="name">New child scope name.</param>
		/// <returns><code>true</code> if child scope was successfully creates, <code>false</code> otherwise.</returns>
		bool CreateChildScope(string name);
		/// <summary>
		/// Adds scope as a child scope.
		/// </summary>
		/// <param name="scope">Add the specified scope.</param>
		/// <returns><code>true</code> if child scope was successfully added, <code>false</code> otherwise</returns>
		bool AddChildScope(IBasicScope scope);
		/// <summary>
		/// Removes scope from the children scope list.
		/// </summary>
		/// <param name="scope">Removes the specified scope.</param>
		void RemoveChildScope(IBasicScope scope);
		/// <summary>
		/// Gets the child scope names.
		/// </summary>
		/// <returns></returns>
		ICollection GetScopeNames();
		IEnumerator GetBasicScopeNames(string type);
		/// <summary>
		/// Gets a child scope by name.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="name">Name of the child scope.</param>
		/// <returns></returns>
		IBasicScope GetBasicScope(string type, string name);
		IScope GetScope(string name);
		/// <summary>
		/// Gets a set of connected clients.
		/// </summary>
		/// <returns></returns>
		ICollection GetClients();
		/// <summary>
		/// Checks whether scope has handler or not.
		/// </summary>
		bool HasHandler{ get; }
		/// <summary>
		/// Returns handler of the scope.
		/// </summary>
		IScopeHandler Handler{ get; }
		/// <summary>
		/// Returns context path.
		/// </summary>
		string ContextPath{ get; }
	}
}
