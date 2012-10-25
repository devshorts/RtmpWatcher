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

namespace com.TheSilentGroup.Fluorine.Configuration
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	public sealed class ServiceMap
	{
		class ServiceDescriptor
		{
			string		_serviceName;
			string		_serviceLocation;
			Hashtable	_methods;
			Hashtable	_methodsNames;

			public ServiceDescriptor(string serviceName, string serviceLocation)
			{
				_serviceName = serviceName;
				_serviceLocation = serviceLocation;
				_methods = new Hashtable(3);
				_methodsNames = new Hashtable(3);
			}

			public string ServiceName
			{
				get{ return _serviceName; }
			}

			public string ServiceLocation
			{
				get{ return _serviceLocation; }
			}

			public void AddMethod(string name, string method)
			{
				_methods[name] = method;
				_methodsNames[method]=name;
			}

			public bool Contains(string name)
			{
				return _methods.Contains(name);
			}

			public string GetMethod(string name)
			{
				if( _methods.Contains(name) )
					return _methods[name] as string;
				return name;
			}

			public string GetMethodName(string method)
			{
				if( _methodsNames.Contains(method) )
					return _methodsNames[method] as string;
				return method;
			}
		}

		private Hashtable _serviceNames;
		private Hashtable _serviceLocations;

		public ServiceMap()
		{
			_serviceNames = new Hashtable(5);
			_serviceLocations = new Hashtable(5);
		}

		public void Add(string serviceName, string serviceLocation)
		{
			ServiceDescriptor serviceDescriptor = new ServiceDescriptor(serviceName, serviceLocation);
			_serviceNames[serviceName] = serviceDescriptor;
			_serviceLocations[serviceLocation] = serviceDescriptor;
		}

		public bool Contains(string serviceName)
		{
			return _serviceNames.Contains(serviceName);
		}

		public bool ContainsLocation(string serviceLocation)
		{
			return _serviceLocations.Contains(serviceLocation);
		}

		public string GetServiceLocation(string serviceName)
		{
			if( _serviceNames.Contains(serviceName) )
				return (_serviceNames[serviceName] as ServiceDescriptor).ServiceLocation;
			else
				return serviceName;
		}

		public string GetServiceName(string serviceLocation)
		{
			if( _serviceLocations.Contains(serviceLocation) )
				return (_serviceLocations[serviceLocation] as ServiceDescriptor).ServiceName;
			else
				return serviceLocation;
		}

		public void AddMethod(string serviceName, string name, string method)
		{
			ServiceDescriptor serviceDescriptor = _serviceNames[serviceName] as ServiceDescriptor;
			if( serviceDescriptor != null )
			{
				serviceDescriptor.AddMethod(name, method);
			}
		}

		public string GetMethod(string serviceName, string name)
		{
			ServiceDescriptor serviceDescriptor = _serviceNames[serviceName] as ServiceDescriptor;
			if( serviceDescriptor != null )
				return serviceDescriptor.GetMethod(name);
			return name;
		}

		public string GetMethodName(string serviceLocation, string method)
		{
			ServiceDescriptor serviceDescriptor = _serviceLocations[serviceLocation] as ServiceDescriptor;
			if( serviceDescriptor != null )
				return serviceDescriptor.GetMethodName(method);
			return method;
		}
	}
}
