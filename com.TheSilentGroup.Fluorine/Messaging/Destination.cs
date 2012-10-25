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

using com.TheSilentGroup.Fluorine.Activation;

using com.TheSilentGroup.Fluorine.Messaging.Services;
using com.TheSilentGroup.Fluorine.Messaging.Config;

namespace com.TheSilentGroup.Fluorine.Messaging
{
	/// <summary>
	/// The Destination class is a source and sink for messages sent through 
	/// a service destination and uses an adapter to process messages.
	/// </summary>
	internal class Destination
	{
		protected IService				_service;
		protected DestinationSettings	_settings;
		protected IAdapter				_adapter;

		public Destination(IService service, DestinationSettings settings)
		{
			_service = service;
			_settings = settings;
		}

		public string Id
		{
			get{ return _settings.Id; }
		}

		public void Init(IApplicationContext applicationContext, AdapterSettings adapterSettings)
		{
			if( adapterSettings != null )
			{
				string typeName = adapterSettings["class"] as string;
				//_adapter = new ServiceAdapter();
				Type type = ObjectFactory.Locate(applicationContext, typeName);
				if( type != null )
				{
					_adapter = ObjectFactory.CreateInstance(applicationContext, type) as IAdapter;
					_adapter.Service = _service;
					_adapter.Destination = this;
					_adapter.AdapterSettings = adapterSettings;
					_adapter.DestinationSettings = _settings;
					_adapter.Init();
				}
			}
		}

		public IAdapter ServiceAdapter{ get{ return _adapter; } }

		public DestinationSettings DestinationSettings{ get{ return _settings; } }
	}
}
