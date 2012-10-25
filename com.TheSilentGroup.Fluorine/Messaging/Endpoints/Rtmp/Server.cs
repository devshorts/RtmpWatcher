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

namespace com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp
{
	/// <summary>
	/// Summary description for Server.
	/// </summary>
	public class Server : IServer
	{
		protected Hashtable _globals = new Hashtable();
		protected StringDictionary _mapping = new StringDictionary();

		public Server()
		{
		}

		#region IServer Members

		public IGlobalScope GetGlobal(string name)
		{
			return _globals[name] as IGlobalScope;
		}

		public void RegisterGlobal(IGlobalScope scope)
		{
			_globals.Add(scope.Name, scope);
		}

		public IGlobalScope LookupGlobal(string hostName, string contextPath)
		{
			// TODO:  Add Server.LookupGlobal implementation
			return null;
		}

		public bool AddMapping(string hostName, string contextPath, string globalName)
		{
			return false;
		}

		public bool RemoveMapping(string hostName, string contextPath)
		{
			// TODO:  Add Server.RemoveMapping implementation
			return false;
		}

		public ICollection GetGlobalNames()
		{
			return _globals.Keys;
		}

		public ICollection GetGlobalScopes()
		{
			return _globals.Values;
		}

		#endregion
	}
}
