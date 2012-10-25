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
using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api.Persistence;
using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api.Service;

namespace com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp
{
	/// <summary>
	/// Summary description for Context.
	/// </summary>
	public class Context : IContext
	{
		private string _contextPath = string.Empty;
		private IScopeResolver _scopeResolver;
		private IClientRegistry _clientRegistry;
		private IServiceInvoker _serviceInvoker;
		//private IMappingStrategy mappingStrategy;
		private IPersistenceStore _persistanceStore;

		public Context(string contextPath, IClientRegistry clientRegistry, IScopeResolver scopeResolver, IPersistenceStore persistanceStore)
		{
			_contextPath = contextPath;
			_clientRegistry = clientRegistry;
			_scopeResolver = scopeResolver;
			_persistanceStore = persistanceStore;
			_serviceInvoker = null;
		}

		#region IContext Members

		public IScope ResolveScope(string path)
		{
			return _scopeResolver.ResolveScope(path);
		}

		public IScope GetGlobalScope()
		{
			return _scopeResolver.GlobalScope;
		}

		public IClientRegistry GetClientRegistry()
		{
			return _clientRegistry;
		}

		public IPersistenceStore GetPersistanceStore()
		{
			return _persistanceStore;
		}

		public IScopeHandler LookupScopeHandler(string contextPath)
		{
			return null;
		}

		public IServiceInvoker GetServiceInvoker()
		{
			return _serviceInvoker;
		}

		#endregion
	}
}
