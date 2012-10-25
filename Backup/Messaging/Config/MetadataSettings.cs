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

namespace com.TheSilentGroup.Fluorine.Messaging.Config
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	public class MetadataSettings : Hashtable
	{
		ArrayList _identity = new ArrayList();
		ArrayList _associations = new ArrayList();

		public MetadataSettings(XmlNode metadataDefinitionNode)
		{
			foreach(XmlNode propertyNode in metadataDefinitionNode.ChildNodes)
			{
				if( propertyNode.InnerXml != null && propertyNode.InnerXml != string.Empty )
					this[propertyNode.Name] = propertyNode.InnerXml;
				else
				{
					if( propertyNode.Name == "identity" )
					{
						foreach(XmlAttribute attribute in propertyNode.Attributes)
						{
							_identity.Add(attribute.Value);
						}
					}
					if( propertyNode.Name == "many-to-one" )
					{
						Hashtable association = new Hashtable(3);
						foreach(XmlAttribute attribute in propertyNode.Attributes)
						{
							association[attribute.Name] = attribute.Value;
						}
						_associations.Add(association);
					}
				}
			}
		}

		public ArrayList Identity
		{
			get
			{
				return _identity;
			}
		}
	}
}
