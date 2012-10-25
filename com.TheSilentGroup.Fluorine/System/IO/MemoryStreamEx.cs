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

namespace com.TheSilentGroup.Fluorine.SystemHelpers.IO
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	sealed class MemoryStreamEx : MemoryStream
	{
		public MemoryStreamEx() : base()
		{
		}

		public MemoryStreamEx(byte[] buffer) : base()
		{
			for(int i = 0; i < buffer.Length; i++)
				this.WriteByte(buffer[i]);
			Reset();
		}

		public void Reset()
		{
			this.Seek(0, SeekOrigin.Begin);
		}

		public long Remaining
		{
			get{ return this.Length - this.Position; }
		}

		public bool HasRemaining
		{
			get{ return this.Remaining > 0; }
		}

		public void Skip(int offset)
		{
			this.Seek(offset, SeekOrigin.Current);
		}

		public void Fill(byte value, long count)
		{
			for(long i = 0; i < count; i++)
				this.WriteByte(value);
		}

		public void Append(Stream stream)
		{
			while( stream.Position < stream.Length )
			{
				this.WriteByte( (byte)stream.ReadByte() );
			}
		}

		/*
		public void Insert(Stream stream)
		{
			long position = this.Position;
			while( stream.Position < stream.Length )
			{
				this.WriteByte( (byte)stream.ReadByte() );
			}
			this.Position = position;
		}
		*/

		public void Append(Stream stream, long count)
		{
			while( stream.Position < stream.Length && count > 0 )
			{
				this.WriteByte( (byte)stream.ReadByte() );
				count--;
			}
		}

		public void WriteMediumInt(int value) 
		{
			byte[] bytes = new byte[3];
			bytes[0] = (byte) (0xFF & (value >> 16));
			bytes[1] = (byte) (0xFF & (value >> 8));
			bytes[2] = (byte) (0xFF & (value >> 0));
			this.Write(bytes, 0, bytes.Length);
		}

		public void WriteReverseInt(int value) 
		{
			byte[] bytes = new byte[4];
			bytes[3] = (byte) (0xFF & (value >> 24));
			bytes[2] = (byte) (0xFF & (value >> 16));
			bytes[1] = (byte) (0xFF & (value >> 8));
			bytes[0] = (byte) (0xFF & value);
			this.Write(bytes, 0, bytes.Length);
		}

		public void WriteInt32(int number)
		{
			byte[] bytes = BitConverter.GetBytes(number);
			this.Write(bytes, 0, bytes.Length);
		}

	}
}
