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
	sealed class SecurityTagHandler : IConfigurationSectionHandler
	{
		public SecurityTagHandler()
		{
		}

		#region IConfigurationSectionHandler Members

		public object Create(object parent, object configContext, XmlNode section)
		{
			Hashtable loginCommands = new Hashtable();
			XmlNodeList loginCommandEntryNodes = section.SelectNodes("login-command");
			if( loginCommandEntryNodes != null )
			{
				foreach(XmlNode loginCommandEntryNode in loginCommandEntryNodes)
				{
					string loginCommandClass = loginCommandEntryNode.Attributes["class"].Value;
					string server = loginCommandEntryNode.Attributes["server"].Value;
					loginCommands[server] = loginCommandClass;
				}
			}
			return loginCommands;
		}

		#endregion
	}
}
