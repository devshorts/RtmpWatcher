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

using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api;

namespace com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp
{
	/// <summary>
	/// Summary description for ClientRegistry.
	/// </summary>
	public class ClientRegistry : IClientRegistry
	{
		private Hashtable _idToClient = new Hashtable();
		private int _nextId = 0;

		public ClientRegistry()
		{
		}

		public string GetNextId()
		{
			lock(this)
			{
				return string.Empty + _nextId++;
			}
		}

		protected void AddClient(IClient client) 
		{
			_idToClient.Add(client.Id, client);
		}

		internal void RemoveClient(IClient client) 
		{
			_idToClient.Remove(client.Id);
		}

		protected ICollection GetClients() 
		{
			return _idToClient.Values;
		}

		#region IClientRegistry Members

		public bool HasClient(string id)
		{
			if( id == null )
				return false;
			return _idToClient.ContainsKey(id);
		}

		public IClient NewClient(object[] parameters)
		{
			IClient client = new Client(GetNextId(), this);
			AddClient(client);
			return client;
		}

		public IClient LookupClient(string id)
		{
			if(HasClient(id)) 
				return _idToClient[id] as IClient;
			return null;
		}

		#endregion
	}
}
