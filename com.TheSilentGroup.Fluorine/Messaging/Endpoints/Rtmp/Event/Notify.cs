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
using System.IO;
using System.Collections;

using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api;
using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api.Service;
using com.TheSilentGroup.Fluorine.SystemHelpers.IO;

namespace com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Event
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class Notify : RtmpMessage
	{
		protected IServiceCall		_serviceCall = null;
		protected ByteBuffer		_data = null;
		int							_invokeId = 0;
		Hashtable					_connectionParameters;

		public Notify():base()
		{
			_dataType = DataType.TypeNotify;
		}
		
		public Notify(ByteBuffer data):this()
		{
			_data = data;
		}

		public Notify(IServiceCall serviceCall):this()
		{
			_serviceCall = serviceCall;
		}

		public ByteBuffer Data
		{
			get{ return _data; }
			set{ _data = value; }
		}

		public int InvokeId
		{
			get{ return _invokeId; }
			set{ _invokeId = value; }
		}

		public IServiceCall ServiceCall
		{
			get{ return _serviceCall; }
			set{ _serviceCall = value; }
		}

		public Hashtable ConnectionParameters
		{
			get{ return _connectionParameters; }
			set{ _connectionParameters = value; }
		}
	}
}
