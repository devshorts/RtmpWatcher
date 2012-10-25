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
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Specialized;

using System.Web;
using System.Web.SessionState;
using System.Security.Principal;

using com.TheSilentGroup.Fluorine.Configuration;
using com.TheSilentGroup.Fluorine.Diagnostic;
using com.TheSilentGroup.Fluorine.Messaging;

namespace com.TheSilentGroup.Fluorine
{
	public enum RemotingServiceAttributeConstraint
	{
		Browse = 1,
		Access = 2
	}

	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	public interface IApplicationContext : IDisposable
	{
		void AddHeader(string name, string value);
		string ApplicationPath { get; }
		string RootPath { get; }
		string RequestPath { get; }
		string RequestApplicationPath { get; }
		string DynamicDirectory { get; }
		bool IsSecureConnection { get; }
		Stream InputStream { get; }
		Stream OutputStream { get; }
		ISessionState Session { get; }
		string MapPath(string path);
		void Execute(string path, TextWriter writer);
		IDictionary Items{ get; }
		IPrincipal User{ get; set; }
		void AddCookie(HttpCookie cookie);
		string GetCookieValue(string name);
		void RemoveCookie(string name);
		NameValueCollection Headers {get;}

		ClassMappings ClassMappings { get; }
		string GetMappedTypeName(string type);
		string GetCustomClass(string type);
		ServiceMap ServiceMap { get; }
		CacheMap CacheMap { get; }
		string GetServiceName(string serviceLocation);
		string GetServiceLocation(string serviceName);
		string GetMethodName(string serviceLocation, string method);
		bool WsdlGenerateProxyClasses{ get; }
		string WsdlProxyNamespace{ get; }
		NameValueCollection ImportNamespaces{ get; }
		bool AcceptNullValueTypes{ get; }
		Hashtable NullableValues{ get; }
		RemotingServiceAttributeConstraint RemotingServiceAttributeConstraint{ get; }

		AMFBody[] FailedAMFBodies{ get; }
		void Fail(AMFBody amfBody, Exception exception);

		/// <summary>
		/// If specified this will overwrite activation mode specified via attributes.
		/// </summary>
		string ActivationMode{ get; }
	}
}
