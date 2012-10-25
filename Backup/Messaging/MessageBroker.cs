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
using System.Text;
using System.Security;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using System.Web.Configuration;
using System.Web.Caching;
using System.Threading;

using com.TheSilentGroup.Fluorine.Messaging.Config;
using com.TheSilentGroup.Fluorine.Messaging.Services;
using com.TheSilentGroup.Fluorine.Messaging.Endpoints;
using com.TheSilentGroup.Fluorine.Messaging.Messages;
using com.TheSilentGroup.Fluorine.Messaging.Security;

namespace com.TheSilentGroup.Fluorine.Messaging
{
	/// <summary>
	/// All communication with the various services provided is mediated by the message broker.
	/// <br/><br/>
	/// It has a number of endpoints which send and receive messages over the network, and it has 
	/// a number of services that are message destinations. The broker routes messages to 
	/// endpoints based on the content type of those messages, and routes decoded messages 
	/// to services based on message type.
	/// The broker also has a means of calling back into the endpoints in order to push messages 
	/// back through them. 
	/// </summary>
	/// <example>
	/// A RemoteObject message arrives over the RTMP endpoint. The endpoint decodes 
	/// the message and sends it to the Message Broker, and the broker then passes it on to 
	/// the RemotingService which will perform the RemoteObject invocation. 
	/// </example>
	public class MessageBroker
	{
		Hashtable			_services;
		Hashtable			_endpoints;
		SecuritySettings	_securitySettings;
		ILoginCommand		_loginCommand;
		public static string FluorineTicket = "fluorineauthticket";


		/// <summary>
		/// Initializes a new instance of the MessageBroker class.
		/// </summary>
		public MessageBroker()
		{
			_services = new Hashtable();
			_endpoints = new Hashtable();
		}

		internal SecuritySettings SecuritySettings
		{
			get{ return _securitySettings; }
			set{ _securitySettings = value; }
		}

		internal ILoginCommand LoginCommand
		{
			get{ return _loginCommand; }
			set{ _loginCommand = value; }
		}

		internal void AddService(IService service)
		{
			_services[service.id] = service;
		}

		internal IService GetService(string id)
		{
			return _services[id] as IService;
		}

		internal void AddEndpoint(IEndpoint endpoint)
		{
			_endpoints[endpoint.Id] = endpoint;
		}
		/// <summary>
		/// Start all of the broker's services.
		/// </summary>
		internal void StartServices()
		{
			foreach(DictionaryEntry entry in _services)
			{
				IService service = entry.Value as IService;
				service.Start();
			}
		}
		/// <summary>
		/// Stop all of the broker's services.
		/// </summary>
		internal void StopServices()
		{
			foreach(DictionaryEntry entry in _services)
			{
				IService service = entry.Value as IService;
				service.Stop();
			}
		}
		/// <summary>
		/// Start all of the broker's endpoints.
		/// </summary>
		internal void StartEndpoints()
		{
			foreach(DictionaryEntry entry in _endpoints)
			{
				IEndpoint endpoint = entry.Value as IEndpoint;
				endpoint.Start();
			}
		}
		/// <summary>
		/// Stop all of the broker's endpoints.
		/// </summary>
		internal void StopEndpoints()
		{
			foreach(DictionaryEntry entry in _endpoints)
			{
				IEndpoint endpoint = entry.Value as IEndpoint;
				endpoint.Stop();
			}
		}
		/// <summary>
		/// Start the message broker.
		/// </summary>
		public void Start()
		{
			StartServices();
			StartEndpoints();
		}
		/// <summary>
		/// Stop the message broker.
		/// </summary>
		public void Stop()
		{
			StopServices();
			StopEndpoints();
		}

		internal IEndpoint GetEndpoint(IApplicationContext applicationContext, string path, string contextPath, bool secure)
		{
			foreach(DictionaryEntry entry in _endpoints)
			{
				IEndpoint endpoint = entry.Value as IEndpoint;
				ChannelSettings channelSettings = endpoint.GetSettings();
				if( channelSettings != null && channelSettings.Bind(applicationContext, path, contextPath ) )
					return endpoint;
			}
			return null;
		}

