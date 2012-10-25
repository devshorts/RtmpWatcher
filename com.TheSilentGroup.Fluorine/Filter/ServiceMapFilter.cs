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
// Import log4net classes.
using log4net;
using log4net.Config;

using com.TheSilentGroup.Fluorine.Gateway;
using com.TheSilentGroup.Fluorine.Exceptions;
using com.TheSilentGroup.Fluorine.Diagnostic;
using com.TheSilentGroup.Fluorine.Activation;
using com.TheSilentGroup.Fluorine.Messaging.Security;
using com.TheSilentGroup.Fluorine.Messaging.Services;
using com.TheSilentGroup.Fluorine.Messaging.Messages;
using com.TheSilentGroup.Fluorine.Messaging.Endpoints;
using com.TheSilentGroup.Fluorine.Messaging;

namespace com.TheSilentGroup.Fluorine.Filter
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class ServiceMapFilter : AbstractFilter
	{
		private ILog _log;
		EndpointBase _endpoint;

		/// <summary>
		/// Initializes a new instance of the ServiceMapFilter class.
		/// </summary>
		/// <param name="endpoint"></param>
		public ServiceMapFilter(EndpointBase endpoint)
		{
			try
			{
				_log = LogManager.GetLogger(typeof(ServiceMapFilter));
			}
			catch{}
			_endpoint = endpoint;
		}

		#region IFilter Members

		public override void Invoke(AMFContext context)
		{
			IApplicationContext applicationContext = context.ApplicationContext;
			if( applicationContext != null )
			{
				for(int i = 0; i < context.AMFMessage.BodyCount; i++)
				{
					AMFBody amfBody = context.AMFMessage.GetBodyAt(i);

					if( !amfBody.IsEmptyTarget )
					{//Flash
						if( applicationContext.ServiceMap != null )
						{

							string typeName = amfBody.TypeName;
							string method = amfBody.Method;
							if( typeName != null && applicationContext.ServiceMap.Contains(typeName) )
							{
								string serviceLocation = applicationContext.ServiceMap.GetServiceLocation(typeName);
								method = applicationContext.ServiceMap.GetMethod(typeName, method);
								string target = serviceLocation + "." + method;
								if( _log != null && _log.IsDebugEnabled )
									_log.Debug("Mapping " + amfBody.Target + " to " + target);
								amfBody.Target = target;
							}
						}
					}
					else
					{//Flex
						object content = amfBody.Content;
						if( content is IList )
							content = (content as IList)[0];
						IMessage message = content as IMessage;

						if( message != null )
						{
							if( message is RemotingMessage )//Flex2 RemotingMessage
							{
								RemotingMessage remotingMessage = message as RemotingMessage;

								//Service mapping
								if( applicationContext.ServiceMap != null )
								{
									string typeName = remotingMessage.source;
									string method = remotingMessage.operation;
									if( typeName != null && applicationContext.ServiceMap.Contains(typeName) )
									{
										string serviceLocation = applicationContext.ServiceMap.GetServiceLocation(typeName);
										method = applicationContext.ServiceMap.GetMethod(typeName, method);
										if( _log != null && _log.IsDebugEnabled )
											_log.Debug("Mapping " + remotingMessage.GetMessageSource() + " to " + serviceLocation + "." + method );

										remotingMessage.source = serviceLocation;
										remotingMessage.operation = method;
									}
								}

								if( remotingMessage.source == null )
								{
									//Resolve source
									MessageBroker messageBroker = _endpoint.GetMessageBroker();
									IService service = messageBroker.GetService(applicationContext, remotingMessage);
									Destination destination = service.GetDestination(remotingMessage);
									string source = destination.DestinationSettings.Properties["source"] as string;
									remotingMessage.source = source;
								}
							}
						}
					}
				}
			}
		}

		#endregion
	}
}
