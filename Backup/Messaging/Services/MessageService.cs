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

using com.TheSilentGroup.Fluorine.Messaging.Config;
using com.TheSilentGroup.Fluorine.Messaging.Messages;
using com.TheSilentGroup.Fluorine.Messaging.Services.Messaging;

namespace com.TheSilentGroup.Fluorine.Messaging.Services
{
	/// <summary>
	/// The MessageService class is the Service implementation that manages 
	/// point-to-point and publish-subscribe messaging.
	/// </summary>
	internal class MessageService : ServiceBase
	{
		public MessageService(IApplicationContext applicationContext, MessageBroker messageBroker, ServiceSettings serviceSettings) : base(applicationContext, messageBroker, serviceSettings)
		{
		}

		protected override Destination NewDestination(DestinationSettings destinationSettings)
		{
			return new MessageDestination(this, destinationSettings);
		}


		public override object ServiceMessage(IApplicationContext applicationContext, IMessage message)
		{
			CommandMessage commandMessage = message as CommandMessage;
			MessageDestination messageDestination = GetDestination(message) as MessageDestination;
			if( commandMessage != null )
			{
				object clientId = commandMessage.clientId;
				MessageClient client = messageDestination.SubscriptionManager.GetSubscriber(clientId);
			
				switch(commandMessage.operation)
				{
					case CommandMessage.SubscribeOperation:
						if( client != null )
						{
							//TODO
							return new AcknowledgeMessage();
						}
						else
						{
							client = new MessageClient(clientId, messageDestination);
							client.AddEndpoint(commandMessage.GetHeader("DSEndpoint") as string);
							messageDestination.SubscriptionManager.AddSubscriber(client);
							return new AcknowledgeMessage();
						}
					case CommandMessage.UnsubscribeOperation:
						if( client != null )
							messageDestination.SubscriptionManager.RemoveSubscriber(client);
						return new AcknowledgeMessage();
					case CommandMessage.PollOperation:
						//TODO just acknowlede everything right now
						return new AcknowledgeMessage();
					default:
						return new AcknowledgeMessage();
				}
			}
			else
			{
				object result = messageDestination.ServiceAdapter.Invoke(applicationContext, message);
				return result;
			}
		}

	}
}
