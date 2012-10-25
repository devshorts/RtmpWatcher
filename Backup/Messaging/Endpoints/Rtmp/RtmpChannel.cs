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

using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api.Stream;

namespace com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp
{
	/// <summary>
	/// Summary description for RtmpChannel.
	/// </summary>
	internal class RtmpChannel
	{
		RtmpConnection	_connection;
		byte			_channelId;

		public RtmpChannel(RtmpConnection connection, byte channelId)
		{
			_connection = connection;
			_channelId = channelId;
		}

		public byte ChannelId
		{
			get{ return _channelId; }
		}

		public void Close()
		{
			_connection.CloseChannel(_channelId);
		}

		public void Write(RtmpMessage message)
		{
			IClientStream stream = _connection.GetStreamByChannelId(_channelId);
			if(_channelId > 3 && stream == null) 
			{
				//Stream doesn't exist any longer, discarding message
				return;
			}
			int streamId = (stream == null) ? 0 : stream.StreamId;
			Write(message, streamId);
		}

		private void Write(RtmpMessage message, int streamId) 
		{
			RtmpHeader header = new RtmpHeader();
			RtmpPacket packet = new RtmpPacket(header, message);

			header.ChannelId = _channelId;
			header.Timer = message.Timestamp;
			header.StreamId = streamId;
			header.DataType = message.DataType;
			if(message.Header != null) 
			{
				header.IsTimerRelative =  message.Header.IsTimerRelative;
			}
			_connection.Write(packet);
		}
	}
}
