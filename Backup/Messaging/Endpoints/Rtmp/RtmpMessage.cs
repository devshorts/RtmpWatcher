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
using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api.Event;
using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Event;

namespace com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class RtmpMessage : IRtmpEvent
	{
		protected RtmpHeader _header;
		protected object _object;
		protected int _refcount = 1;
		protected int _timestamp;
		protected DataType _dataType;
		protected IEventListener _source;

		public RtmpMessage()
		{
			_object = null;
		}

		public RtmpHeader Header
		{
			get{ return _header; }
			set{ _header = value; }
		}
		public object Object
		{ 
			get{ return _object; }
		}

		public int Timestamp
		{
			get{ return _timestamp; }
			set{ _timestamp = value; }
		}

		public DataType DataType
		{
			get{ return _dataType; }
			set{ _dataType = value; }
		}

		public IEventListener Source
		{
			get{ return _source; }
		}

		public void SetSource(IEventListener source)
		{
			_source = source;
		}

		public bool HasSource
		{
			get{ return _source != null; }
		}

		public void AddRef()
		{
			if(_refcount > 0) 
				_refcount++;
		}

		public void Retain()
		{
		}

		public void Release()
		{
			lock(this)
			{
				if(_refcount > 0) 
					_refcount--;
			}
		}
	}
}
