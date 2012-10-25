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
using System.ComponentModel;
using System.Collections;
using System.Reflection;
// Import log4net classes.
using log4net;
using log4net.Config;

using com.TheSilentGroup.Fluorine.Exceptions;

namespace com.TheSilentGroup.Fluorine
{
	/// <summary>
	/// Summary description for MethodHandler.
	/// </summary>
	internal sealed class MethodHandler
	{
		private static ILog _log;

		static MethodHandler()
		{
			try
			{
				_log = LogManager.GetLogger(typeof(MethodHandler));
			}
			catch{}
		}

		public MethodHandler()
		{
		}

		public static bool SupportsMethod(IApplicationContext applicationContext, Type type, string methodName)
		{
			MethodInfo[] methodInfos = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
			ArrayList suitableMethodInfos = new ArrayList();
			for (int i = 0; i < methodInfos.Length; i++)
			{
				MethodInfo methodInfo = methodInfos[i];
				if (methodInfo.Name == methodName)
					return true;
			}
			return false;
		}

		public static MethodInfo GetMethod(IApplicationContext applicationContext, Type type, string methodName, IList arguments)
		{
			MethodInfo[] methodInfos = type.GetMethods(BindingFlags.Public|BindingFlags.Instance|BindingFlags.Static);
			ArrayList suitableMethodInfos = new ArrayList();
			for(int i = 0; i < methodInfos.Length; i++)
			{
				MethodInfo methodInfo = methodInfos[i];
				ParameterInfo[] parameterInfos = methodInfo.GetParameters();
				if( methodInfo.Name == methodName && parameterInfos.Length == arguments.Count )
				{
					bool match = true;
					//Matching method name and parameters number
					for(int j = 0; j < parameterInfos.Length; j++)
					{
						ParameterInfo parameterInfo = parameterInfos[j];
						if( !TypeHelper.IsAssignable(applicationContext, arguments[j], parameterInfo.ParameterType ) )
						{
							match = false;
							break;
						}
					}
					if( match )
						suitableMethodInfos.Add(methodInfo);
				}
			}
			if( suitableMethodInfos.Count == 0 )
			{
				string msg = string.Format("Could not find a suitable method with name {0}", methodName);
				if( _log != null && _log.IsErrorEnabled )
				{
					_log.Error(msg);
					for(int j = 0; j < arguments.Count; j++)
					{
						object arg = arguments[j];
						string trace;
						if( arg != null )
							trace = "Parameter " + j + " received " + arg.GetType().FullName;
						else
							trace = "Parameter " + j + " received null";
						_log.Error(trace);
					}
				}
				throw new FluorineException(msg);
			}
			if( suitableMethodInfos.Count > 1 )
			{
				string msg = string.Format("Method ambiguity, could not select the method with name {0}", methodName);
				throw new FluorineException(msg);				
			}
			return suitableMethodInfos[0] as MethodInfo;
		}
	}
}
