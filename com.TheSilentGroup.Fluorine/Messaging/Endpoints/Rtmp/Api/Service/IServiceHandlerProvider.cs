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

namespace com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api.Service
{
	/// <summary>
	/// Summary description for IServiceHandlerProvider.
	/// </summary>
	public interface IServiceHandlerProvider
	{
		/// <summary>
		/// Register an object that provides methods which can be called from a client.
		/// 
		/// <p>
		/// Example:<br/>
		/// If you registered a handler with the name "<code>one.two</code>" that
		/// provides a method "<code>callMe</code>", you can call a method
		/// "<code>one.two.callMe</code>" from the client.</p>
		/// </summary>
		/// <param name="name">The name of the handler.</param>
		/// <param name="handler">The handler object.</param>
		void RegisterServiceHandler(string name, object handler);
		/// <summary>
		/// Unregister service handler.
		/// </summary>
		/// <param name="name">The name of the handler.</param>
		void UnregisterServiceHandler(string name);
		/// <summary>
		/// Returns a previously registered service handler.
		/// </summary>
		/// <param name="name">The name of the handler to return.</param>
		/// <returns></returns>
		object GetServiceHandler(string name);
		/// <summary>
		/// Gets a list of registered service handler names.
		/// </summary>
		/// <returns>Return the names of the registered handlers.</returns>
		ICollection GetServiceHandlerNames();
	}
}
