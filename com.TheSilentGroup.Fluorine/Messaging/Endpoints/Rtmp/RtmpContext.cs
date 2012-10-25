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
	public enum DecoderState
	{
		Ok = 0,
		Continue = 1,
		Buffer = 2
	}
	/// <summary>
	/// Summary description for RtmpContext.
	/// </summary>
	sealed class RtmpContext
	{
		long _decoderBufferAmount = 0;
		DecoderState _decoderState = DecoderState.Ok;

		RtmpMode _mode;
		RtmpState _state;

		byte _lastReadChannel = 0x00;
		byte _lastWriteChannel = 0x00;
		RtmpHeader[] _readHeaders = new RtmpHeader[128];
		RtmpHeader[] _writeHeaders = new RtmpHeader[128];
		RtmpPacket[] _readPackets = new RtmpPacket[128];
		RtmpPacket[] _writePackets = new RtmpPacket[128];

		const int DefaultChunkSize = 128;
		int _readChunkSize = DefaultChunkSize;
		int _writeChunkSize = DefaultChunkSize;

		public RtmpContext(RtmpMode mode)
		{
			_mode = mode;
		}

		public RtmpState State
		{
			get{ return _state; }
			set
			{
				_state = value;
				if(_state == RtmpState.Disconnected) 
				{
					// Free temporary packets
					FreePackets(_readPackets);
					FreePackets(_writePackets);
				}
			}
		}

		public RtmpMode Mode
		{
			get{ return _mode; }
		}

		private void FreePackets(RtmpPacket[] packets) 
		{
			foreach(RtmpPacket packet in packets) 
			{
				if (packet != null && packet.Data != null) 
				{
					packet.Data = null;
				}
			}
		}

		public void SetLastReadHeader(byte channelId, RtmpHeader header) 
		{
			_lastReadChannel = channelId;
			_readHeaders[channelId] = header;
		}

		public RtmpHeader GetLastReadHeader(byte channelId) 
		{
			return _readHeaders[channelId] as RtmpHeader;
		}

		public void SetLastWriteHeader(byte channelId, RtmpHeader header) 
		{
			_lastWriteChannel = channelId;
			_writeHeaders[channelId] = header;
		}

		public RtmpHeader GetLastWriteHeader(byte channelId) 
		{
			return _writeHeaders[channelId] as RtmpHeader;
		}

		public void SetLastReadPacket(byte channelId, RtmpPacket packet) 
		{
			RtmpPacket prevPacket = _readPackets[channelId];
			if (prevPacket != null && prevPacket.Data != null) 
			{
				prevPacket.Data = null;
			}
			_readPackets[channelId] = packet;
		}

		public RtmpPacket GetLastReadPacket(byte channelId) 
		{
			return _readPackets[channelId] as RtmpPacket;
		}

		public void SetLastWritePacket(byte channelId, RtmpPacket packet) 
		{
			RtmpPacket prevPacket = _writePackets[channelId];
			if (prevPacket != null && prevPacket.Data != null) 
			{
				prevPacket.Data = null;
			}
			_writePackets[channelId] = packet;
		}

		public RtmpPacket GetLastWritePacket(byte channelId) 
		{
			return _writePackets[channelId] as RtmpPacket;
		}

		public byte GetLastReadChannel() 
		{
			return _lastReadChannel;
		}

		public byte GetLastWriteChannel() 
		{
			return _lastWriteChannel;
		}

		public int GetReadChunkSize() 
		{
			return _readChunkSize;
		}

		public void SetReadChunkSize(int readChunkSize) 
		{
			_readChunkSize = readChunkSize;
		}

		public int GetWriteChunkSize() 
		{
			return _writeChunkSize;
		}

		public void SetWriteChunkSize(int writeChunkSize) 
		{
			_writeChunkSize = writeChunkSize;
		}



		public long GetDecoderBufferAmount() 
		{
			return _decoderBufferAmount;
		}

		public void SetBufferDecoding(long amount) 
		{
			_decoderState = DecoderState.Buffer;
			_decoderBufferAmount = amount;
		}

		public void ContinueDecoding() 
		{
			_decoderState = DecoderState.Continue;
		}

		public bool CanStartDecoding(long remaining) 
		{
			if(remaining >= _decoderBufferAmount) 
				return true;
			else 
				return false;
		}

		public void StartDecoding() 
		{
			_decoderState = DecoderState.Ok;
			_decoderBufferAmount = 0;
		}

		public bool HasDecodedObject
		{
			get{ return _decoderState == DecoderState.Ok; }
		}

		public bool CanContinueDecoding
		{
			get{ return _decoderState != DecoderState.Buffer; }
		}

	}
}
