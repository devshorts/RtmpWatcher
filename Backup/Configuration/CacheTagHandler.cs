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
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	sealed class CacheTagHandler : IConfigurationSectionHandler
	{
		/// <summary>
		/// Initializes a new instance of the CacheTagHandler class.
		/// </summary>
		public CacheTagHandler()
		{
		}

		#region IConfigurationSectionHandler Members

		public object Create(object parent, object configContext, XmlNode section)
		{
			XmlNodeList cacheEntryNodes = section.SelectNodes("service");
			if( cacheEntryNodes != null )
			{
				CacheMap cacheMap = new CacheMap();
				foreach(XmlNode cacheEntryNode in cacheEntryNodes)
				{
					string serviceName = cacheEntryNode.InnerText;
					int timeout = 30;
					XmlAttribute attribute = cacheEntryNode.Attributes.GetNamedItem("timeout") as XmlAttribute;
					if( attribute != null )
						timeout = Convert.ToInt32(attribute.Value);
					bool slidingExpiration = false;
					attribute = cacheEntryNode.Attributes.GetNamedItem("slidingExpiration") as XmlAttribute;
					if( attribute != null )
						slidingExpiration = Convert.ToBoolean(attribute.Value);
					cacheMap.AddCacheDescriptor( serviceName, timeout, slidingExpiration );
				}
				return cacheMap;
			}
			return null;
		}

		#endregion
	}
}
