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
	public class RtmpServer : IServer
	{
		public static string Slash = "/";

		protected Hashtable _globals = new Hashtable();
		protected StringDictionary _mapping = new StringDictionary();

		public RtmpServer()
		{
		}

		protected string GetKey(string hostName, string contextPath) 
		{
			if (hostName == null) 
				hostName = String.Empty;
			if (contextPath == null) 
				contextPath = String.Empty;
			return hostName + Slash + contextPath;
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
			string key = GetKey(hostName, contextPath);
			while(contextPath.IndexOf(Slash) != -1) 
			{
				key = GetKey(hostName, contextPath);
				if(_mapping.ContainsKey(key)) 
				{
					return GetGlobal(_mapping[key] as string);
				}
				contextPath = contextPath.Substring(0, contextPath.LastIndexOf(Slash));
			}
			key = GetKey(hostName, contextPath);
			if(_mapping.ContainsKey(key)) 
			{
				return GetGlobal(_mapping[key] as string);
			}
			key = GetKey(String.Empty, contextPath);
			//log.debug("Check wildcard host with path: " + key);
			if(_mapping.ContainsKey(key)) 
			{
				return GetGlobal(_mapping[key] as string);
			}
			key = GetKey(hostName, String.Empty);
			//log.debug("Check host with no path: " + key);
			if(_mapping.ContainsKey(key)) 
			{
				return GetGlobal(_mapping[key] as string);
			}
			key = GetKey(String.Empty, String.Empty);
			//log.debug("Check default host, default path: " + key);
			return GetGlobal(_mapping[key] as string);
		}

		public bool AddMapping(string hostName, string contextPath, string globalName)
		{
			string key = GetKey(hostName, contextPath);
			//log.debug("Add mapping: " + key + " => " + globalName);
			if(_mapping.ContainsKey(key)) 
				return false;
			_mapping.Add(key, globalName);
			return true;
		}

		public bool RemoveMapping(string hostName, string contextPath)
		{
			string key = GetKey(hostName, contextPath);
			//log.debug("Remove mapping: " + key);
			if(!_mapping.ContainsKey(key)) 
				return false;
			_mapping.Remove(key);
			return true;
		}

		public System.Collections.ICollection GetGlobalNames()
		{
			return _globals.Keys;
		}

		public System.Collections.ICollection GetGlobalScopes()
		{
			return _globals.Values;
		}

		#endregion
	}
}
