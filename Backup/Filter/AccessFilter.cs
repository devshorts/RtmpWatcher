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

using com.TheSilentGroup.Fluorine.Configuration;
using com.TheSilentGroup.Fluorine.Messaging;
using com.TheSilentGroup.Fluorine.Messaging.Endpoints;
using com.TheSilentGroup.Fluorine.Messaging.Messages;
using com.TheSilentGroup.Fluorine.Messaging.Services;
using com.TheSilentGroup.Fluorine.Gateway;
using com.TheSilentGroup.Fluorine.Activation;

namespace com.TheSilentGroup.Fluorine.Filter
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class AccessFilter : AbstractFilter
	{
		private ILog _log;

		/// <summary>
		/// Initializes a new instance of the AccessFilter class.
		/// </summary>
		public AccessFilter()
		{
			try
			{
				_log = LogManager.GetLogger(typeof(AccessFilter));
			}
			catch{}
		}

		#region IFilter Members

		public override void Invoke(AMFContext context)
		{
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
						if( message is RemotingMessage )
						{
							RemotingMessage remotingMessage = message as RemotingMessage;
							Type type = ObjectFactory.LocateInLac(context.ApplicationContext, remotingMessage.source);
							if( type != null )
							{
								if( context.ApplicationContext.RemotingServiceAttributeConstraint == RemotingServiceAttributeConstraint.Access )
								{
									object[] roleAttributes = type.GetCustomAttributes( typeof(RemotingServiceAttribute), true);
									if( roleAttributes != null && roleAttributes.Length == 1 )
									{
										continue;
									}
								}
								else
									continue;
							}
							string msg = string.Format("The requested type [{0}] is not accessible.", remotingMessage.source);
							if( _log != null && _log.IsErrorEnabled )
							{
								_log.Error(msg);
							}
                            ErrorResponseBody errorResponseBody = new ErrorResponseBody(amfBody, message, new TypeLoadException(msg));
                            context.MessageOutput.AddBody(errorResponseBody);
                        }
					}
					else
					{
						Type type = ObjectFactory.LocateInLac(context.ApplicationContext, amfBody.TypeName);
						if( type != null )
						{
							if( context.ApplicationContext.RemotingServiceAttributeConstraint == RemotingServiceAttributeConstraint.Access )
							{
								object[] roleAttributes = type.GetCustomAttributes( typeof(RemotingServiceAttribute), true);
								if( roleAttributes != null && roleAttributes.Length == 1 )
								{
									continue;
								}
							}
							else
								continue;
						}
						else
						{
							if( amfBody.IsAspxPage(context.ApplicationContext) )
							{
								//Only allow aspx page in compatibility mode
								if( context.ApplicationContext.RemotingServiceAttributeConstraint == RemotingServiceAttributeConstraint.Browse )
									continue;
							}
						}


                        string msg = string.Format("The requested type [{0}] is not accessible.", amfBody.TypeName);
                        if (_log != null && _log.IsErrorEnabled)
						{
							_log.Error(msg);
						}
                        ErrorResponseBody errorResponseBody = new ErrorResponseBody(amfBody, new TypeLoadException(msg));
                        context.MessageOutput.AddBody(errorResponseBody);
                    }
				}
		}

		#endregion
	}
}
