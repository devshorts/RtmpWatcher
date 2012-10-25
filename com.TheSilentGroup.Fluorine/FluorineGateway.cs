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
using System.Web.SessionState;
using System.Security;
using System.Security.Permissions;

// Import log4net classes.
using log4net;
using com.TheSilentGroup.Fluorine.Gateway;
using com.TheSilentGroup.Fluorine.Messaging;

namespace com.TheSilentGroup.Fluorine
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	public class FluorineGateway : IHttpModule, IRequiresSessionState
	{
		static int _unhandledExceptionCount = 0;
		static string _sourceName = null;
		static object _objLock = new object();
		static bool _initialized = false;

		/// <summary>
		/// Initializes a new instance of the FluorineGateway class.
		/// </summary>
		public FluorineGateway()
		{
		}

		private MessageServer GetMessageServer()
		{
			return HttpContext.Current.Application["FluorineMessageServer"] as MessageServer;
		}

		#region IHttpModule Members

		/// <summary>
		/// Initializes the module and prepares it to handle requests.
		/// </summary>
		/// <param name="application">An HttpApplication that provides access to the methods, properties, and events common to all application objects within an ASP.NET application.</param>
		public void Init(HttpApplication application)
		{
			try
			{
				ILog log = LogManager.GetLogger(typeof(FluorineGateway));
				log.Info("Starting FluorineGateway HttpModule.");
			}
			catch { }

			//http://support.microsoft.com/kb/911816
			// Do this one time for each AppDomain.
			if (!_initialized) 
			{
				lock (_objLock) 
				{
					if (!_initialized) 
					{ 
						try
						{
							// See if we're running in full trust
							new PermissionSet(PermissionState.Unrestricted).Demand();
						}
						catch(SecurityException)
						{
						}

						_initialized = true;
					}
				}
			}

			//Wire up the HttpApplication events.
			//
			//BeginRequest 
			//AuthenticateRequest 
			//AuthorizeRequest 
			//ResolveRequestCache 
			//A handler (a page corresponding to the request URL) is created at this point.
			//AcquireRequestState ** Session State ** 
			//PreRequestHandlerExecute 
			//[The handler is executed.] 
			//PostRequestHandlerExecute 
			//ReleaseRequestState 
			//Response filters, if any, filter the output.
			//UpdateRequestCache 
			//EndRequest 

			application.BeginRequest += new EventHandler(context_BeginRequest);
			application.PreRequestHandlerExecute += new EventHandler(application_PreRequestHandlerExecute);
			application.AuthenticateRequest += new EventHandler(application_AuthenticateRequest);

			if (GetMessageServer() == null)
			{
				lock (_objLock) 
				{
					if (GetMessageServer() == null)
					{
						try
						{
							ILog log = LogManager.GetLogger(typeof(FluorineGateway));
							log.Info("Creating Message Server.");
						}
						catch { }

						MessageServer messageServer = new MessageServer();
						using (FluorineHttpApplicationContext applicationContext = new FluorineHttpApplicationContext(application, messageServer))
						{
							try
							{
								applicationContext.Init(application);
								string configPath = HttpRuntime.AppDomainAppPath + System.IO.Path.DirectorySeparatorChar + "web-inf" + System.IO.Path.DirectorySeparatorChar + "flex" + System.IO.Path.DirectorySeparatorChar;
								messageServer.Init(applicationContext, configPath);
								messageServer.Start();

								try
								{
									ILog log = LogManager.GetLogger(typeof(FluorineGateway));
									log.Info("Message Server started.");
								}
								catch { }

								HttpContext.Current.Application["FluorineMessageServer"] = messageServer;
							}
							catch (Exception ex)
							{
								try
								{
									ILog log = LogManager.GetLogger(typeof(FluorineGateway));
									log.Fatal("Failed to start message server.", ex);
								}
								catch { }
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Disposes of the resources (other than memory) used by the module that implements IHttpModule.
		/// </summary>
		public void Dispose()
		{
		}

		#endregion

		/// <summary>
		/// Occurs as the first event in the HTTP pipeline chain of execution when ASP.NET responds to a request.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void context_BeginRequest(object sender, EventArgs e)
		{
			HttpApplication httpApplication = (HttpApplication)sender;
			HttpRequest httpRequest = httpApplication.Request;

			if( httpApplication.Request.ContentType == "application/x-amf" )
			{
				httpApplication.Context.SkipAuthorization = true;
			}
			//sessionState cookieless="true" requires to handle here an HTTP POST but Session is not available here
			//HandleXAmfEx(httpApplication);
		}

		/// <summary>
		/// Occurs just before ASP.NET begins executing a handler such as a page or XML Web service.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void application_PreRequestHandlerExecute(object sender, EventArgs e)
		{
			HttpApplication httpApplication = (HttpApplication)sender;
			HandleXAmfEx(httpApplication);
		}

		private void HandleXAmfEx(HttpApplication httpApplication)
		{
			if( httpApplication.Request.ContentType == "application/x-amf" )
			{
				httpApplication.Response.Clear();
				using (IApplicationContext applicationContext = new FluorineHttpApplicationContext(httpApplication, GetMessageServer()))
				{
					ILog log = null;
					try
					{
						log = LogManager.GetLogger(typeof(FluorineGateway));
					}
					catch{}
					if (log != null && log.IsDebugEnabled)
						log.Debug("Begin AMF request.");

					applicationContext.User = httpApplication.Context.User;

					try
					{
						MessageServer messageServer = GetMessageServer();
						if (messageServer != null)
							messageServer.Service(applicationContext/*, httpApplication*/);
						else
						{
							if (log != null)
								log.Fatal("Failed to access Message Server.");
						}

						if( log != null && log.IsDebugEnabled )
							log.Debug("End AMF request.");

						//http://support.microsoft.com/default.aspx?scid=kb;en-us;312629
						//httpApplication.Response.End();
						httpApplication.CompleteRequest();
					}
					catch(Exception ex)
					{
						if(log != null )
							log.Fatal("Failed to handle AMF message", ex);
						httpApplication.Response.Clear();
						httpApplication.Response.ClearHeaders();//FluorineHttpApplicationContext modifies headers
						httpApplication.Response.Status = "404 Failed to handle AMF message. " + ex.Message;
						httpApplication.CompleteRequest();
					}
				}
			}
		}
		/// <summary>
		/// Occurs when a security module has established the identity of the user.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void application_AuthenticateRequest(object sender, EventArgs e)
		{
			HttpApplication httpApplication = (HttpApplication)sender;
			if( httpApplication.Request.ContentType == "application/x-amf" )
			{
				httpApplication.Context.SkipAuthorization = true;
			}
		}

	}
}