		/// <summary>
		/// Call this method in order to send a message from your code into the message routing system.
		/// The message is routed to a service that is defined to handle messages of this type.
		/// Once the service is identified, the destination property of the message is used to find a destination configured for that service.
		/// The adapter defined for that destination is used to handle the message.
		/// </summary>
		/// <param name="applicationContext"></param>
		/// <param name="message">The message to be routed to a service.</param>
		/// <param name="endpoint">This can identify the endpoint that is sending the message but it is currently not used so you may pass in null.</param>
		/// <returns></returns>
		internal IMessage RouteMessage(IApplicationContext applicationContext, IMessage message, IEndpoint endpoint)
		{
			IService service = null;
			object result = null;
			IMessage responseMessage = null;


            CommandMessage commandMessage = message as CommandMessage;
            if (commandMessage != null && (commandMessage.operation == CommandMessage.LoginOperation || commandMessage.operation == CommandMessage.LogoutOperation))//Login, Logout
            {
                service = GetService(AuthenticationService.ServiceId);
                result = service.ServiceMessage(applicationContext, commandMessage);
                responseMessage = result as IMessage;
            }
            else if (commandMessage != null && commandMessage.messageRefType == null)//Ping
            {
                responseMessage = new AcknowledgeMessage();
            }
            else
            {
                //Moved to AuthenticationFilter
                //RestorePrincipal(applicationContext);
                service = GetService(applicationContext, message);
                if (service != null)
                {
                    //Moved to AuthenticationFilter
                    //service.CheckSecurity(applicationContext, message);
                    result = service.ServiceMessage(applicationContext, message);
                }
                if (!(result is IMessage))
                {
                    responseMessage = new AcknowledgeMessage();
                    responseMessage.body = result;
                }
                else
                    responseMessage = result as IMessage;
            }

			if( responseMessage is AsyncMessage )
			{
				((AsyncMessage)responseMessage).correlationId = message.messageId;
			}
			responseMessage.destination = message.destination;
			responseMessage.clientId = message.clientId;
			return responseMessage;
		}

		internal IService GetService(IApplicationContext applicationContext, IMessage message)
		{
			IService service = null;
			CommandMessage commandMessage = message as CommandMessage;
			if( commandMessage != null && commandMessage.messageRefType == null )//Ping
				return null;
			else
			{
				foreach(DictionaryEntry entry in _services)
				{
					IService serviceTmp = entry.Value as IService;
					if( commandMessage != null )
					{
						if( serviceTmp.IsSupportedMessageType(applicationContext, commandMessage.messageRefType ) )
						{
							service = serviceTmp;
							break;
						}
					}
					else
					{
						if( serviceTmp.IsSupportedMessage(applicationContext, message ) )
						{
							service = serviceTmp;
							break;
						}
					}
				}
			}
			return service;
		}

		internal IPrincipal RestorePrincipal(IApplicationContext applicationContext)
		{
			IPrincipal principal = null;

			//User already authenticated
			if(HttpContext.Current != null && HttpContext.Current.Request.IsAuthenticated)
			{
				
				if (HttpContext.Current.User.Identity is FormsIdentity)
				{
					FormsIdentity formsIdentity = HttpContext.Current.User.Identity as FormsIdentity;
					if( formsIdentity.Ticket.UserData == null || !formsIdentity.Ticket.UserData.StartsWith(MessageBroker.FluorineTicket) )
						return HttpContext.Current.User;
					//Let fluorine get the correct principal
				}
				else
					return HttpContext.Current.User;
			}

			if( applicationContext != null && applicationContext.GetCookieValue(AuthenticationService.GetFormsAuthCookieName()) != null )
			{
				FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt( applicationContext.GetCookieValue( AuthenticationService.GetFormsAuthCookieName() ) );
				if( ticket != null )
				{
					principal = HttpContext.Current.Cache[ticket.UserData] as IPrincipal;
					if( principal == null )
					{
						if( ticket.UserData != null && ticket.UserData.StartsWith(MessageBroker.FluorineTicket) )
						{
							//Get the principal as the cache lost the data
							string[] userData = ticket.UserData.Split(new char[] {'|'});
							string userId = userData[2];
							string password = userData[3];
							ILoginCommand loginCommand = _loginCommand;
							if( loginCommand != null )
							{
								Hashtable credentials = new Hashtable(1);
								credentials["password"] = password;
								principal = loginCommand.DoAuthentication(userId, credentials);
								if( principal == null )
									throw new UnauthorizedAccessException("DoAuthentication method must return a valid Principal object." );
								AuthenticationService.StorePrincipal(applicationContext, principal, userId, password);
							}
							else
								throw new UnauthorizedAccessException("ILoginCommand was not found." );
						}
					}
				}
				else
				{
					//This is not our cookie so rely on application's authentication
					principal = applicationContext.User;
				}
			}
			if( principal != null )
			{
				applicationContext.User = principal;
				Thread.CurrentPrincipal = principal;
			}
			return principal;
		}
	}
}
