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
using System.Collections.Specialized;
using System.IO;

using com.TheSilentGroup.Fluorine.Filter;
using com.TheSilentGroup.Fluorine.Gateway;

using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api;
using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api.Stream;
using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api.Service;

namespace com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp
{
	/// <summary>
	/// Summary description for RtmpConnection.
	/// </summary>
	internal class RtmpConnection : ConnectionBase
	{
		object _objLock = new object();
		/// <summary>
		/// Persistent connection type, eg RTMP.
		/// </summary>
		public static string Persistent = "persistent";
		/// <summary>
		/// Polling connection type, eg RTMPT.
		/// </summary>
		public static string Polling = "polling";
		/// <summary>
		/// Transient connection type, eg Remoting, HTTP, etc.
		/// </summary>
		public static string Transient = "transient";

		RtmpContext	_context;
		Hashtable	_channels;

		RtmpService _rtmpService;
		string		_connectionId;

		int _invokeId = 1;

		protected const int MAX_STREAMS = 12;
		IClientStream[] _streams = new IClientStream[MAX_STREAMS];
		protected Hashtable _pendingCalls = new Hashtable();

		public RtmpConnection(RtmpService rtmpService, string type, string connectionId):base(type, null, null, 0, null, null)
		{
			// We start with an anonymous connection without a scope.
			// These parameters will be set during the call of "connect" later.
			_connectionId = connectionId;
			_rtmpService = rtmpService;
			_channels = new Hashtable();
			_context = new RtmpContext(RtmpMode.Server);
		}

		public void Setup(string host, string path, string sessionId, Hashtable parameters)
		{
			_host = host;
			_path = path;
			_sessionId = sessionId;
			_parameters = parameters;
			if( _parameters.ContainsKey("objectEncoding") )
			{
				int objectEncoding = System.Convert.ToInt32( _parameters["objectEncoding"] );
				_objectEncoding = (ObjectEncoding)objectEncoding;
			}
		}

		public string ConnectionId{ get{ return _connectionId; } }


		public RtmpContext Context
		{
			get{ return _context; }
		}

		public bool IsChannelUsed(byte channelId) 
		{
			return _channels.Contains(channelId) && _channels[channelId] != null;
		}

		public RtmpChannel GetChannel(byte channelId) 
		{
			if(!IsChannelUsed(channelId))
				_channels[channelId] = new RtmpChannel(this, channelId);
			return _channels[channelId] as RtmpChannel;
		}

		public void CloseChannel(byte channelId) 
		{
			_channels[channelId] = null;
		}

		public int InvokeId
		{ 
			get{ return _invokeId; } 
			set{ _invokeId = value; } 
		}

		public IClientStream GetStreamByChannelId(byte channelId) 
		{
			if(channelId < 4) 
				return null;
			int id = (int)Math.Floor((double)((channelId - 4) / 5));
			return _streams[id];
		}

		public IClientStream GetStreamById(int id) 
		{
			if (id <= 0 || id > MAX_STREAMS - 1) 
				return null;
			return _streams[id - 1];
		}

		public IPendingServiceCall GetPendingCall(int invokeId)
		{
			IPendingServiceCall result;
			lock(_objLock)
			{
				result = _pendingCalls[invokeId] as IPendingServiceCall;
				if(result != null)
				{
					_pendingCalls.Remove(invokeId);
				}
			}
			return result;
		}


		public void Write(RtmpPacket packet)
		{
			//encode
			_rtmpService.Encode(this, packet);
		}
	}
}
