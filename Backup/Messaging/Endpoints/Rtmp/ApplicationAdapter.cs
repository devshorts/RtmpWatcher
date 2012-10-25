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

namespace com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp
{
	/// <summary>
	/// Summary description for ApplicationAdapter.
	/// </summary>
	public class ApplicationAdapter : AdapterBase
	{
		public ApplicationAdapter()
		{
		}

		public override bool Connect(IConnection connection, IScope scope, object[] parameters)
		{
			if( !base.Connect (connection, scope, parameters) )
				return false;
			bool success = false;
			if(BasicScope.IsApplication(scope)) 
			{
				success = AppConnect(connection, parameters);
			} 
			else if (BasicScope.IsRoom(scope)) 
			{
				success = RoomConnect(connection, parameters);
			}
			return success;
		}

		/// <summary>
		/// Handler method. Called every time new client connects (that is, new
		/// IConnection object is created after call from a SWF movie) to the
		/// application.
		/// 
		/// You override this method to pass additional data from client to server
		/// application using <code>NetConnection.connect</code> method.
		/// </summary>
		/// <param name="conn"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public bool AppConnect(IConnection conn, Object[] parameters) 
		{
			//log.debug("appConnect: " + conn);
			/*
			for (IApplication listener : listeners) 
			{
				listener.appConnect(conn, params);
			}
			*/
			return true;
		}
		/// <summary>
		/// Handler method. Called every time new client connects (that is, new
		/// IConnection object is created after call from a SWF movie) to the
		/// application.
		/// </summary>
		/// <param name="conn"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public bool RoomConnect(IConnection conn, Object[] parameters) 
		{
			//log.debug("roomConnect: " + conn);
			/*
			for (IApplication listener : listeners) 
			{
				listener.roomConnect(conn, params);
			}
			*/
			return true;
		}

	}
}
