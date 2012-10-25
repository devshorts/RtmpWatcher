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
using System.IO;

// Import log4net classes.
using com.TheSilentGroup.Fluorine.SystemHelpers.IO;
using log4net;
using log4net.Config;

using com.TheSilentGroup.Fluorine.Exceptions;
using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Event;
using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api;
using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api.Service;

namespace com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	sealed class RtmpProtocolEncoder
	{
		static private ILog _log;

		static RtmpProtocolEncoder()
		{
			try
			{
				_log = LogManager.GetLogger(typeof(RtmpProtocolEncoder));
			}
			catch{}
		}

		public static ByteBuffer Encode(RtmpContext context, object message)
		{		
			try
			{
				if (message is ByteBuffer) 
					return (ByteBuffer) message;
				else 
					return EncodePacket(context, message as RtmpPacket);
			}			 
			catch(Exception ex) 
			{
				if( _log != null )
					_log.Fatal("Error encoding object. ", ex);
			}
			return null;
		}

		public static ByteBuffer EncodePacket(RtmpContext context, RtmpPacket packet) 
		{
			RtmpHeader header = packet.Header;
			byte channelId = header.ChannelId;
			RtmpMessage message = packet.Message;
			//MemoryStreamEx data;
			ByteBuffer data;

			if (message is ChunkSize)
			{
				ChunkSize chunkSizeMsg = (ChunkSize)message;
				context.SetWriteChunkSize(chunkSizeMsg.Size);
			}

			try 
			{
				data = EncodeMessage(header, message);
			} 
			finally 
			{
				message.Release();
			}

			if(data.Position != 0) 
				data.Flip();
			else 
				data.Rewind();
			header.Size = (int)data.Length;

			ByteBuffer headers = EncodeHeader(header, context.GetLastWriteHeader(channelId));
			context.SetLastWriteHeader(channelId, header);
			context.SetLastWritePacket(channelId, packet);

			int chunkSize = context.GetWriteChunkSize();
			int numChunks = (int)Math.Ceiling(header.Size / (float)chunkSize);
			int bufSize = (int)header.Size + headers.Limit + (numChunks - 1 * 1);
			ByteBuffer output = ByteBuffer.Allocate(bufSize);

			headers.Flip();
			output.Put(headers);
			headers.Close();

			if (numChunks == 1) 
			{
				// we can do it with a single copy
				ByteBuffer.Put(output, data, output.Remaining);
			}
			else 
			{
				for(int i = 0; i < numChunks - 1; i++) 
				{
					ByteBuffer.Put(output, data, chunkSize);
					output.Put( EncodeHeaderByte((byte)HeaderType.HeaderContinue, header.ChannelId) );
				}
				ByteBuffer.Put(output, data, output.Remaining);
			}
			data.Close();
			output.Flip();
			return output;
		}

		public static byte EncodeHeaderByte(byte headerSize, byte channelId) 
		{
			return (byte)((headerSize << 6) + channelId);
		}

		public static ByteBuffer EncodeHeader(RtmpHeader header, RtmpHeader lastHeader) 
		{
			HeaderType headerType = HeaderType.HeaderNew;
			if (lastHeader == null || header.StreamId != lastHeader.StreamId || !header.IsTimerRelative ) 
			{
				headerType = HeaderType.HeaderNew;
			} 
			else if (header.Size != lastHeader.Size || header.DataType != lastHeader.DataType ) 
			{
				headerType = HeaderType.HeaderSameSource;
			} 
			else if (header.Timer != lastHeader.Timer) 
			{
				headerType = HeaderType.HeaderTimerChange;
			} 
			else 
			{
				headerType = HeaderType.HeaderContinue;
			}
			ByteBuffer buf = ByteBuffer.Allocate( RtmpHeader.GetHeaderLength(headerType) );
			byte headerByte = EncodeHeaderByte((byte)headerType, header.ChannelId);
			buf.Put(headerByte);
			switch(headerType) 
			{
				case HeaderType.HeaderNew:
					buf.WriteMediumInt(header.Timer);
					buf.WriteMediumInt(header.Size);
					buf.Put((byte)header.DataType);
					buf.WriteReverseInt(header.StreamId);
					break;
				case HeaderType.HeaderSameSource:
					buf.WriteMediumInt(header.Timer);
					buf.WriteMediumInt(header.Size);
					buf.Put((byte)header.DataType);
					break;
				case HeaderType.HeaderTimerChange:
					buf.WriteMediumInt(header.Timer);
					break;
				case HeaderType.HeaderContinue:
					break;
			}
			return buf;
		}

		public static ByteBuffer EncodeMessage(RtmpHeader header, RtmpMessage message) 
		{
			switch(header.DataType) 
			{
				case DataType.TypeChunkSize:
					return EncodeChunkSize(message as ChunkSize);
				case DataType.TypeInvoke:
					return EncodeInvoke(message as Invoke);
				/*
				case DataType.TypeNotify:
					if (((Notify) message).getCall() == null) 
						return EncodeStreamMetadata((Notify) message);
					else 
						return EncodeNotify((Notify) message);
				case DataType.TypePing:
					return EncodePing(message as Ping);
				case DataType.TypeBytesRead:
					return EncodeBytesRead(message as BytesRead);
				case DataType.TypeAudioData:
					return EncodeAudioData(message as AudioData);
				case DataType.TypeVideoData:
					return EncodeVideoData(message as VideoData);
				case DataType.TypeSharedObject:
					return EncodeSharedObject(message as ISharedObjectMessage);
				case DataType.TypeServerBandwidth:
					return EncodeServerBW(message as ServerBW);
				case DataType.TypeClientBandwidth:
					return EncodeClientBW(message as ClientBW);
				*/
				default:
					return null;
			}
		}

		public static ByteBuffer EncodeChunkSize(ChunkSize chunkSize) 
		{
			ByteBuffer output = ByteBuffer.Allocate(4);
			output.WriteInt32(chunkSize.Size);
			return output;
		}

		public static ByteBuffer EncodePing(Ping ping) 
		{
			/*
			int len = 6;
			if (ping.getValue3() != Ping.UNDEFINED) 
			{
				len += 4;
			}
			if (ping.getValue4() != Ping.UNDEFINED) 
			{
				len += 4;
			}
			final ByteBuffer out = ByteBuffer.allocate(len);
			out.putShort(ping.getValue1());
			out.putInt(ping.getValue2());
			if (ping.getValue3() != Ping.UNDEFINED) 
			{
				out.putInt(ping.getValue3());
			}
			if (ping.getValue4() != Ping.UNDEFINED) 
			{
				out.putInt(ping.getValue4());
			}
			*/
			return null;
		}
		public static ByteBuffer EncodeInvoke(Invoke invoke) 
		{
			return EncodeNotifyOrInvoke(invoke);
		}

		static ByteBuffer EncodeNotifyOrInvoke(Notify invoke) 
		{
			//MemoryStreamEx output = new MemoryStreamEx();
			ByteBuffer output = ByteBuffer.Allocate(1024);
			output.AutoExpand = true;
			RtmpWriter writer = new RtmpWriter(output);

			IServiceCall serviceCall = invoke.ServiceCall;
			bool isPending = serviceCall.Status == ServiceCall.STATUS_PENDING;
			if (!isPending) 
			{
				//log.debug("Call has been executed, send result");
				writer.WriteData("_result");
			}
			else
			{
				//log.debug("This is a pending call, send request");
				string action = (serviceCall.ServiceName == null) ? serviceCall.ServiceMethodName : serviceCall.ServiceName + "." + serviceCall.ServiceMethodName;
				writer.WriteData(action);
			}
			if(invoke is Invoke)
			{
				writer.WriteData(invoke.InvokeId);
				writer.WriteData(null);
			}
			if(!isPending && (invoke is Invoke)) 
			{
				IPendingServiceCall pendingCall = (IPendingServiceCall)serviceCall;
				//log.debug("Writing result: " + pendingCall.getResult());
				writer.WriteData(pendingCall.Result);
			}
			else
			{
				//log.debug("Writing params");
				object[] args = invoke.ServiceCall.Arguments;
				if (args != null) 
				{
					foreach(object element in args)
					{
						writer.WriteData(element);
					}
				}
			}
			return output;
		}
	}
}
