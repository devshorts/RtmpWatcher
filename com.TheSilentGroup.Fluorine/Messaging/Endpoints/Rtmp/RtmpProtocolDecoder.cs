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

using com.TheSilentGroup.Fluorine.Exceptions;
using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Event;
using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api.Service;
using com.TheSilentGroup.Fluorine.SystemHelpers.IO;

namespace com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	sealed class RtmpProtocolDecoder
	{
		public const int HandshakeSize = 1536;

		public RtmpProtocolDecoder()
		{
		}

		public static byte DecodeChannelId(byte headerByte) 
		{
			return (byte)(headerByte & 0x3f);
		}

		public static byte DecodeHeaderSize(byte headerByte) 
		{
			int headerSize = (headerByte >= 0) ? headerByte : headerByte + 256;
			byte size = (byte)(headerSize >> 6);
			return size;
		}

		public static int GetHeaderLength(byte headerSize) 
		{
			switch((HeaderType)headerSize) 
			{
				case HeaderType.HeaderNew:
					return 12;
				case HeaderType.HeaderSameSource:
					return 8;
				case HeaderType.HeaderTimerChange:
					return 4;
				case HeaderType.HeaderContinue:
					return 1;
				default:
					return -1;
			}
		}

		public static ArrayList DecodeBuffer(RtmpContext context, ByteBuffer stream)
		{
			// >> HEADER[1] + CLIENT_HANDSHAKE[1536] 
			// << HEADER[1] + SERVER_HANDSHAKE[1536] + CLIENT_HANDSHAKE[1536];
			// >> SERVER_HANDSHAKE[1536] + AMF[n]

			ArrayList result = new ArrayList();
			try 
			{
				while (true) 
				{
					long remaining = stream.Remaining;
					if(context.CanStartDecoding(remaining)) 
						context.StartDecoding();
					else 
						break;
				
					object decodedObject = Decode(context, stream);
					if(context.HasDecodedObject) 
						result.Add(decodedObject);
					else if(context.CanContinueDecoding) 
						continue;
					else 
						break;

					if(!stream.HasRemaining) 
						break;
				}
			}
			catch
			{
				throw;
			}
			finally
			{
				stream.Compact();
			}
			return result;
		}

		public static object Decode(RtmpContext context, ByteBuffer stream)
		{
			long start = stream.Position;
			try 
			{
				switch(context.State)
				{					
					case RtmpState.Connected:
						return DecodePacket(context, stream);
					case RtmpState.Error:
						// attempt to correct error 
					case RtmpState.Connect:
					case RtmpState.Handshake:
						return DecodeHandshake(context, stream);
					default:
						return null;
				}
			} 
			catch(Exception ex) 
			{
				throw new ProtocolException("Error during decoding", ex);
			}
		}

		public static ByteBuffer DecodeHandshake(RtmpContext context, ByteBuffer stream) 
		{
			long remaining = stream.Remaining;
			if(context.Mode == RtmpMode.Server)
			{
				if(context.State == RtmpState.Connect)
				{
					if(remaining < HandshakeSize + 1) 
					{
						//("Handshake init too small, buffering. remaining: " + remaining);
						context.SetBufferDecoding(HandshakeSize + 1);
						return null;
					}
					else 
					{
						//This is not a RTMP packet but a single byte (0x3) followed by two 
						//1536 byte chunks (so a total of 3072 raw bytes). 
						//The second chunk of bytes is the original client request bytes sent 
						//in handshake request. The first chunk can be anything. 
						//Use null bytes it doesnt seem to matter. 
						ByteBuffer hs = ByteBuffer.Allocate(2*HandshakeSize+1);
						hs.Put(0x03);
						hs.Fill((byte)0x00, HandshakeSize);
						stream.Get();// skip the header byte
						ByteBuffer.Put(hs, stream, HandshakeSize);
						hs.Flip();
						/*
						byte header = (byte)stream.ReadByte();
						byte[] buffer = new byte[2*HandshakeSize+1];
						MemoryStreamEx outStream = new MemoryStreamEx(buffer);
						outStream.WriteByte(0x03);//Header
						outStream.Fill(0, HandshakeSize);
						outStream.Append(stream);
						*/
						context.State = RtmpState.Handshake;
						return hs;
					}
				}
				if(context.State == RtmpState.Handshake)
				{
					if(remaining < HandshakeSize)
					{
						//"Handshake reply too small, buffering. remaining: "+ remaining);
						context.SetBufferDecoding(HandshakeSize);
						return null;
					}				 
					else 
					{
						stream.Skip(HandshakeSize);
						context.State = RtmpState.Connected;
						context.ContinueDecoding();
						return null;
					}
				}
			}
			else
			{
				//Client mode
				if(context.State == RtmpState.Connect)
				{
					int size = (2 * HandshakeSize) + 1;
					if(remaining < size) 
					{
						//"Handshake init too small, buffering. remaining: "+ remaining);
						context.SetBufferDecoding(size);
						return null;
					}
					else
					{
						ByteBuffer hs = ByteBuffer.Allocate(size);
						ByteBuffer.Put(hs, stream, size);
						hs.Flip();
						/*
						byte[] buffer = new byte[size];
						buffer[0] = 0x03;
						Array.Copy(stream.GetBuffer(), 0, buffer, 0, size);
						MemoryStreamEx outStream = new MemoryStreamEx(buffer);
						*/
						context.State = RtmpState.Connected;
						return hs;
					}
				}
			}
			return null;
		}

		public static RtmpPacket DecodePacket(RtmpContext context, ByteBuffer stream)
		{
			int remaining = stream.Remaining;
			// We need at least one byte
			if(remaining < 1) 
			{
				context.SetBufferDecoding(1);
				return null;
			}
			int position = (int)stream.Position;
			byte headerByte = stream.Get();
			byte channelId = DecodeChannelId(headerByte);
			if (channelId < 0) 
				throw new ProtocolException("Bad channel id: " + channelId);
			byte headerSize = DecodeHeaderSize(headerByte);
			int headerLength = GetHeaderLength(headerSize);

			if(headerLength > remaining) 
			{
				//log.debug("Header too small, buffering. remaining: " + remaining);
				stream.Position = position;
				context.SetBufferDecoding(headerLength);
				return null;
			}
			// Move the position back to the start
			stream.Position = position;

			RtmpHeader header = DecodeHeader(context, context.GetLastReadHeader(channelId), stream);

			if (header == null) 
				throw new ProtocolException("Header is null, check for error");

			// Save the header
			context.SetLastReadHeader(channelId, header);
			// Check to see if this is a new packets or continue decoding an existing one.
			RtmpPacket packet = context.GetLastReadPacket(channelId);
			if(packet == null) 
			{
				packet = new RtmpPacket(header);
				context.SetLastReadPacket(channelId, packet);
			}

			ByteBuffer buf = packet.Data;
			int addSize = (header.Timer == 0xffffff ? 4 : 0);
			int readRemaining = header.Size + addSize - (int)buf.Position;
			int chunkSize = context.GetReadChunkSize();
			int readAmount = (readRemaining > chunkSize) ? chunkSize : readRemaining;
			if(stream.Remaining < readAmount) 
			{
				//"Chunk too small, buffering (" + in.remaining() + ","+ readAmount);
				
				//Skip the position back to the start
				stream.Position = position;
				context.SetBufferDecoding(headerSize + readAmount);
				return null;
			}
			
			//http://osflash.org/pipermail/free_osflash.org/2005-September/000261.html
			//http://www.acmewebworks.com/Downloads/openCS/091305-initialMeeting.txt
			ByteBuffer.Put(buf, stream, readAmount);
			if(buf.Position < header.Size + addSize) 
			{
				context.ContinueDecoding();
				return null;
			}

			buf.Flip();
			RtmpMessage message = DecodeMessage(context, header, buf);
			packet.Message = message;

			
			if (message is ChunkSize) 
			{
				ChunkSize chunkSizeMsg = message as ChunkSize;
				context.SetReadChunkSize( chunkSizeMsg.Size );
			}
			context.SetLastReadPacket(channelId, null);
			return packet;
		}

		public static RtmpHeader DecodeHeader(RtmpContext context, RtmpHeader lastHeader, ByteBuffer stream)
		{
			byte headerByte = stream.Get();
			byte channelId = DecodeChannelId(headerByte);
			byte headerSize = DecodeHeaderSize(headerByte);
			int headerLength = GetHeaderLength(headerSize);
			RtmpHeader header = new RtmpHeader();
			header.ChannelId = channelId;
			header.IsTimerRelative = (HeaderType)headerSize != HeaderType.HeaderNew;

			switch((HeaderType)headerSize)
			{
				case HeaderType.HeaderNew:
					header.Timer = stream.ReadUnsignedMediumInt();
					header.Size = stream.ReadMediumInt();
					header.DataType = (DataType)stream.Get();
					header.StreamId = stream.ReadReverseInt();
					break;
				case HeaderType.HeaderSameSource:
					header.Timer = stream.ReadUnsignedMediumInt();
					header.Size = stream.ReadMediumInt();
					header.DataType = (DataType)stream.Get();
					header.StreamId = lastHeader.StreamId;
					break;
				case HeaderType.HeaderTimerChange:
					header.Timer = stream.ReadUnsignedMediumInt();
					header.Size = lastHeader.Size;
					header.DataType = lastHeader.DataType;
					header.StreamId = lastHeader.StreamId;
					break;
				case HeaderType.HeaderContinue:
					header.Timer = lastHeader.Timer;
					header.Size = lastHeader.Size;
					header.DataType = lastHeader.DataType;
					header.StreamId = lastHeader.StreamId;
					break;
				default:
					return null;
			}
			return header;
		}

		public static RtmpMessage DecodeMessage(RtmpContext context, RtmpHeader header, ByteBuffer stream)
		{
			RtmpMessage message = null;
			if(header.Timer == 0xffffff) 
			{
				// Skip first four bytes
				byte[] tmp = new byte[4];
				stream.Read(tmp, 0, 4);
				//int unknown = stream.ReadInt32();
			}

			switch(header.DataType) 
			{
				case DataType.TypeChunkSize:
					//message = decodeChunkSize(in);
					break;
				case DataType.TypeInvoke:
					message = DecodeInvoke(stream);
					break;
				case DataType.TypeNotify:
					message = DecodeNotify(stream, header);
					break;
				case DataType.TypePing:
					//message = decodePing(in);
					break;
				case DataType.TypeBytesRead:
					//message = decodeBytesRead(in);
					break;
				case DataType.TypeAudioData:
					//message = decodeAudioData(in);
					break;
				case DataType.TypeVideoData:
					//message = decodeVideoData(in);
					break;
				case DataType.TypeSharedObject:
					//message = decodeSharedObject(in);
					break;
				case DataType.TypeServerBandwidth:
					//message = decodeServerBW(in);
					break;
				case DataType.TypeClientBandwidth:
					//message = decodeClientBW(in);
					break;
				default:
					//message = decodeUnknown(header.getDataType(), in);
					break;
			}
			if( message != null )
				message.Header = header;
			//message.setTimestamp(header.getTimer());
			return message;
		}

		static Invoke DecodeInvoke(ByteBuffer stream)
		{
			return DecodeNotifyOrInvoke(new Invoke(), stream, null) as Invoke;
		}

		static Notify DecodeNotify(ByteBuffer stream, RtmpHeader header)
		{
			return DecodeNotifyOrInvoke(new Notify(), stream, header);
		}


		static Notify DecodeNotifyOrInvoke(Notify notify, ByteBuffer stream, RtmpHeader header)
		{
			long start = stream.Position;
			RtmpReader reader = new RtmpReader(stream);
			string action = reader.ReadData() as string;

			if(!(notify is Invoke))
			{
				//Don't decode "NetStream.send" requests
				stream.Position = start;
				//notify.setData(in.asReadOnlyBuffer());
				return notify;
			}

			if(header == null || header.StreamId == 0) 
			{
				double invokeId = (double)reader.ReadData();
				notify.InvokeId = (int)invokeId;
			}

			object[] parameters = new object[]{};
			if(stream.HasRemaining)
			{
				ArrayList paramList = new ArrayList();
				object obj = reader.ReadData();

				if (obj is ASObject)
				{
					// for connect we get a map
					notify.ConnectionParameters = obj as Hashtable;
				} 
				else if (obj != null) 
				{
					paramList.Add(obj);
				}

				while(stream.HasRemaining)
				{
					paramList.Add(reader.ReadData());
				}
				parameters = paramList.ToArray();
			}

			int dotIndex = action.LastIndexOf(".");
			string serviceName = (dotIndex == -1) ? null : action.Substring(0, dotIndex);
			string serviceMethod = (dotIndex == -1) ? action : action.Substring(dotIndex + 1, action.Length);

			if (notify is Invoke)
			{
				PendingCall call = new PendingCall(serviceName, serviceMethod, parameters);
				(notify as Invoke).ServiceCall = call;
			} 
			else 
			{
				ServiceCall call = new ServiceCall(serviceName, serviceMethod, parameters);
				notify.ServiceCall = call;
			}
			return notify;
		}
	}
}
