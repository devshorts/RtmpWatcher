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
using System.Security.Principal;
using System.Security;
using System.Web;
using System.Web.Security;
using System.Web.Configuration;
using System.Web.Caching;
using System.Threading;
using System.Reflection;
// Import log4net classes.
using log4net;
using log4net.Config;

using com.TheSilentGroup.Fluorine.Exceptions;
using com.TheSilentGroup.Fluorine.Gateway;
using com.TheSilentGroup.Fluorine.Diagnostic;
using com.TheSilentGroup.Fluorine.Activation;

using com.TheSilentGroup.Fluorine.Messaging.Messages;
using com.TheSilentGroup.Fluorine.Messaging.Endpoints;
using com.TheSilentGroup.Fluorine.Messaging;

namespace com.TheSilentGroup.Fluorine.Filter
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class MessageFilter : AbstractFilter
	{
		private ILog _log;
		EndpointBase _endpoint;

		public MessageFilter(EndpointBase endpoint)
		{
			try
			{
				_log = LogManager.GetLogger(typeof(MessageFilter));
			}
			catch{}
			_endpoint = endpoint;
		}

		#region IFilter Members

		public override void Invoke(AMFContext context)
		{
			MessageOutput messageOutput = context.MessageOutput;
			for(int i = 0; i < context.AMFMessage.BodyCount; i++)
			{
				AMFBody amfBody = context.AMFMessage.GetBodyAt(i);

				if( !amfBody.IsEmptyTarget )
					continue;

				object content = amfBody.Content;
				if( content is IList )
					content = (content as IList)[0];
				IMessage message = content as IMessage;

				//Check for Flex2 messages and handle
				if( message != null )
				{
					if(message.clientId == null)
						message.clientId = Guid.NewGuid().ToString("D");

					//Check if response exists.
					ResponseBody responseBody = messageOutput.GetResponse(amfBody);
					if( responseBody != null )
					{
						if( responseBody is CachedBody )
						{
							if( _log != null && _log.IsDebugEnabled )
								_log.Debug("Response to the request " + message.GetMessageSource() + " retrieved.");
						}
						continue;
					}

					try
					{
						IMessage resultMessage = _endpoint.ServiceMessage(context.ApplicationContext, message);
						responseBody = new ResponseBody(amfBody, resultMessage);
					}
                    catch (SecurityException exception)
					{
                        responseBody = new ErrorResponseBody(amfBody, message, exception);
                        (responseBody.Content as ErrorMessage).faultCode = "Client.Authentication";
                        messageOutput.AddBody(responseBody);
                        for (int j = i + 1; j < context.AMFMessage.BodyCount; j++)
                        {
                            amfBody = context.AMFMessage.GetBodyAt(j);

                            if (!amfBody.IsEmptyTarget)
                                continue;

                            content = amfBody.Content;
                            if (content is IList)
                                content = (content as IList)[0];
                            message = content as IMessage;

                            //Check for Flex2 messages and handle
                            if (message != null)
                            {
                                responseBody = new ErrorResponseBody(amfBody, message, exception);
                                (responseBody.Content as ErrorMessage).faultCode = "Client.Authentication";
                                messageOutput.AddBody(responseBody);
                            }
                        }
                        //Leave further processing
                        return;
                    }
                    catch (UnauthorizedAccessException exception)
                    {
                        responseBody = new ErrorResponseBody(amfBody, message, exception);
                        (responseBody.Content as ErrorMessage).faultCode = "Client.Authorization";
                        messageOutput.AddBody(responseBody);
                    }
					catch(Exception exception)
					{
						if( _log != null && _log.IsErrorEnabled )
							_log.Error(exception.Message, exception);
						responseBody = new ErrorResponseBody(amfBody, message, exception);
					}
					messageOutput.AddBody(responseBody);
				}
			}
		}

		#endregion
	}
}
