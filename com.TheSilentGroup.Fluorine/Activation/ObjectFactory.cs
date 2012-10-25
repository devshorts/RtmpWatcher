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
using System.Collections.Specialized;
using System.Configuration;
using com.TheSilentGroup.Fluorine.Gateway;
using com.TheSilentGroup.Fluorine.Exceptions;

// Import log4net classes.
using log4net;
using log4net.Config;

namespace com.TheSilentGroup.Fluorine.Activation
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	internal class ObjectFactory
	{
		private static ILog _log;

		private static Hashtable _typeCache = new Hashtable();
		private static Hashtable _activationModeCache = new Hashtable();
		private static Hashtable _activatorsCache = new Hashtable();

		static ObjectFactory()
		{
			try
			{
				_log = LogManager.GetLogger(typeof(ObjectFactory));
			}
			catch{}

			_activatorsCache.Add("application", new ApplicationActivator());
			_activatorsCache.Add("request", new RequestActivator());
			_activatorsCache.Add("session", new SessionActivator());

			try
			{
				NameValueCollection activators = (NameValueCollection)ConfigurationManager.GetSection("fluorine/activators");
				if( activators != null )
				{
					foreach(string name in activators)
					{
						string typeName = activators[name];
						Type type = Locate(null, typeName);
						if( type != null && !_activatorsCache.Contains(name))
							_activatorsCache[name] = Activator.CreateInstance(type);
					}
				}
			}
			catch(Exception ex)
			{
				throw ex;
			}
		}

		static internal void AddActivator(IApplicationContext applicationContext, string activationMode, string typeName)
		{
			Type type = Locate(applicationContext, typeName);
			if( type != null )
			{
				_activationModeCache[type] = activationMode;
			}
			else
			{
				if( _log != null && _log.IsErrorEnabled )
				{
					string msg = string.Format("The requested type [{0}] was not found.", typeName);
					_log.Error(msg);
				}
			}
		}

		static public Type Locate(IApplicationContext applicationContext, string typeName)
		{
			if( typeName == null || typeName == string.Empty )
				return null;

			string mappedTypeName = typeName;
			if( applicationContext != null )
				mappedTypeName = applicationContext.GetMappedTypeName(typeName);

			//Lookup first in our cache.
			lock(typeof(Type))
			{
				Type type = _typeCache[mappedTypeName] as Type;
				if( type == null )
				{

					type = com.TheSilentGroup.Fluorine.TypeHelper.Locate(mappedTypeName);
					if(type != null)
					{
						_typeCache[mappedTypeName] = type;
						return type;
					}
					else
					{
						//Locate in LAC
						if( applicationContext != null )
							type = com.TheSilentGroup.Fluorine.TypeHelper.Locate(applicationContext, mappedTypeName, applicationContext.ApplicationPath);
						if(type != null)
						{
							_typeCache[mappedTypeName] = type;
							return type;
						}
					}
				}
				return type;
			}
		}

		static public Type LocateInLac(IApplicationContext applicationContext, string typeName)
		{
			//Locate in LAC
			if( typeName == null || typeName == string.Empty )
				return null;

			string mappedTypeName = typeName;
			mappedTypeName = applicationContext.GetMappedTypeName(typeName);

			//Lookup first in our cache.
			lock(typeof(Type))
			{
				Type type = _typeCache[mappedTypeName] as Type;
				if( type == null )
				{
					//Locate in LAC
					if( applicationContext != null )
					{
						type = com.TheSilentGroup.Fluorine.TypeHelper.Locate(applicationContext, mappedTypeName, applicationContext.ApplicationPath);
						if(type != null)
						{
							_typeCache[mappedTypeName] = type;
							return type;
						}
						else
						{
							type = com.TheSilentGroup.Fluorine.TypeHelper.Locate(applicationContext, mappedTypeName, applicationContext.DynamicDirectory);
							if(type != null)
							{
								_typeCache[mappedTypeName] = type;
								return type;
							}
						}
					}
				}
				return type;
			}
		}

		static public void AddTypeToCache(Type type)
		{
			if( type != null )
			{
				lock(typeof(Type))
				{
					_typeCache[type.FullName] = type;
				}
			}
		}

		static public bool ContainsType(string typeName)
		{
			if( typeName != null )
			{
				lock(typeof(Type))
				{
					return _typeCache.Contains(typeName);
				}
			}
			return false;
		}

		static public object CreateInstance(IApplicationContext applicationContext, Type type)
		{
			return CreateInstance(applicationContext, type, null);
		}

		static public object CreateInstance(IApplicationContext applicationContext, Type type, object[] args)
		{
			if( type != null )
			{
				lock(typeof(Type))
				{
					string activationMode = _activationModeCache[type] as string;
					if( activationMode == null )
					{
						ActivationAttribute activationAttribute = null;
						foreach (Attribute attribute in type.GetCustomAttributes( typeof(ActivationAttribute), true)) 
						{
							activationAttribute = attribute as ActivationAttribute;
							break;
						}
						if( activationAttribute != null )
						{
							activationMode = activationAttribute.ActivationMode;
							_activationModeCache[type] = activationMode;
						}
					}
					if( activationMode == null )
					{
						activationMode = "request";
						_activationModeCache[type] = activationMode;
					}

					if( applicationContext != null && applicationContext.ActivationMode != null )
						activationMode = applicationContext.ActivationMode;

					IActivator activator = _activatorsCache[activationMode] as IActivator;

					if( activator != null )
						return activator.Activate(applicationContext, type, args);
					else
					{
						string msg = string.Format("The requested activator [{0}] was not found.", activationMode);
						if( _log != null )
							_log.Error(msg);
						throw new FluorineException(msg);
					}
				}
			}
			return null;
		}

		static public object CreateInstance(IApplicationContext applicationContext, string typeName)
		{
			return CreateInstance(applicationContext, typeName, null);
		}

		static public object CreateInstance(IApplicationContext applicationContext, string typeName, object[] args)
		{
			Type type = Locate(applicationContext, typeName);
			return CreateInstance(applicationContext, type, args);
		}
	}
}
