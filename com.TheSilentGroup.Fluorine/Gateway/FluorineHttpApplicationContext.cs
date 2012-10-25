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
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Security.Principal;

using com.TheSilentGroup.Fluorine.Configuration;
using com.TheSilentGroup.Fluorine.Diagnostic;
using com.TheSilentGroup.Fluorine.Messaging;

namespace com.TheSilentGroup.Fluorine.Gateway
{
	/// <summary>
	/// Summary description for FluorineHttpApplicationContext.
	/// </summary>
	public class FluorineHttpApplicationContext : IApplicationContext
	{
		/// <summary>
		/// Used to implement IDisposable.
		/// </summary>
		bool _isDisposed;

		HttpApplication			_httpApplication;
		HttpSessionStateWrapper	_httpSessionStateWrapper;
		ArrayList				_failedAMFBodies;
		MessageServer			_messageServer;

		/// <summary>
		/// Initializes a new instance of the FilterChain class.
		/// </summary>
		/// <param name="httpApplication"></param>
		/// <param name="messageServer"></param>
		public FluorineHttpApplicationContext(HttpApplication httpApplication, MessageServer messageServer)
		{
			_httpApplication = httpApplication;
			_messageServer = messageServer;
			try
			{
				//_httpApplication.Response.AddHeader("Content-Type", "application/x-amf");
				HttpContext.Current.Response.AddHeader("Content-Type", "application/x-amf");
			}
			catch(HttpException)
			{
			}

			_failedAMFBodies = new ArrayList(1);
		}

		public void Init(HttpApplication httpApplication)
		{
			ClassMappings classMappings = (ClassMappings)ConfigurationManager.GetSection("fluorine/classMappings");
			if( classMappings == null )
				classMappings = new ClassMappings();
			httpApplication.Context.Application["ClassMappings"] = classMappings;

			ServiceMap serviceMap = (ServiceMap)ConfigurationManager.GetSection("fluorine/services");
			httpApplication.Context.Application["ServiceMap"] = serviceMap;

			CacheMap cacheMap = (CacheMap)ConfigurationManager.GetSection("fluorine/cache");
			httpApplication.Context.Application["CacheMap"] = cacheMap;

			NameValueCollection importNamespaces = (NameValueCollection)ConfigurationManager.GetSection("fluorine/importNamespaces");
			httpApplication.Context.Application["ImportNamespaces"] = importNamespaces;

			Hashtable nullables = ConfigurationManager.GetSection("fluorine/nullable") as Hashtable;
			httpApplication.Context.Application["NullableValues"] = nullables;

			string tmp = ConfigurationManager.AppSettings["wsdlGenerateProxyClasses"];
			bool wsdlGenerateProxyClasses = true;
			if( tmp != null )
				wsdlGenerateProxyClasses = Convert.ToBoolean(tmp);
			httpApplication.Context.Application["WsdlGenerateProxyClasses"] = wsdlGenerateProxyClasses;

			tmp = ConfigurationManager.AppSettings["wsdlProxyNamespace"];
			string wsdlProxyNamespace = "com.TheSilentGroup.Fluorine.Proxy";
			if( tmp != null )
				wsdlProxyNamespace = tmp;
			httpApplication.Context.Application["WsdlProxyNamespace"] = wsdlProxyNamespace;

			tmp = ConfigurationManager.AppSettings["acceptNullValueTypes"];
			bool acceptNullValueTypes = false;
			if( tmp != null )
				acceptNullValueTypes = Convert.ToBoolean(tmp);
			httpApplication.Context.Application["AcceptNullValueTypes"] = acceptNullValueTypes;

			RemotingServiceAttributeConstraint remotingServiceAttributeConstraint = RemotingServiceAttributeConstraint.Browse;
			tmp = ConfigurationManager.AppSettings["remotingServiceAttribute"];
			if( tmp != null )
			{
				try 
				{
					remotingServiceAttributeConstraint = (RemotingServiceAttributeConstraint)Enum.Parse(typeof(RemotingServiceAttributeConstraint), tmp, true);
				}
				catch(ArgumentException) { }
			}
			httpApplication.Context.Application["RemotingServiceAttributeConstraint"] = remotingServiceAttributeConstraint;
		}

