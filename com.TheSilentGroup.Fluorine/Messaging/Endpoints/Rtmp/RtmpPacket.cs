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
using com.TheSilentGroup.Fluorine.SystemHelpers.IO;

namespace com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	sealed class RtmpPacket
	{
		RtmpHeader		_header;
		RtmpMessage		_message;
		ByteBuffer		_data;

		public RtmpPacket(RtmpHeader header)
		{
			_header = header;
			_data = ByteBuffer.Allocate(header.Size + (header.Timer == 0xffffff ? 4 : 0));
		}

		public RtmpPacket(RtmpHeader header, RtmpMessage message)
		{
			_header = header;
			_message = message;
		}

		public RtmpHeader Header
		{ 
			get{ return _header; }
			set{ _header = value; } 
		}

		public RtmpMessage Message
		{ 
			get{ return _message; }
			set{ _message = value; } 
		}

		public ByteBuffer Data
		{ 
			get{ return _data; }
			set{ _data = value; } 
		}
	}
}
