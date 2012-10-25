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
	sealed class ClassMapping
	{
		string _type;
		string _customClass;

		/// <summary>
		/// Initializes a new instance of the ClassMapping class.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="customClass"></param>
		internal ClassMapping(string type, string customClass)
		{
			_type = type;
			_customClass = customClass;
		}
		/// <summary>
		/// Gets the type name (the .Net class name).
		/// </summary>
		public string Type
		{
			get{ return _type; }
		}
		/// <summary>
		/// Gets the custom class name (class name in AS)
		/// </summary>
		public string CustomClass
		{
			get{ return _customClass; }
		}
	}

	/// <summary>
	/// Retrieves custom tags from the configuration file.
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	sealed class CustomTagHandler : IConfigurationSectionHandler
	{
		/// <summary>
		/// Initializes a new instance of the CustomTagHandler class.
		/// </summary>
		public CustomTagHandler()
		{
		}

		#region IConfigurationSectionHandler Members

		public object Create(object parent, object configContext, XmlNode section)
		{
			ClassMappings classMappings = new ClassMappings();
			XmlNodeList classMappingNodes = section.SelectNodes("classMapping");
			foreach(XmlNode classMappingNode in classMappingNodes)
			{
				string type = classMappingNode.SelectSingleNode("type").InnerText;
				string customClass = classMappingNode.SelectSingleNode("customClass").InnerText;
				classMappings.Add( type, customClass );
			}
			return classMappings;
		}

		#endregion
	}
}