		#region IDisposable Members

		/// <summary>
		/// Implements the IDispose' method Dispose.
		/// Use the Dispose method of this interface to explicitly release unmanaged resources in conjunction with 
		/// the garbage collector. The consumer of an object can call this method when the object is no longer needed.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			// Once the Dispose method has been called, it is typically unnecessary for 
			// the garbage collector to call the disposed object's finalizer. 
			// To prevent automatic finalization, Dispose implementations can call the GC.SuppressFinalize method.
			System.GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Implements the Dispose functionality.
		/// </summary>
		/// <param name="isDisposing"></param>
		protected virtual void Dispose(bool isDisposing)
		{
			// Check to see if Dispose has already been called.
			if(!_isDisposed)
			{
				if(isDisposing)
				{
					_httpApplication = null;
					_httpSessionStateWrapper = null;
					_isDisposed = true;
				}
			}
		}

		#endregion

		#region IApplicationContext Members

		public void AddHeader(string name, string value)
		{
			_httpApplication.Response.AddHeader(name, value);
		}

		public string ApplicationPath
		{
			get
			{
				Uri uri = new Uri( Path.GetDirectoryName( System.Reflection.Assembly.GetExecutingAssembly().CodeBase ) );
				string path = uri.LocalPath;
				return path;
			}
		}

		public string RootPath
		{ 
			get
			{
				//return _httpApplication.Request.PhysicalApplicationPath;
				return HttpRuntime.AppDomainAppPath;
			}
		}

		public string RequestPath 
		{ 
			get { return _httpApplication.Context.Request.Path; }
		}

		public string RequestApplicationPath
		{ 
			get { return _httpApplication.Context.Request.ApplicationPath; }
		}

		public bool IsSecureConnection 
		{
			get{ return _httpApplication.Context.Request.IsSecureConnection; }
		}

		public System.IO.Stream InputStream
		{
			get
			{
				return _httpApplication.Request.InputStream;
			}
		}

		public System.IO.Stream OutputStream
		{
			get
			{
				return _httpApplication.Response.OutputStream;
			}
		}

		public ISessionState Session
		{ 
			get
			{
				if( _httpSessionStateWrapper == null )
					_httpSessionStateWrapper = new HttpSessionStateWrapper(_httpApplication.Context.Session);
				return _httpSessionStateWrapper;
			}
		}

		public string MapPath(string path)
		{
			return _httpApplication.Server.MapPath(path);
		}

		public void Execute(string path, TextWriter writer)
		{
			_httpApplication.Server.Execute( path, writer);
		}

		public IDictionary Items
		{ 
			get{ return _httpApplication.Context.Items; }
		}

		public IPrincipal User
		{ 
			get
			{
				return _httpApplication.Context.User;
			}
			set
			{
				_httpApplication.Context.User = value;
			}
		}

		public void AddCookie(HttpCookie cookie)
		{
			_httpApplication.Response.Cookies.Add( cookie );
		}

		public string GetCookieValue(string name)
		{
			if(_httpApplication.Request.Cookies.Count > 0 && _httpApplication.Request.Cookies[name] != null)
			{
				HttpCookie cookie = _httpApplication.Request.Cookies[name];
				if(cookie != null)
					return cookie.Value;
			}
			return null;
		}

		public void RemoveCookie(string name)
		{
			if(_httpApplication.Request.Cookies.Count > 0 && _httpApplication.Request.Cookies[name] != null)
			{
				HttpCookie cookie = _httpApplication.Request.Cookies[name];
				cookie.Expires = DateTime.Now.AddDays(-1);
				_httpApplication.Response.Cookies.Add(cookie);
			}
		}

		public NameValueCollection Headers 
		{
			get{ return _httpApplication.Request.Headers; }
		}

		public ServiceMap ServiceMap
		{ 
			get
			{
				return _httpApplication.Context.Application["ServiceMap"] as ServiceMap;
			}
		}

