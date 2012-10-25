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

namespace com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api
{
	/// <summary>
	/// Summary description for IConnection.
	/// </summary>
	public interface IConnection : ICoreObject
	{
		string Type{ get; }
		void Initialize(IClient client);
		bool Connect(IScope scope);
		bool Connect(IScope scope, object[] args);
		bool IsConnected { get; }
		void Close();
		Hashtable ConnectionParameters{ get; }
		IClient Client { get; }
		IScope Scope { get; }
		string Host { get; }
		string RemoteAddress { get; }
		int RemotePort { get; }
		string Path { get; }
		ICollection BasicScopes { get; }
		string SessionId{ get; }
		ObjectEncoding ObjectEncoding{ get; set; }
	}
}
