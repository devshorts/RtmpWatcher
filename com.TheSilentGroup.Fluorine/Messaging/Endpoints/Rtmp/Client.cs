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
	/// Summary description for Client.
	/// </summary>
	public class Client : AttributeStore, IClient
	{
		protected string _id;
		protected long _creationTime;
		protected Hashtable _connectionToScope = new Hashtable();
		protected ClientRegistry _registry;

		public Client(string id, ClientRegistry registry)
		{
			_id = id;
			_registry = registry;
			_creationTime = System.Environment.TickCount;
		}

		internal void Register(IConnection connection) 
		{
			_connectionToScope.Add(connection, connection.Scope);
		}

		internal void Unregister(IConnection connection) 
		{
			_connectionToScope.Remove(connection);
			if(_connectionToScope.Count == 0) 
			{
				// This client is not connected to any scopes, remove from registry.
				_registry.RemoveClient(this);
			}
		}


		#region IClient Members

		public string Id
		{
			get
			{
				return _id;
			}
		}

		public ICollection Scopes
		{
			get
			{
				return _connectionToScope.Values;
			}
		}

		public ICollection Connections
		{
			get
			{
				return _connectionToScope.Keys;
			}
		}

		public void Disconnect()
		{
			foreach(IConnection connection in this.Connections)
			{
				connection.Close();
			}
		}

		#endregion
	}
}
