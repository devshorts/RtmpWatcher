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

namespace com.TheSilentGroup.Fluorine.SystemHelpers.Net
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	interface IServerSocket : ISocket
	{
		/// <summary>
		/// Begin send data to all server connections.
		/// </summary>
		/// <param name="buffer">
		/// Data to be sent.
		/// </param>
		void BeginSendToAll(byte[] buffer);

		/// <summary>
		/// Begin send data to the connection.
		/// </summary>
		/// <param name="connection">
		/// The connection that the data will be sent.
		/// </param>
		/// <param name="buffer">
		/// Data to be sent.
		/// </param>
		void BeginSendTo(ISocket connection, byte[] buffer);


		/// <summary>
		/// Get the connection from the connectionId
		/// </summary>
		/// <param name="connectionId">
		/// The connectionId.
		/// </param>
		/// <returns>
		/// ISocketConnection to use.
		/// </returns>
		ISocket GetConnectionById(string connectionId);
	}
}
