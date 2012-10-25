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
using System.Xml;
using System.Collections;
using com.TheSilentGroup.Fluorine.Activation;

namespace com.TheSilentGroup.Fluorine.Messaging.Config
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class ServiceSettings : Hashtable
	{
		Hashtable		_supportedMessageTypes;
		Hashtable		_destinationSettings;
		Hashtable		_adapterSettings;
		AdapterSettings	_defaultAdapterSettings;
		ServerSettings	_serverSettings;
		object _objLock = new object();


		public ServiceSettings(ServerSettings serverSettings)
		{
			_serverSettings = serverSettings;
			_supportedMessageTypes = new Hashtable(1);
			_destinationSettings = new Hashtable();
			_adapterSettings = new Hashtable();
		}

		public void Init(IApplicationContext applicationContext, string configPath)
		{
			XmlDocument servicesXml = new XmlDocument();
			servicesXml.Load(configPath);
			XmlElement root = servicesXml.DocumentElement;
			Init(applicationContext, root);
		}

		public void Init(IApplicationContext applicationContext, XmlNode serviceElement)
		{
			this["id"] = serviceElement.Attributes["id"].Value;
			this["class"] = serviceElement.Attributes["class"].Value;
			string messageTypes = serviceElement.Attributes["messageTypes"].Value;
			string[] messageTypesList = messageTypes.Split(new char[]{','}); 
			foreach(string messageType in messageTypesList)
			{
				string type = applicationContext.ClassMappings.GetType(messageType);
				_supportedMessageTypes[messageType] = type;
			}
			//Read adapters
			XmlNode adaptersNode = serviceElement.SelectSingleNode("adapters");
			if( adaptersNode != null )
			{
				foreach(XmlNode adapterNode in adaptersNode.ChildNodes)
				{
					AdapterSettings adapterSettings = new AdapterSettings();
					string id = adapterNode.Attributes["id"].Value;
					adapterSettings["id"] = id;
					adapterSettings["class"] = adapterNode.Attributes["class"].Value;
					_adapterSettings[id] = adapterSettings;

					if( adapterNode.Attributes["default"] != null && adapterNode.Attributes["default"].Value == "true" )
						_defaultAdapterSettings = adapterSettings;
				}
			}
			else
			{
				AdapterSettings adapterSettings = new AdapterSettings();
				adapterSettings["id"] = "dotnet";
				adapterSettings["class"] = "com.TheSilentGroup.Fluorine.Remoting.RemotingAdapter";
				_defaultAdapterSettings = adapterSettings;
				_adapterSettings[adapterSettings["id"]] = adapterSettings;
			}
			//Read destinations
			XmlNodeList destinationNodeList = serviceElement.SelectNodes("destination");
			foreach(XmlNode destinationNode in destinationNodeList)
			{
				string id = destinationNode.Attributes["id"].Value;

				DestinationSettings destinationSettings = new DestinationSettings(this);
				destinationSettings["id"] = id;

				XmlNode adapterNode = destinationNode.SelectSingleNode("adapter");
				if( adapterNode != null )
				{
					string adapterRef = adapterNode.Attributes["ref"].Value;
					AdapterSettings adapterSettings = _adapterSettings[adapterRef] as AdapterSettings;
					destinationSettings["adapter"] = adapterSettings;
				}

				XmlNode propertiesNode = destinationNode.SelectSingleNode("properties");
				if( propertiesNode != null )
				{
					Hashtable properties = new Hashtable();
					destinationSettings["properties"] = properties;

					XmlNode sourceNode = propertiesNode.SelectSingleNode("source");
					if( sourceNode != null )
						properties["source"] = sourceNode.InnerXml;
					XmlNode scopeNode = propertiesNode.SelectSingleNode("scope");
					if( scopeNode != null )
					{
						properties["scope"] = scopeNode.InnerXml;
						if( sourceNode.InnerXml != "*" )
							ObjectFactory.AddActivator(applicationContext, scopeNode.InnerXml, sourceNode.InnerXml);
					}

					XmlNode networkNode = propertiesNode.SelectSingleNode("network");
					if( networkNode != null )
					{
						NetworkSettings networkSettings = new NetworkSettings(networkNode);
						properties["network"] = networkSettings;
					}
					XmlNode metadataNode = propertiesNode.SelectSingleNode("metadata");
					if( metadataNode != null )
					{
						MetadataSettings metadataSettings = new MetadataSettings(metadataNode);
						properties["metadata"] = metadataSettings;
					}
				}
				XmlNode securityNode = destinationNode.SelectSingleNode("security");
				if( securityNode != null )
				{
					SecuritySettings securitySettings = new SecuritySettings(destinationSettings, securityNode);
					destinationSettings["security"] = securitySettings;
				}
				_destinationSettings[id] = destinationSettings;
			}
		}

		public Hashtable SupportedMessageTypes{ get{ return _supportedMessageTypes; } }

		public Hashtable DestinationSettings{ get{ return _destinationSettings; } }

		public AdapterSettings DefaultAdapterSettings
		{ 
			get{ return _defaultAdapterSettings; } 
			set{ _defaultAdapterSettings = value; }
		}

		public string ServiceTypeName{ get{ return this["class"] as string; } }

		public ServerSettings ServerSettings{ get{ return _serverSettings; } }

		public DestinationSettings CreateDestinationSettings(string id, string source)
		{
			lock(_objLock)
			{
				if( !this.DestinationSettings.ContainsKey(id) )
				{
					AdapterSettings adapterSettings = new AdapterSettings();
					adapterSettings["id"] = "dotnet";
					adapterSettings["class"] = "com.TheSilentGroup.Fluorine.Remoting.RemotingAdapter";
			
					DestinationSettings destinationSettings = new DestinationSettings(this);
					destinationSettings["id"] = id;
					destinationSettings["adapter"] = adapterSettings;
					destinationSettings.Properties["source"] = "*";

					this.DestinationSettings[ id ] = destinationSettings;
					return destinationSettings;
				}
				else
					return this.DestinationSettings[id] as DestinationSettings;
			}
		}
	}
}
