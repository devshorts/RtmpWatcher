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

using com.TheSilentGroup.Fluorine.AMF3;

using com.TheSilentGroup.Fluorine.Messaging.Config;
using com.TheSilentGroup.Fluorine.Messaging.Services;
using com.TheSilentGroup.Fluorine.Messaging;
using com.TheSilentGroup.Fluorine.Messaging.Messages;
using com.TheSilentGroup.Fluorine.Data.Messages;

namespace com.TheSilentGroup.Fluorine.Data
{
	/// <summary>
	/// Summary description for DataService.
	/// </summary>
	internal class DataService : MessageService
	{
		public DataService(IApplicationContext applicationContext, MessageBroker messageBroker, ServiceSettings serviceSettings) : base(applicationContext, messageBroker, serviceSettings)
		{
		}

		protected override Destination NewDestination(DestinationSettings destinationSettings)
		{
			return new DataDestination(this, destinationSettings);
		}

		public override object ServiceMessage(IApplicationContext applicationContext, IMessage message)
		{
			MessageContext messageContext = new MessageContext(applicationContext);
			return InternalServiceMessage(messageContext, message);
		}

		public object InternalServiceMessage(MessageContext messageContext, IMessage message)
		{
			messageContext.PushIncomingMessage(message);
			CommandMessage commandMessage = message as CommandMessage;
			if( commandMessage != null )
			{
				return base.ServiceMessage(messageContext.ApplicationContext, commandMessage);
			}
			else
			{
				AsyncMessage responseMessage = null;
				DataMessage dataMessage = message as DataMessage;
				switch(dataMessage.operation)
				{
					case DataMessage.FillOperation:
						responseMessage = ExecuteFillOperation(messageContext.ApplicationContext, messageContext, message);
						break;
					case DataMessage.BatchedOperation:
						break;
					case DataMessage.MultiBatchOperation:
						break;
					case DataMessage.TransactedOperation:
						responseMessage = ExecuteTransactedOperation(messageContext.ApplicationContext, messageContext, message);
						break;
					case DataMessage.UpdateOperation:
						responseMessage = ExecuteUpdateOperation(messageContext.ApplicationContext, messageContext, message);
						break;
					case DataMessage.CreateOperation:
						responseMessage = ExecuteCreateOperation(messageContext.ApplicationContext, messageContext, message);
						break;
					case DataMessage.CreateAndSequenceOperation:
						responseMessage = ExecuteCreateAndSequenceOperation(messageContext.ApplicationContext, messageContext, message);
						break;
					case DataMessage.DeleteOperation:
						responseMessage = ExecuteDeleteOperation(messageContext.ApplicationContext, messageContext, message);
						break;
					case DataMessage.UpdateCollectionOperation:
						responseMessage = ExecuteUpdateCollectionOperation(messageContext.ApplicationContext, messageContext, message);
						break;
					case DataMessage.PageItemsOperation:
						responseMessage = ExecutePageItemsOperation(messageContext.ApplicationContext, messageContext, message);
						break;
					case DataMessage.PageOperation:
						responseMessage = ExecutePageOperation(messageContext.ApplicationContext, messageContext, message);
						break;
					case DataMessage.ReleaseCollectionOperation:
						responseMessage = ExecuteReleaseCollectionOperation(messageContext.ApplicationContext, messageContext, message);
						break;
					case DataMessage.ReleaseItemOperation:
						responseMessage = ExecuteReleaseItemOperation(messageContext.ApplicationContext, messageContext, message);
						break;
					default:
						responseMessage = new AcknowledgeMessage();
						break;
				}
				responseMessage.clientId = message.clientId;
				responseMessage.correlationId = message.messageId;
				return responseMessage;
			}
		}

		private AcknowledgeMessage ExecuteFillOperation(IApplicationContext applicationContext, MessageContext messageContext, IMessage message)
		{
			DataMessage dataMessage = message as DataMessage;
			AcknowledgeMessage responseMessage = null;
			DataDestination dataDestination = this.GetDestination(dataMessage) as DataDestination;
			IList collection = dataDestination.ServiceAdapter.Invoke(messageContext.ApplicationContext, message) as IList;
			IList parameters = message.body as IList;
			responseMessage = dataDestination.CreateSequencedMessage(messageContext, dataMessage, collection, parameters);
			return responseMessage;
		}

		private DataMessage ExecuteUpdateOperation(IApplicationContext applicationContext, MessageContext messageContext, IMessage message)
		{
			DataMessage dataMessage = message as DataMessage;
			DataDestination dataDestination = this.GetDestination(dataMessage) as DataDestination;
			dataDestination.ServiceAdapter.Invoke(messageContext.ApplicationContext, message);
			return dataMessage;
		}

		private DataMessage ExecuteCreateOperation(IApplicationContext applicationContext, MessageContext messageContext, IMessage message)
		{
			DataMessage dataMessage = message as DataMessage;
			DataDestination dataDestination = this.GetDestination(dataMessage) as DataDestination;
			dataDestination.ServiceAdapter.Invoke(messageContext.ApplicationContext, message);
			return dataMessage;
		}

