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
using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Exceptions;

namespace com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp
{
	/// <summary>
	/// Summary description for ScopeResolver.
	/// </summary>
	public class ScopeResolver : IScopeResolver
	{
		object _objLock = new object();
		protected IGlobalScope _globalScope;

		public ScopeResolver(IGlobalScope globalScope)
		{
			_globalScope = globalScope;
		}

		public IGlobalScope GlobalScope
		{
			get{ return _globalScope; }
		}

		#region IScopeResolver Members

		public IScope ResolveScope(string path)
		{
			IScope scope = _globalScope;
			if(path == null) 
				return scope;
			string[] parts = path.Split(new char[]{'/'});
			foreach(string element in parts) 
			{
				string room = element;
				if(room == string.Empty) 
				{
					// Skip empty path elements
					continue;
				}

				// Prevent the same subscope from getting created twice
				lock(_objLock) 
				{
					if(scope.HasChildScope(room)) 
					{
						scope = scope.GetScope(room);
					} 
					else if (scope != _globalScope && scope.CreateChildScope(room) )
					{
						scope = scope.GetScope(room);
					} 
					else 
					{
						throw new ScopeNotFoundException(scope, element);
					}
				}
			}
			return scope;
		}

		#endregion
	}
}
