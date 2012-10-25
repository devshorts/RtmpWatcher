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
using System.Configuration;

namespace com.TheSilentGroup.Fluorine.Configuration
{
	/// <summary>
	/// Specifies a service entry.
	/// </summary>
	sealed class ServiceEntry
	{
	}

	/// <summary>
	/// Retrieves custom tags from the configuration file.
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	sealed class ServiceTagHandler : IConfigurationSectionHandler
	{
		/// <summary>
		/// Initializes a new instance of the ServiceTagHandler class.
		/// </summary>
		public ServiceTagHandler()
		{
		}

		#region IConfigurationSectionHandler Members

		public object Create(object parent, object configContext, XmlNode section)
		{
			ServiceMap serviceMap = new ServiceMap();
			XmlNodeList serviceEntryNodes = section.SelectNodes("service");
			if( serviceEntryNodes != null )
			{
				foreach(XmlNode serviceEntryNode in serviceEntryNodes)
				{
					string serviceName = serviceEntryNode.SelectSingleNode("name").InnerText;
					string serviceLocation = serviceEntryNode.SelectSingleNode("service-location").InnerText;
					serviceMap.Add( serviceName, serviceLocation );
				
					XmlNode methodsNode = serviceEntryNode.SelectSingleNode("methods");
					if(methodsNode != null )
					{
						foreach(XmlNode remoteMethodNode in methodsNode)
						{
							string name = remoteMethodNode.SelectSingleNode("name").InnerText;
							string method = remoteMethodNode.SelectSingleNode("method").InnerText;
							serviceMap.AddMethod( serviceName, name, method);
						}
					}
				}
			}
			return serviceMap;
		}

		#endregion
	}
}
