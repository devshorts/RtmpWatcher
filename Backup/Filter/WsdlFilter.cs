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
using System.Xml;
using System.Reflection;
// Import log4net classes.
using log4net;
using log4net.Config;
using com.TheSilentGroup.Fluorine.Gateway;
using com.TheSilentGroup.Fluorine.Util;
using com.TheSilentGroup.Fluorine.Activation;

namespace com.TheSilentGroup.Fluorine.Filter
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class WsdlFilter : AbstractFilter
	{
		private ILog _log;
		Hashtable	_webserviceTypeCache;
		object _objLock = new object();

		/// <summary>
		/// Initializes a new instance of the WsdlFilter class.
		/// </summary>
		public WsdlFilter()
		{
			try
			{
				_log = LogManager.GetLogger(typeof(WsdlFilter));
			}
			catch{}
			_webserviceTypeCache = new Hashtable();
		}

		#region IFilter Members

		public override void Invoke(AMFContext context)
		{
			for(int i = 0; i < context.AMFMessage.BodyCount; i++)
			{
				AMFBody amfBody = context.AMFMessage.GetBodyAt(i);
				if( amfBody.IsWebService )
				{
					Type type = null;
					lock(_objLock)
					{
						if( _webserviceTypeCache.ContainsKey(amfBody.TypeName) )
						{
							type = _webserviceTypeCache[amfBody.TypeName] as Type;
							//We can handle it with a LibraryAdapter now
							amfBody.Target = type.FullName + "." + amfBody.Method;
							continue;
						}
						try
						{
							if( _log != null && _log.IsInfoEnabled )
								_log.Info("Generate assembly from wsdl " + amfBody.Target);
							Assembly assembly = WsdlHelper.GetAssemblyFromWsdl(context.ApplicationContext, WsdlHelper.GetWsdl(context.ApplicationContext, amfBody.TypeName));
							if( assembly != null )
							{
								Type[] types = assembly.GetTypes();
								if( types.Length > 0 )
								{
									type = types[0];
									_webserviceTypeCache[amfBody.TypeName] = type;//cache
									ObjectFactory.AddTypeToCache(type);
									//We can handle it with a LibraryAdapter now
									amfBody.Target = type.FullName + "." + amfBody.Method;
								}
								else
								{
									Exception exception = new TypeInitializationException(amfBody.TypeName, null);
									if( _log != null && _log.IsErrorEnabled )
										_log.Error("Generated assembly from wsdl " + amfBody.Target + ". Failed to locate type.", exception );

									ErrorResponseBody errorResponseBody = new ErrorResponseBody(amfBody, exception);
									context.MessageOutput.AddBody(errorResponseBody);
								}
							}
							if( type == null )
							{
								Exception exception = new TypeInitializationException(amfBody.TypeName, null);
								if( _log != null && _log.IsErrorEnabled )
									_log.Error("Generated assembly from wsdl " + amfBody.Target + ". Failed to locate type.", exception );

								ErrorResponseBody errorResponseBody = new ErrorResponseBody(amfBody, exception);
								context.MessageOutput.AddBody(errorResponseBody);
							}
						}
						catch(Exception exception)
						{
							if( _log != null && _log.IsErrorEnabled )
								_log.Error("Failed generating assembly from wsdl " + amfBody.Target, exception);
							ErrorResponseBody errorResponseBody = new ErrorResponseBody(amfBody, exception);
							context.MessageOutput.AddBody(errorResponseBody);
						}
						continue;
					}
				}
			}
		}
		
		#endregion

	}
}