		public CacheMap CacheMap 
		{	
			get
			{
				return _httpApplication.Context.Application["CacheMap"] as CacheMap;
			}
		}

		public ClassMappings ClassMappings
		{
			get
			{
				return _httpApplication.Context.Application["ClassMappings"] as ClassMappings;
			}
		}

		public string GetServiceName(string serviceLocation)
		{
			if( this.ServiceMap != null )
				return this.ServiceMap.GetServiceName(serviceLocation);
			return serviceLocation;
		}

		public string GetServiceLocation(string serviceName)
		{
			if( this.ServiceMap != null )
				return this.ServiceMap.GetServiceLocation(serviceName);
			return serviceName;
		}

		public string GetMethodName(string serviceLocation, string method)
		{
			if( this.ServiceMap != null )
				return this.ServiceMap.GetMethodName(serviceLocation, method);
			return method;
		}

		public string GetMappedTypeName(string customClass)
		{
			if( this.ClassMappings != null )
				return this.ClassMappings.GetType(customClass);
			else
				return customClass;
		}

		public string GetCustomClass(string type)
		{
			if( this.ClassMappings != null )
				return this.ClassMappings.GetCustomClass(type);
			else
				return type;
		}

		public AMFBody[] FailedAMFBodies
		{ 
			get
			{
				return _failedAMFBodies.ToArray(typeof(AMFBody)) as AMFBody[];
			}
		}

		public void Fail(AMFBody amfBody, Exception exception)
		{
			_failedAMFBodies.Add( new ErrorResponseBody(amfBody, exception) );
		}

		public string ActivationMode
		{ 
			get
			{
				try
				{
					return HttpContext.Current.Request.QueryString["activate"] as string;
				}
				catch(HttpException)//Request is not available in this context
				{
				}
				return null;
			}
		}

		public bool WsdlGenerateProxyClasses
		{ 
			get
			{ 
				if( _httpApplication.Context.Application["WsdlGenerateProxyClasses"] != null )
					return (bool)_httpApplication.Context.Application["WsdlGenerateProxyClasses"]; 
				return true;
			}
		}
		
		public string WsdlProxyNamespace
		{
			get{ return _httpApplication.Context.Application["WsdlProxyNamespace"] as string; }
		}

		public NameValueCollection ImportNamespaces
		{ 
			get{ return _httpApplication.Context.Application["ImportNamespaces"] as NameValueCollection; }
		}

		public Hashtable NullableValues
		{ 
			get{ return _httpApplication.Context.Application["NullableValues"] as Hashtable; }
		}

		public bool AcceptNullValueTypes
		{ 
			get
			{ 
				if( _httpApplication.Context.Application["AcceptNullValueTypes"] != null )
					return (bool)_httpApplication.Context.Application["AcceptNullValueTypes"]; 
				return false;
			}
		}

		public RemotingServiceAttributeConstraint RemotingServiceAttributeConstraint
		{
			get
			{
				if( _httpApplication.Context.Application["RemotingServiceAttributeConstraint"] != null )
					return (RemotingServiceAttributeConstraint)_httpApplication.Context.Application["RemotingServiceAttributeConstraint"]; 
				return RemotingServiceAttributeConstraint.Browse;
			}
		}
		
		/// <summary>
		/// Gets the directory that the assembly resolver used to probe for dynamically-created assemblies.
		/// </summary>
		public string DynamicDirectory
		{
			get
			{
				try
				{
					Uri uri;
					if( TypeHelper.DetectMono() )
						//Mono detection
						//http://lists.ximian.com/pipermail/mono-list/2005-May/027274.html
						//DynamicDirectory on Mono cannot be accessed
						uri = new Uri(AppDomain.CurrentDomain.SetupInformation.DynamicBase);
					else
						//.NET2 assemblies in DynamicDirectory
						uri = new Uri( AppDomain.CurrentDomain.DynamicDirectory );
					string path = uri.LocalPath;
					return path;
				}
				catch
				{
					return null;
				}
			}
		}

		#endregion
		
	}
}
