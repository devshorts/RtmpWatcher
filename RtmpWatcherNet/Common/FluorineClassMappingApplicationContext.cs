using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Security.Principal;
using System.Web;
using com.TheSilentGroup.Fluorine;
using com.TheSilentGroup.Fluorine.Configuration;
using com.TheSilentGroup.Fluorine.SystemHelpers;

namespace RtmpWatcherNet.Common
{
    public class FluorineClassMappingApplicationContext : DisposableBase, IApplicationContext
    {
        public FluorineClassMappingApplicationContext()
        {
            ClassMappings = new ClassMappings();
        }

        public string GetCustomClass(string type)
        {
            return ClassMappings.GetCustomClass(type);
        }

        public Hashtable NullableValues
        {
            get { return null; }
        }

        protected override void Dispose(bool disposing)
        {
        }

        #region DummpMethods Should Not Be Called 

        public void AddHeader(string name, string value)
        {
            throw new NotImplementedException();
        }

        public string MapPath(string path)
        {
            throw new NotImplementedException();
        }

        public void Execute(string path, TextWriter writer)
        {
            throw new NotImplementedException();
        }

        public void AddCookie(HttpCookie cookie)
        {
            throw new NotImplementedException();
        }

        public string GetCookieValue(string name)
        {
            throw new NotImplementedException();
        }

        public void RemoveCookie(string name)
        {
            throw new NotImplementedException();
        }

        public string GetMappedTypeName(string type)
        {
            return ClassMappings.GetType(type);
        }

        public string GetServiceName(string serviceLocation)
        {
            throw new NotImplementedException();
        }

        public string GetServiceLocation(string serviceName)
        {
            throw new NotImplementedException();
        }

        public string GetMethodName(string serviceLocation, string method)
        {
            throw new NotImplementedException();
        }

        public void Fail(AMFBody amfBody, Exception exception)
        {
            throw new NotImplementedException();
        }

        public string ApplicationPath
        {
            get { throw new NotImplementedException(); }
        }

        public string RootPath
        {
            get { throw new NotImplementedException(); }
        }

        public string RequestPath
        {
            get { throw new NotImplementedException(); }
        }

        public string RequestApplicationPath
        {
            get { throw new NotImplementedException(); }
        }

        public string DynamicDirectory
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsSecureConnection
        {
            get { throw new NotImplementedException(); }
        }

        public Stream InputStream
        {
            get { throw new NotImplementedException(); }
        }

        public Stream OutputStream
        {
            get { throw new NotImplementedException(); }
        }

        public ISessionState Session
        {
            get { throw new NotImplementedException(); }
        }

        public IDictionary Items
        {
            get { throw new NotImplementedException(); }
        }

        public IPrincipal User
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public NameValueCollection Headers
        {
            get { throw new NotImplementedException(); }
        }

        public ClassMappings ClassMappings { get; private set; }
        
        public ServiceMap ServiceMap
        {
            get { throw new NotImplementedException(); }
        }

        public CacheMap CacheMap
        {
            get { throw new NotImplementedException(); }
        }

        public bool WsdlGenerateProxyClasses
        {
            get { throw new NotImplementedException(); }
        }

        public string WsdlProxyNamespace
        {
            get { throw new NotImplementedException(); }
        }

        public NameValueCollection ImportNamespaces
        {
            get { throw new NotImplementedException(); }
        }

        public bool AcceptNullValueTypes
        {
            get { throw new NotImplementedException(); }
        }

        public RemotingServiceAttributeConstraint RemotingServiceAttributeConstraint
        {
            get { throw new NotImplementedException(); }
        }

        public AMFBody[] FailedAMFBodies
        {
            get { throw new NotImplementedException(); }
        }

        public string ActivationMode
        {
            get { return null; } 
        }

        #endregion

        
    }
}