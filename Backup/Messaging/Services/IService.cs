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

using com.TheSilentGroup.Fluorine.Messaging.Messages;
using com.TheSilentGroup.Fluorine.Messaging.Config;

namespace com.TheSilentGroup.Fluorine.Messaging.Services
{
	/// <summary>
	/// The MessageBroker has endpoints on one end and services on the other.
	/// The Service interface defines the contract between the MessageBroker 
	/// and all Service implementations.
	/// </summary>
	internal interface IService
	{
		string id { get; set; }
		/// <summary>
		/// All services must be managed by a single MessageBroker, and must be capable of 
		/// returning a reference to that broker. This broker is used when a service wishes 
		/// to push a message to one or more endpoints managed by the broker. 
		/// </summary>
		/// <returns></returns>
		MessageBroker GetMessageBroker();
		/// <summary>
		/// Retrieves the destination in this service for which the given message is intended.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		Destination GetDestination(IMessage message);
		/// <summary>
		/// Handles a message routed to the service by the MessageBroker.
		/// </summary>
		/// <param name="message">The message sent by the MessageBroker.</param>
		/// <param name="applicationContext">The application context.</param>
		/// <returns></returns>
		object ServiceMessage(IApplicationContext applicationContext, IMessage message);
		/// <summary>
		/// Determines whether this Service is capable of handling a given Message instance.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="applicationContext">The application context.</param>
		/// <returns></returns>
		bool IsSupportedMessage(IApplicationContext applicationContext, IMessage message);
		/// <summary>
		/// Determines whether this Service is capable of handling messages of a given type.
		/// </summary>
		/// <param name="messageClassName"></param>
		/// <param name="applicationContext">The application context.</param>
		/// <returns></returns>
		bool IsSupportedMessageType(IApplicationContext applicationContext, string messageClassName);
		/// <summary>
		/// Performs any startup actions necessary after the service has been added to the broker.
		/// </summary>
		void Start();
		/// <summary>
		/// Performs any actions necessary before removing the service from the broker.
		/// </summary>
		void Stop();

		Destination GetDestination(string id);
		void CheckSecurity(IApplicationContext applicationContext, IMessage message);
		void CheckSecurity(IApplicationContext applicationContext, Destination destination);
		bool DoAuthorization(IApplicationContext applicationContext, string[] roles);
	}
}
