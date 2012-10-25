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

using com.TheSilentGroup.Fluorine.Messaging.Config;
using com.TheSilentGroup.Fluorine.Messaging;
using com.TheSilentGroup.Fluorine.Messaging.Messages;

namespace com.TheSilentGroup.Fluorine.Messaging.Services
{
	/// <summary>
	/// The ServiceAdapter class is the base definition of a service adapter.
	/// </summary>
	internal class ServiceAdapter : IAdapter
	{
		protected IService _service;
		protected Destination _destination;
		protected DestinationSettings _destinationSettings;
		protected AdapterSettings _adapterSettings;

		public ServiceAdapter()
		{
		}

		public virtual object Invoke(IApplicationContext applicationContext, IMessage message)
		{
			return null;
		}

		public virtual object Manage(IApplicationContext applicationContext, CommandMessage commandMessage)
		{
			return null;
		}

		public virtual bool IsHandlingSubscribe
		{
			get { return false; }
		}

		public virtual void Init()
		{
		}

		public IService Service { get { return _service; } set { _service = value; } }
		public Destination Destination { get { return _destination; } set { _destination = value; } }
		public DestinationSettings DestinationSettings { get { return _destinationSettings; } set { _destinationSettings = value; } }
		public AdapterSettings AdapterSettings { get { return _adapterSettings; } set { _adapterSettings = value; } }
	}
}
