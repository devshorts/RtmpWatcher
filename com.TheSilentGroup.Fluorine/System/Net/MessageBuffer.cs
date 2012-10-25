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

namespace com.TheSilentGroup.Fluorine.SystemHelpers.Net
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	sealed class MessageBuffer
	{
		private byte[]	_rawBuffer;
		private byte[]	_packetBuffer;
		private int		_packetOffSet;

		public MessageBuffer(int bufferSize)
		{
			if(bufferSize > 0)
			{
				_packetBuffer = new byte[bufferSize];
			}
			_packetOffSet = 0;
			_rawBuffer = null;

		}

		public MessageBuffer(ref byte[] rawBuffer, ref byte[] packetBuffer)
		{
			_rawBuffer = rawBuffer;
			_packetBuffer = packetBuffer;
			_packetOffSet = 0;
		}

		public byte[] RawBuffer
		{
			get { return _rawBuffer; }
			set { _rawBuffer = value; }
		}

		public byte[] PacketBuffer
		{
			get { return _packetBuffer; }
			set { _packetBuffer = value; }
		}

		public int PacketOffSet
		{
			get { return _packetOffSet; }
			set { _packetOffSet = value; }
		}

		public int PacketLength
		{
			get { return _packetBuffer.Length; }
		}

		public int PacketRemaining
		{
			get { return _packetBuffer.Length - _packetOffSet; }
		}

		public byte[] GetRawBuffer(int messageLength, int headerSize)
		{
			byte[] result = null;

			result = new byte[messageLength - headerSize];
			Array.Copy(_packetBuffer, headerSize, result, 0, result.Length);

			//Adjust Packet Buffer
			byte[] packetBuffer = new byte[_packetBuffer.Length - messageLength];
			Array.Copy(_packetBuffer, messageLength, packetBuffer, 0, packetBuffer.Length);

			_packetBuffer = packetBuffer;
			_packetOffSet = _packetOffSet - messageLength;

			return result;
		}

		public static MessageBuffer GetPacketMessage(ServerSocketConnection connection, ref byte[] buffer)
		{
			return new MessageBuffer(ref buffer, ref buffer);
		}
	}
}
