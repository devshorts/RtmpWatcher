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
using System.Net;

namespace com.TheSilentGroup.Fluorine.SystemHelpers.Net
{
	/// <summary>
	/// Defines the host type.
	/// </summary>
	enum HostType
	{
		Server,
		Client
	}

	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	interface ISocket
	{
		object Tag
		{
			get;
			set;
		}

		/// <summary>
		/// Connection service header.
		/// </summary>
		byte[] Header
		{
			get;
			set;
		}

		/// <summary>
		/// Connection Session Id (GUID).
		/// </summary>
		string ConnectionId
		{
			get;
		}

		/// <summary>
		/// Handle of the OS Socket.
		/// </summary>
		IntPtr SocketHandle
		{
			get;
		}

		/// <summary>
		/// Local socket endpoint.
		/// </summary>
		IPEndPoint LocalEndPoint
		{
			get;
		}

		/// <summary>
		/// Remote socket endpoint.
		/// </summary>
		IPEndPoint RemoteEndPoint
		{
			get;
		}

		/// <summary>
		/// Connection host type.
		/// </summary>
		HostType HostType
		{
			get;
		}


		/// <summary>
		/// Begin send data.
		/// </summary>
		/// <param name="buffer">
		/// Data to be sent.
		/// </param>
		void BeginSend(byte[] buffer);

		/// <summary>
		/// Begin receive the data.
		/// </summary>
		void BeginReceive();

		/// <summary>
		/// Begin disconnect the connection.
		/// </summary>
		void BeginDisconnect();
	}
}
