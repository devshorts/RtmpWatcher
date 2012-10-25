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
using System.Configuration;
using System.Web;
using System.Xml;
// Import log4net classes.
using com.TheSilentGroup.Fluorine.SystemHelpers;
using log4net;
using log4net.Config;

using com.TheSilentGroup.Fluorine.Activation;
using com.TheSilentGroup.Fluorine.Messaging.Config;
using com.TheSilentGroup.Fluorine.Messaging.Endpoints;
using com.TheSilentGroup.Fluorine.Messaging.Services;
using com.TheSilentGroup.Fluorine.Messaging.Security;

namespace com.TheSilentGroup.Fluorine.Messaging
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	public class MessageServer : DisposableBase
	{
		private ILog _log;

		ServerSettings	_serverSettings;
		MessageBroker	_messageBroker;

		/// <summary>
		/// Initializes a new instance of the MessageServer class.
		/// </summary>
		public MessageServer()
		{
			try
			{
				_log = LogManager.GetLogger(typeof(MessageServer));
			}
			catch{}
			_messageBroker = new MessageBroker();
		}

		public void Init(IApplicationContext applicationContext, string configPath)
		{
			_serverSettings = new ServerSettings();
			XmlDocument servicesConfigXml = new XmlDocument();
			if( System.IO.File.Exists(configPath  + "services-config.xml") )
			{
				servicesConfigXml.Load(configPath  + "services-config.xml");
				XmlNodeList channelsNodeList = servicesConfigXml.SelectNodes("/services-config/channels/channel-definition");
				foreach(XmlNode channelDefinitionNode in channelsNodeList)
				{
					XmlNode endPointNode = channelDefinitionNode.SelectSingleNode("endpoint");
					string endpointClass = endPointNode.Attributes["class"].Value;
					string endpointUri = endPointNode.Attributes["uri"].Value;

					ChannelSettings channelSettings = new ChannelSettings(channelDefinitionNode);
					Type type = ObjectFactory.Locate(applicationContext, endpointClass);
					if( type != null )
					{
						IEndpoint endpoint = ObjectFactory.CreateInstance(applicationContext, type, new object[]{_messageBroker, channelSettings}) as IEndpoint;
				
						if( endpoint != null )
							_messageBroker.AddEndpoint(endpoint);
					}
				}
				XmlNodeList servicesIncludesNodeList = servicesConfigXml.SelectNodes("/services-config/services/service-include");
				foreach(XmlNode servicesIncludeNode in servicesIncludesNodeList)
				{
					string filePath = servicesIncludeNode.Attributes["file-path"].Value;
					ServiceSettings serviceSettings = new ServiceSettings(_serverSettings);
					serviceSettings.Init(applicationContext, configPath + filePath);
					Type type = ObjectFactory.Locate(applicationContext, serviceSettings.ServiceTypeName);//current assembly only
					if( type != null )
					{
						IService service = ObjectFactory.CreateInstance(applicationContext, type, new object[]{applicationContext, _messageBroker, serviceSettings}) as IService;
				
						if( service != null )
							_messageBroker.AddService(service);
					}
				}
				XmlNodeList servicesNodeList = servicesConfigXml.SelectNodes("/services-config/services/service");
				foreach(XmlNode serviceNode in servicesNodeList)
				{
					ServiceSettings serviceSettings = new ServiceSettings(_serverSettings);
					serviceSettings.Init(applicationContext, serviceNode);
					Type type = ObjectFactory.Locate(applicationContext, serviceSettings.ServiceTypeName);//current assembly only
					if( type != null )
					{
						IService service = ObjectFactory.CreateInstance(applicationContext, type, new object[]{applicationContext, _messageBroker, serviceSettings}) as IService;
				
						if( service != null )
							_messageBroker.AddService(service);
					}
				}
				
				XmlNode securityNode = servicesConfigXml.SelectSingleNode("/services-config/security");
				if( securityNode != null )
				{
					SecuritySettings securitySettings = new SecuritySettings(null, securityNode);
					_messageBroker.SecuritySettings = securitySettings;

					if( securitySettings.LoginCommand != null )
					{
						Type type = ObjectFactory.Locate(applicationContext, securitySettings.LoginCommand["class"] as string);
						if( type != null )
						{
							ILoginCommand loginCommand = ObjectFactory.CreateInstance(applicationContext, type, new object[]{}) as ILoginCommand;
							_messageBroker.LoginCommand = loginCommand;
						}
					}
					_serverSettings["security"] = securitySettings;
				}

				InitAuthenticationService(applicationContext);
			}
			else
			{
				Hashtable loginCommands = (Hashtable)ConfigurationSettings.GetConfig("fluorine/security");
				if( loginCommands != null && loginCommands.Contains("asp.net") )
				{
					string loginCommandClass = loginCommands["asp.net"] as string;
					Type loginCommandType = ObjectFactory.Locate(applicationContext, loginCommandClass);
					if( loginCommandType != null )
					{
						ILoginCommand loginCommand = ObjectFactory.CreateInstance(applicationContext, loginCommandType, new object[]{}) as ILoginCommand;
						_messageBroker.LoginCommand = loginCommand;
					}
				}

				//create a default amf
				ChannelSettings channelSettings = new ChannelSettings();
				channelSettings.Id = "my-amf";
				/*
				if( applicationContext.RequestApplicationPath == "/" )
					channelSettings.Uri = @"http://{server.name}:{server.port}/Gateway.aspx";
				else
					channelSettings.Uri = @"http://{server.name}:{server.port}" + applicationContext.RequestApplicationPath + "/Gateway.aspx";
				*/
				channelSettings.Uri = @"http://{server.name}:{server.port}/{context.root}/Gateway.aspx";
				string endpointClass = "flex.messaging.endpoints.AMFEndpoint";
				Type type = ObjectFactory.Locate(applicationContext, endpointClass);
				if( type != null )
				{
					IEndpoint endpoint = ObjectFactory.CreateInstance(applicationContext, type, new object[]{_messageBroker, channelSettings}) as IEndpoint;
					if( endpoint != null )
						_messageBroker.AddEndpoint(endpoint);
				}
				
				ServiceSettings serviceSettings = new ServiceSettings(_serverSettings);
				serviceSettings["id"] = RemotingService.RemotingServiceId;
				string messageType = "flex.messaging.messages.RemotingMessage";
				string typeName = applicationContext.ClassMappings.GetType(messageType);
				serviceSettings.SupportedMessageTypes[messageType] = typeName;

				AdapterSettings adapterSettings = new AdapterSettings();
				adapterSettings["id"] = "dotnet";
				adapterSettings["class"] = "com.TheSilentGroup.Fluorine.Remoting.RemotingAdapter";
				serviceSettings.DefaultAdapterSettings = adapterSettings;

				DestinationSettings destinationSettings = new DestinationSettings(serviceSettings);
				destinationSettings["id"] = DestinationSettings.FluorineDestination;
				destinationSettings["adapter"] = adapterSettings;
				destinationSettings.Properties["source"] = "*";
				serviceSettings.DestinationSettings[ destinationSettings["id"] ] = destinationSettings;

				IService service = new RemotingService(applicationContext, _messageBroker, serviceSettings);
				_messageBroker.AddService(service);
				
				InitAuthenticationService(applicationContext);
			}
		}

		private void InitAuthenticationService(IApplicationContext applicationContext)
		{
			ServiceSettings serviceSettings = new ServiceSettings(_serverSettings);
			serviceSettings["id"] = AuthenticationService.ServiceId;
			string messageType = "flex.messaging.messages.AuthenticationMessage";
			string typeName = applicationContext.ClassMappings.GetType(messageType);
			serviceSettings.SupportedMessageTypes[messageType] = typeName;
			AuthenticationService service = new AuthenticationService(applicationContext, _messageBroker, serviceSettings);
			_messageBroker.AddService(service);
		}

		public MessageBroker MessageBroker{ get { return _messageBroker; } }

		public void Start()
		{
			if (_log != null && _log.IsInfoEnabled)
				_log.Info("Starting Message Broker.");
			_messageBroker.Start();
		}

		public void Stop()
		{
			if( _messageBroker != null )
			{
				if (_log != null && _log.IsInfoEnabled)
					_log.Info("Stopping Message Broker.");
				_messageBroker.Stop();
				_messageBroker = null;
			}
		}

		#region IDisposable Members

		protected override void Free()
		{
			if (_messageBroker != null)
			{
				if (_log != null && _log.IsInfoEnabled)
					_log.Info("Dispose Message Server.");
				Stop();
			}
		}

		protected override void FreeUnmanaged()
		{
			if (_messageBroker != null)
			{
				Stop();
			}
		}


		#endregion

		public void Service(IApplicationContext applicationContext/*, HttpApplication httpApplication*/)
		{
			//This is equivalent to request.getContextPath() (Java) or the HttpRequest.ApplicationPath (.Net).
			//string contextPath = httpApplication.Request.ApplicationPath;
			string contextPath = applicationContext.RequestApplicationPath;
			//string endpointPath = httpApplication.Request.Path;
			string endpointPath = applicationContext.RequestPath;
			//bool isSecure = httpApplication.Request.IsSecureConnection;
			bool isSecure = applicationContext.IsSecureConnection;

			if( _log != null && _log.IsDebugEnabled )
				_log.Debug("Query endpoint endpointPath:" + endpointPath + " contextPath:" + contextPath);

			//http://www.adobe.com/cfusion/knowledgebase/index.cfm?id=e329643d&pss=rss_flex_e329643d
			IEndpoint endpoint = _messageBroker.GetEndpoint(applicationContext, endpointPath, contextPath, isSecure);
			if( endpoint != null )
			{
				endpoint.Service(applicationContext);
			}
			else
			{
				string msg = "Could not bind request to endpoint " + endpointPath + ", contextPath " + contextPath;
				if( _log != null && _log.IsFatalEnabled )
					_log.Fatal(msg);
				throw new Exceptions.FluorineException(msg);
			}
		}

	}
}
