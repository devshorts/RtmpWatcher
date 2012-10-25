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

namespace com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api
{
	/// <summary>
	/// Summary description for IServer.
	/// </summary>
	public interface IServer
	{
		/// <summary>
		/// Gets the global scope with a given name.
		/// </summary>
		/// <param name="name">The name of the global scope.</param>
		/// <returns></returns>
		IGlobalScope GetGlobal(string name);
		/// <summary>
		/// Register a global scope.
		/// </summary>
		/// <param name="scope">The global scope to register.</param>
		void RegisterGlobal(IGlobalScope scope);
		/// <summary>
		/// Lookup the global scope for a host.
		/// </summary>
		/// <param name="hostName">The name of the host.</param>
		/// <param name="contextPath">The path in the host.</param>
		/// <returns></returns>
		IGlobalScope LookupGlobal(string hostName, string contextPath);
		/// <summary>
		/// Map a virtual hostname and a path to the name of a global scope.
		/// </summary>
		/// <param name="hostName">The name of the host to map.</param>
		/// <param name="contextPath"></param>
		/// <param name="globalName">The name of the global scope to map to.</param>
		/// <returns></returns>
		bool AddMapping(string hostName, string contextPath, string globalName);
		/// <summary>
		/// Unregister a previously mapped global scope.
		/// </summary>
		/// <param name="hostName"></param>
		/// <param name="contextPath"></param>
		/// <returns></returns>
		bool RemoveMapping(string hostName, string contextPath);
		/// <summary>
		/// Gets list of global scope names.
		/// </summary>
		/// <returns></returns>
		ICollection GetGlobalNames();
		/// <summary>
		/// Get list of global scopes.
		/// </summary>
		/// <returns></returns>
		ICollection GetGlobalScopes();
	}
}
