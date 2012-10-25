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
using System.Reflection;

using com.TheSilentGroup.Fluorine.Activation;
using com.TheSilentGroup.Fluorine.AMF3;
using com.TheSilentGroup.Fluorine.Invocation;
using com.TheSilentGroup.Fluorine.Messaging.Services;
using com.TheSilentGroup.Fluorine.Messaging.Messages;
using com.TheSilentGroup.Fluorine.Messaging;
using com.TheSilentGroup.Fluorine.Messaging.Config;

namespace com.TheSilentGroup.Fluorine.Remoting
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class RemotingAdapter : ServiceAdapter
	{
		public RemotingAdapter()
		{
		}

		public override object Invoke(IApplicationContext applicationContext, IMessage message)
		{
			object result = null;
			RemotingMessage remotingMessage = message as RemotingMessage;
			string operation = remotingMessage.operation;
			string className = null;
			if( remotingMessage.source != null )
				className = remotingMessage.source;
			else
			{
				className = this.DestinationSettings.Properties["source"] as string;
			}
			if( className == null )
				throw new TypeInitializationException("null", null);

			IList parameterList = remotingMessage.body as IList;

			Type type = ObjectFactory.LocateInLac(applicationContext, className);

			if( type != null )
			{
				try
				{
					MethodInfo mi = MethodHandler.GetMethod(applicationContext, type, operation, parameterList);
					if( mi != null )
					{
						//Messagebroker checked xml configured security, check attributes too
						object[] roleAttributes = mi.GetCustomAttributes( typeof(RoleAttribute), true);
						if( roleAttributes != null && roleAttributes.Length == 1 )
						{
							RoleAttribute roleAttribute = roleAttributes[0] as RoleAttribute;
							string[] roles = roleAttribute.Roles.Split(',');

							bool authorized = _service.DoAuthorization(applicationContext, roles);
							if( !authorized )
							{
								string error = string.Format("The user does not have access to the {0} method.", mi.Name );
								throw new UnauthorizedAccessException(error);
							}
						}

						ParameterInfo[] parameterInfos = mi.GetParameters();
						object[] args = new object[parameterInfos.Length];
						parameterList.CopyTo(args, 0);
						TypeHelper.NarrowValues( applicationContext, args, parameterInfos);

						object obj = ObjectFactory.CreateInstance(applicationContext, className);
						//result = mi.Invoke( obj, args );
						InvocationHandler invocationHandler = new InvocationHandler(mi);
						result = invocationHandler.Invoke(applicationContext, obj, args);
					}
				}
				catch(TargetInvocationException ex)
				{
					MessageException messageException = null;
					if( ex.InnerException is MessageException )
						messageException = ex.InnerException as MessageException;//User code throws MessageException
					else
						messageException = new MessageException(ex.InnerException);
					throw messageException;
				}
				catch(Exception ex)
				{
					MessageException messageException = new MessageException(ex);
					throw messageException;
				}
			}
			else
				throw new MessageException( new TypeInitializationException(className, null) );
			return result;
		}
	}
}