		private AsyncMessage ExecuteCreateAndSequenceOperation(IApplicationContext applicationContext, MessageContext messageContext, IMessage message)
		{
			DataMessage dataMessage = message as DataMessage;
			DataDestination dataDestination = this.GetDestination(dataMessage) as DataDestination;
			dataMessage.operation = DataMessage.CreateOperation;
			IList parameters = new ArrayList();
			//Execute a Create operation
			dataDestination.ServiceAdapter.Invoke(messageContext.ApplicationContext, message);
			IList result = new ArrayList();
			result.Add(message.body);
			SequencedMessage sequencedMessage = dataDestination.CreateSequencedMessage(messageContext, dataMessage, result, parameters);
			dataMessage.operation = DataMessage.CreateAndSequenceOperation;
			dataMessage.identity = new Hashtable(0);
			return sequencedMessage;
		}

		private DataMessage ExecuteDeleteOperation(IApplicationContext applicationContext, MessageContext messageContext, IMessage message)
		{
			DataMessage dataMessage = message as DataMessage;
			DataDestination dataDestination = this.GetDestination(dataMessage) as DataDestination;
			dataDestination.ServiceAdapter.Invoke(messageContext.ApplicationContext, message);
			return dataMessage;
		}

		private DataMessage ExecuteUpdateCollectionOperation(IApplicationContext applicationContext, MessageContext messageContext, IMessage message)
		{
			UpdateCollectionMessage updateCollectionMessage = message as UpdateCollectionMessage;
			DataDestination dataDestination = this.GetDestination(updateCollectionMessage) as DataDestination;
			IList updateCollectionRanges = updateCollectionMessage.body as IList;
			for(int i = 0; i < updateCollectionRanges.Count; i++)
			{
				UpdateCollectionRange updateCollectionRange = updateCollectionRanges[i] as UpdateCollectionRange;
				IList identities = updateCollectionRange.identities;
				for(int j = 0; j < identities.Count; j++)
				{
					string messageId = identities[j] as string;
					if( messageId != null )
					{
						IMessage refMessage = messageContext.GetIncomingMessage(messageId);
						DataMessage dataMessage = refMessage as DataMessage;
						if( dataMessage != null )
						{
							identities[j] = Identity.GetIdentity(dataMessage.body, dataDestination);
						}
					}
					else
					{
						Hashtable identityMap = identities[j] as Hashtable;
						if( identityMap != null )
							identities[j] = new Identity(identityMap);
					}
				}
				//Handle identities
			}
			return updateCollectionMessage;
		}

		private AcknowledgeMessage ExecutePageItemsOperation(IApplicationContext applicationContext, MessageContext messageContext, IMessage message)
		{
			DataMessage dataMessage = message as DataMessage;
			DataDestination dataDestination = this.GetDestination(dataMessage) as DataDestination;
			return dataDestination.GetPageItems(dataMessage);
		}

		private AcknowledgeMessage ExecutePageOperation(IApplicationContext applicationContext, MessageContext messageContext, IMessage message)
		{
			DataMessage dataMessage = message as DataMessage;
			DataDestination dataDestination = this.GetDestination(dataMessage) as DataDestination;
			return dataDestination.GetPage(dataMessage);
		}

		private AcknowledgeMessage ExecuteTransactedOperation(IApplicationContext applicationContext, MessageContext messageContext, IMessage message)
		{
			AcknowledgeMessage responseMessage = null;
			DataMessage dataMessage = message as DataMessage;
			IList messages = dataMessage.body as IList;
			ArrayList outMessages = new ArrayList(messages.Count);
			for(int i = 0; i < messages.Count; i++)
			{
				DataMessage batchMessage = messages[i] as DataMessage;

				object result = this.InternalServiceMessage(messageContext, batchMessage);
				outMessages.Add(result);
			}
			
			outMessages.AddRange(messageContext.GetOutgoingMessages());
			
			responseMessage = new AcknowledgeMessage();
			responseMessage.body = outMessages.ToArray(typeof(object));
			return responseMessage;
		}

		private AcknowledgeMessage ExecuteReleaseCollectionOperation(IApplicationContext applicationContext, MessageContext messageContext, IMessage message)
		{
			AcknowledgeMessage responseMessage = new AcknowledgeMessage();
			DataMessage dataMessage = message as DataMessage;
			DataDestination dataDestination = this.GetDestination(dataMessage) as DataDestination;
			dataDestination.ReleaseCollectionOperation(applicationContext, dataMessage);
			return responseMessage;
		}

		private AcknowledgeMessage ExecuteReleaseItemOperation(IApplicationContext applicationContext, MessageContext messageContext, IMessage message)
		{
			AcknowledgeMessage responseMessage = new AcknowledgeMessage();
			DataMessage dataMessage = message as DataMessage;
			DataDestination dataDestination = this.GetDestination(dataMessage) as DataDestination;
			dataDestination.ReleaseItemOperation(dataMessage);
			return responseMessage;
		}		
	}
}
