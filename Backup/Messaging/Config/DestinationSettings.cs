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

namespace com.TheSilentGroup.Fluorine.Messaging.Config
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	internal class DestinationSettings : Hashtable
	{
		public const string FluorineDestination = "fluorine";

		ServiceSettings _serviceSettings;

		public DestinationSettings(ServiceSettings serviceSettings)
		{
			_serviceSettings = serviceSettings;
			this["properties"] = new Hashtable();
			this["security"] = new SecuritySettings(this);
		}

		public string Id
		{
			get{ return this["id"] as string; }
		}

		public ServiceSettings ServiceSettings
		{
			get{ return _serviceSettings; }
		}

		public AdapterSettings AdapterSettings
		{
			get{ return this["adapter"] as AdapterSettings; }
		}

		public Hashtable Properties
		{
			get{ return this["properties"] as Hashtable; }
		}

		public SecuritySettings SecuritySettings
		{
			get{ return this["security"] as SecuritySettings; }
		}

		public NetworkSettings NetworkSettings
		{ 
			get
			{ 
				if( this.Properties != null )
					return this.Properties["network"] as NetworkSettings; 
				return null;
			}
		}

		public MetadataSettings MetadataSettings
		{ 
			get
			{ 
				if( this.Properties != null )
					return this.Properties["metadata"] as MetadataSettings; 
				return null;
			}
		}
	}
}
