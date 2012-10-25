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

namespace com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp
{
	public enum HeaderType : byte
	{
		HeaderNew = 0,
		HeaderSameSource = 1,
		HeaderTimerChange = 2,
		HeaderContinue = 3
	}

	public enum DataType : byte
	{
		TypeChunkSize = 1,
		TypeBytesRead = 3,
		TypePing = 4,
		TypeServerBandwidth = 5,
		TypeClientBandwidth = 6,
		TypeAudioData = 8,
		TypeVideoData = 9,
		TypeNotify = 0x12,
		TypeSharedObject = 0x13,
		TypeInvoke = 0x14
	}

	public enum RtmpState
	{
		Connect = 0,
		Handshake = 1,
		Connected = 2,
		Error = 3,
		Disconnected = 4
	}

	public enum RtmpMode
	{
		Server = 0,
		Client = 1
	}

	public enum ScopeLevel
	{
		Global = 0,
		Application = 1,
		Room = 2
	}
}
