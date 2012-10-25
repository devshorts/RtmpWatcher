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
using System.Net;

using com.TheSilentGroup.Fluorine.Messaging.Util;
using com.TheSilentGroup.Fluorine.Messaging;
using com.TheSilentGroup.Fluorine.Messaging.Config;
using com.TheSilentGroup.Fluorine.Messaging.Messages;
using com.TheSilentGroup.Fluorine.Messaging.Endpoints;
using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp;
using com.TheSilentGroup.Fluorine.SystemHelpers.Net;

//namespace com.TheSilentGroup.Fluorine.Messaging.Endpoints
namespace flex.messaging.endpoints
{
	/// <summary>
	/// Summary description for RTMPEndpoint.
	/// </summary>
	internal class RTMPEndpoint : EndpointBase
	{
		RtmpService		_rtmpService;
		SocketServer	_socketServer;

		public RTMPEndpoint(MessageBroker messageBroker, ChannelSettings channelSettings):base(messageBroker, channelSettings)
		{
			_socketServer = null;
		}

		internal SocketServer SocketServer{ get{ return _socketServer; } }

		public override void Start()
		{
			RtmpServer rtmpServer = new RtmpServer();
			rtmpServer.AddMapping(null, null, "global");
			
			GlobalScope globalScope = new GlobalScope(rtmpServer);
			globalScope.Name = "global";

			ClientRegistry clientRegistry = new ClientRegistry();
			ScopeResolver scopeResolver = new ScopeResolver(globalScope);
			Context context = new Context("/", clientRegistry, scopeResolver, null);
			globalScope.Context = context;
			globalScope.Handler = new CoreHandler();
			globalScope.Register();

			_rtmpService = new RtmpService(this, rtmpServer);
			_socketServer = new SocketServer(_rtmpService);

			UriBase uri = _channelSettings.GetUri();
			int port = Convert.ToInt32(uri.Port);
			_socketServer.AddListener(new IPEndPoint(IPAddress.Any, port));
			_socketServer.Start();
			_socketServer.OnError +=new ErrorHandler(OnError);
		}

		public override void Stop()
		{
			if( _socketServer != null ) 
			{
				_socketServer.Stop();
				_socketServer.OnError -=new ErrorHandler(OnError);
				_socketServer.Dispose();
				_socketServer = null;
				_rtmpService = null;
			}
		}

		public override void Push(IMessage message, MessageClient messageclient)
		{
		}

		private void OnError(object sender, ServerErrorEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine(e.Exception.Message);
		}
	}
}
