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
using System.Text;
using System.Collections;
using System.Security;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using System.Web.Configuration;
using System.Web.Caching;
using System.Threading;

using com.TheSilentGroup.Fluorine.Messaging.Config;
using com.TheSilentGroup.Fluorine.Messaging;
using com.TheSilentGroup.Fluorine.Messaging.Messages;
using com.TheSilentGroup.Fluorine.Messaging.Security;

namespace com.TheSilentGroup.Fluorine.Messaging.Services
{
	/// <summary>
	/// Summary description for AuthenticationService.
	/// </summary>
	internal class AuthenticationService : ServiceBase
	{
		public const string ServiceId = "authentication-service";
		public static string FluorineTicket = "fluorineauthticket";

		public AuthenticationService(IApplicationContext applicationContext, MessageBroker broker, ServiceSettings settings) : base(applicationContext, broker, settings)
		{
		}

		public override object ServiceMessage(IApplicationContext applicationContext, IMessage message)
		{
			IMessage responseMessage = null;
			CommandMessage commandMessage = message as CommandMessage;
			if( commandMessage != null )
			{
				switch(commandMessage.operation)
				{
					case CommandMessage.LoginOperation:
						IPrincipal principal = Authenticate(applicationContext, message);
                        if (principal == null)
                        {
                            throw new SecurityException("Authenticate method must return a valid Principal object.");
                        }
						responseMessage = new AcknowledgeMessage();
						responseMessage.body = "success";
						break;
					case CommandMessage.LogoutOperation:
						//TODO: Logs the user out of the destination. Logging out of a destination applies to everything connected using the same ChannelSet as specified in the server configuration. For example, if you're connected over the my-rtmp channel and you log out using one of your RPC components, anything that was connected over the same ChannelSet is logged out.
						bool logout = _messageBroker.LoginCommand.Logout(applicationContext.User);
						responseMessage = new AcknowledgeMessage();
						responseMessage.body = "success";
						break;
				}
			}
			return responseMessage;
		}

		public override void CheckSecurity(IApplicationContext applicationContext, IMessage message)
		{
			//Ignore for this service
		}


		private IPrincipal Authenticate(IApplicationContext applicationContext, IMessage message)
		{
			IPrincipal principal = null;
			//if( message.headers != null && message.headers.ContainsKey(CommandMessage.RemoteCredentialsHeader) )
			if( message.body is string )
			{
				//string base64String = message.headers[CommandMessage.RemoteCredentialsHeader] as string;
				string base64String = message.body as string;
				byte[] base64Data = Convert.FromBase64String(base64String); 
				StringBuilder sb = new StringBuilder();
				sb.Append(UTF8Encoding.UTF8.GetChars(base64Data));
				string data = sb.ToString();
				string[] parts = data.Split(new char[]{':'});
				string user = parts[0];
				string password = parts[1];
				//TODO
				if( _messageBroker.LoginCommand != null )
				{
					Hashtable credentials = new Hashtable(1);
					credentials["password"] = password;
					principal = _messageBroker.LoginCommand.DoAuthentication(user, credentials);
					if( principal != null )
					{
						StorePrincipal(applicationContext, principal, user, password);
					}
				}
			}
			return principal;
		}

		public static void StorePrincipal(IApplicationContext applicationContext, IPrincipal principal, string userId, string password)
		{
			string uniqueKey = Guid.NewGuid().ToString("N");
			// Get the cookie created by the FormsAuthentication API
			// Notice that this cookie will have all the attributes according to   
			// the ones in the config file setting.
			// This does not set the cookie as part of the outgoing response.
			HttpCookie cookie = FormsAuthentication.GetAuthCookie(userId, false );
			FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);

			string cacheKey = string.Join("|", new string[] {AuthenticationService.FluorineTicket, uniqueKey, userId, password});
			// Store the Guid inside the Forms Ticket with all the attributes aligned with 
			// the config Forms section.
			FormsAuthenticationTicket newTicket = new FormsAuthenticationTicket(
				ticket.Version,
				ticket.Name,
				ticket.IssueDate,
				ticket.Expiration,
				ticket.IsPersistent,
				cacheKey,
				ticket.CookiePath);
			// Add the encrypted ticket to the cookie as data.
			cookie.Value = FormsAuthentication.Encrypt(newTicket);
			// Update the outgoing cookies collection.
			applicationContext.AddCookie(cookie);
			// Add the principal to the Cache with the expiration item sync with the FormsAuthentication ticket timeout
			HttpContext.Current.Cache.Insert( cacheKey, principal, null, 
				Cache.NoAbsoluteExpiration,
				newTicket.Expiration.Subtract( newTicket.IssueDate ), 
				CacheItemPriority.Default, null );
		}

		public static string GetFormsAuthCookieName()
		{
			string formsCookieName = Environment.UserInteractive ? ".ASPXAUTH" : FormsAuthentication.FormsCookieName;
			return formsCookieName;
		}
	}
}
