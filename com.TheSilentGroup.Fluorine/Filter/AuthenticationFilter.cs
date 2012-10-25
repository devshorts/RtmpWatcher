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
using System.Web;
using System.Reflection;
using System.Collections;
using System.Security.Principal;
using System.Web.Security;
using System.Threading;
// Import log4net classes.
using log4net;
using log4net.Config;

using com.TheSilentGroup.Fluorine.Gateway;
using com.TheSilentGroup.Fluorine.Activation;
using com.TheSilentGroup.Fluorine.Messaging;
using com.TheSilentGroup.Fluorine.Messaging.Messages;
using com.TheSilentGroup.Fluorine.Messaging.Endpoints;
using com.TheSilentGroup.Fluorine.Messaging.Security;
using com.TheSilentGroup.Fluorine.Messaging.Services;
using com.TheSilentGroup.Fluorine.Messaging.Config;

namespace com.TheSilentGroup.Fluorine.Filter
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class AuthenticationFilter : AbstractFilter
	{
		private ILog _log;
		EndpointBase _endpoint;

		/// <summary>
		/// Initializes a new instance of the AuthenticationFilter class.
		/// </summary>
		/// <param name="endpoint"></param>
		public AuthenticationFilter(EndpointBase endpoint)
		{
			try
			{
				_log = LogManager.GetLogger(typeof(AuthenticationFilter));
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

				//Check for Flex2 messages
				if( amfBody.IsEmptyTarget )
				{
					object content = amfBody.Content;
					if( content is IList )
						content = (content as IList)[0];
					IMessage message = content as IMessage;
					if(message.clientId == null)
						message.clientId = Guid.NewGuid().ToString("D");

					if( message is CommandMessage && (message as CommandMessage).operation == CommandMessage.LoginOperation )
					{
						//If the first message is a LoginOperation then skip authorization here
						//If authentication will fail then MessageFilter will cancel the messages
						return;
					}

					try
					{
						AuthenticateFlexClient(context, amfBody);
					}
					catch(UnauthorizedAccessException ex)
					{
						ErrorResponseBody errorResponseBody = new ErrorResponseBody(amfBody, message, ex);
						(errorResponseBody.Content as ErrorMessage).faultCode = "Client.Authorization";
						messageOutput.AddBody(errorResponseBody);
					}
					catch(Exception ex)
					{
						if( _log != null && _log.IsErrorEnabled )
							_log.Error(ex.Message, ex);
						ErrorResponseBody errorResponseBody = new ErrorResponseBody(amfBody, message, ex);
						messageOutput.AddBody(errorResponseBody);
					}
				}
				else
				{
					try
					{
						AuthenticateFlashClient(context, amfBody);
					}
					catch(UnauthorizedAccessException ex)
					{
						ErrorResponseBody errorResponseBody = new ErrorResponseBody(amfBody, ex);
						messageOutput.AddBody(errorResponseBody);
					}
					catch(Exception ex)
					{
						if( _log != null && _log.IsErrorEnabled )
							_log.Error(ex.Message, ex);
						ErrorResponseBody errorResponseBody = new ErrorResponseBody(amfBody, ex);
						messageOutput.AddBody(errorResponseBody);
					}
				}
			}
		}

		#endregion

		void AuthenticateFlashClient(AMFContext context, AMFBody body)
		{
			IApplicationContext applicationContext = context.ApplicationContext;
			MessageBroker messageBroker = _endpoint.GetMessageBroker();
			IPrincipal principal = messageBroker.RestorePrincipal(applicationContext);

			if( principal == null )
			{
				//Check for Credentials Header
				AMFMessage amfMessage = context.AMFMessage;
				AMFHeader amfHeader = amfMessage.GetHeader( AMFHeader.CredentialsHeader );
				if( amfHeader != null && amfHeader.Content != null )
				{
					string userId = ((ASObject)amfHeader.Content)["userid"] as string;
					string password = ((ASObject)amfHeader.Content)["password"] as string;
					//Clear credentials header
					ASObject asoObject = new ASObject();
					asoObject["name"] = AMFHeader.CredentialsHeader;
					asoObject["mustUnderstand"] = false;
					asoObject["data"] = null;//clear
					AMFHeader header = new AMFHeader("RequestPersistentHeader", true, asoObject);
					context.MessageOutput.AddHeader( header );

					ILoginCommand loginCommand = _endpoint.GetMessageBroker().LoginCommand;
					if( loginCommand != null )
					{
						Hashtable credentials = new Hashtable(1);
						credentials["password"] = password;
						principal = loginCommand.DoAuthentication(userId, credentials);
						if( principal == null )
							throw new UnauthorizedAccessException("DoAuthentication method must return a valid Principal object." );
						AuthenticationService.StorePrincipal(applicationContext, principal, userId, password);
					
						// Attach the new principal object to the current HttpContext object
						context.ApplicationContext.User = principal;
						Thread.CurrentPrincipal = principal;
					}
					else
						throw new UnauthorizedAccessException("ILoginCommand was not found." );
				}
			}

			// Check if we can find a destination for the amf target.
			// If the destination is missing create it and configure a declarative security.
			RemotingService remotingService = messageBroker.GetService(RemotingService.RemotingServiceId) as RemotingService;
			Destination destination = remotingService.GetDestinationWithSource(body.TypeName);
			if( destination == null )
				destination = remotingService.DefaultDestination;
			if( destination == null )
				destination = remotingService.GetDestination(body.TypeName);

			if( destination == null )
			{
				Type type = ObjectFactory.Locate(applicationContext, body.TypeName);
				if( type != null )
				{
					MethodInfo mi = MethodHandler.GetMethod(applicationContext, type, body.Method, body.GetParameterList());
					if( mi != null )
					{
						DestinationSettings destinationSettings = remotingService.ServiceSettings.CreateDestinationSettings(body.TypeName, body.TypeName);
						destination = remotingService.CreateDestination(applicationContext, destinationSettings);

						object[] roleAttributes = mi.GetCustomAttributes( typeof(RoleAttribute), true);
						if( roleAttributes != null && roleAttributes.Length == 1 )
						{
							RoleAttribute roleAttribute = roleAttributes[0] as RoleAttribute;
							string[] roles = roleAttribute.Roles.Split(',');
							destinationSettings.SecuritySettings.CreateSecurityConstraint(Guid.NewGuid().ToString("N"), "Custom", roles);
						}
					}
					else
					{
						if(_log != null && _log.IsErrorEnabled)
							_log.Error("Could not locate service: " + body.Target);
						throw new MissingMethodException(body.TypeName, body.Method);
					}
				}
				else
				{
					if(_log != null && _log.IsErrorEnabled)
						_log.Error("Could not locate type: " + body.TypeName);
					throw new TypeLoadException("Could not locate type: " + body.TypeName);
				}
			}
			remotingService.CheckSecurity(applicationContext, destination);
		}

		void AuthenticateFlexClient(AMFContext context, AMFBody body)
		{
			IApplicationContext applicationContext = context.ApplicationContext;
			MessageBroker messageBroker = _endpoint.GetMessageBroker();
			IPrincipal principal = messageBroker.RestorePrincipal(applicationContext);

			object content = body.Content;
			if( content is IList )
				content = (content as IList)[0];
			IMessage message = content as IMessage;
			//Check for Flex2 messages and handle
			if( message != null )
			{
				IService service = messageBroker.GetService(applicationContext, message);
				if( service != null )
				{
					service.CheckSecurity(applicationContext, message);
				}
			}
			else
			{
				if( _log != null && _log.IsFatalEnabled )
					_log.Fatal("Flex message expected. Invalid AMF body.");
			}			
		}
	}
}
