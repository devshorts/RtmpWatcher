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
	/// Summary description for ByteBuffer.
	/// http://java.sun.com/j2se/1.5.0/docs/api/java/nio/ByteBuffer.html
	/// </summary>
	class ByteBuffer : Stream
	{
		private MemoryStream _stream;
		private bool _autoExpand;

		public ByteBuffer(MemoryStream stream)
		{
			_stream = stream;
		}

		/// <summary>
		/// Allocates a new byte buffer.
		/// The new buffer's position will be zero, its limit will be its capacity, 
		/// and its mark will be undefined. 
		/// It will have a backing array, and its array offset will be zero. 
		/// </summary>
		/// <param name="capacity"></param>
		/// <returns></returns>
		public static ByteBuffer Allocate(int capacity)
		{
			MemoryStream ms = new MemoryStream(capacity);
			ByteBuffer buffer = new ByteBuffer(ms);
			buffer.Limit = capacity;
			return buffer;
		}
		/// <summary>
		/// Wraps a byte array into a buffer.
		/// The new buffer will be backed by the given byte array; that is, modifications 
		/// to the buffer will cause the array to be modified and vice versa. 
		/// The new buffer's capacity will be array.length, its position will be offset, 
		/// its limit will be offset + length, and its mark will be undefined.
		/// </summary>
		/// <param name="array"></param>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public static ByteBuffer Wrap(byte[] array, int offset, int length)
		{
			MemoryStream ms = new MemoryStream(array, offset, length, true, true);
			ms.Capacity = array.Length;
			ms.SetLength(offset + length);
			ms.Position = offset;
			return new ByteBuffer(ms);
		}
		/// <summary>
		/// Wraps a byte array into a buffer. 
		/// The new buffer will be backed by the given byte array; that is, modifications 
		/// to the buffer will cause the array to be modified and vice versa. 
		/// The new buffer's capacity and limit will be array.length, its position will be zero,
		/// and its mark will be undefined.
		/// </summary>
		/// <param name="array"></param>
		/// <returns></returns>
		public static ByteBuffer Wrap(byte[] array)
		{
			return Wrap(array, 0, array.Length);
		}
		/// <summary>
		/// Turns on or off autoExpand
		/// </summary>
		public bool AutoExpand
		{
			get{ return _autoExpand; }
			set{ _autoExpand = value; }
		}
		/// <summary>
		/// Returns this buffer's capacity.
		/// </summary>
		public int Capacity
		{
			get{ return (int)_stream.Capacity; }
		}
		/// <summary>
		/// Returns this buffer's limit. 
		/// </summary>
		public int Limit
		{
			get{ return (int)_stream.Length; }
			set{ _stream.SetLength(value); }
		}
		/// <summary>
		/// Returns the number of elements between the current position and the limit. 
		/// </summary>
		public int Remaining
		{
			get{ return this.Limit - (int)this.Position; }
		}
		/// <summary>
		/// Tells whether there are any elements between the current position and the limit. 
		/// </summary>
		public bool HasRemaining
		{
			get{ return this.Remaining > 0; }
		}
		/// <summary>
		/// Flips this buffer. The limit is set to the current position and then 
		/// the position is set to zero. If the mark is defined then it is discarded.
		/// </summary>
		public void Flip()
		{
			this.Limit = (int)this.Position;
			this.Position = 0;
		}
		/// <summary>
		/// Rewinds this buffer. The position is set to zero and the mark is discarded.
		/// </summary>
		public void Rewind()
		{
			this.Position = 0;
		}
		/// <summary>
		/// Writes the given byte into this buffer at the current position, and then increments the position.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public void Put(byte value)
		{
			this.WriteByte(value);
		}
		/// <summary>
		/// Relative bulk put method.
		/// 
		/// This method transfers bytes into this buffer from the given source array. 
		/// If there are more bytes to be copied from the array than remain in this buffer, 
		/// that is, if length > remaining(), then no bytes are transferred and a 
		/// BufferOverflowException is thrown. 
		/// 
		/// Otherwise, this method copies length bytes from the given array into this buffer, 
		/// starting at the given offset in the array and at the current position of this buffer. 
		/// The position of this buffer is then incremented by length. 
		/// </summary>
		/// <param name="src"></param>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		public void Put(byte[] src, int offset, int length)
		{
			for(int i = offset; i < offset + length; i++)
				Put(src[i]); 
		}
		/// <summary>
		/// This method transfers the entire content of the given source byte array into this buffer. 
		/// </summary>
		/// <param name="src"></param>
		public void Put(byte[] src)
		{
			Put(src, 0, src.Length);
		}
		/// <summary>
		/// This method transfers the bytes remaining in the given source buffer into this buffer. 
		/// If there are more bytes remaining in the source buffer than in this buffer, 
		/// that is, if src.remaining() > remaining(), then no bytes are transferred 
		/// and a BufferOverflowException is thrown. 
		/// 
		/// Otherwise, this method copies n = src.remaining() bytes from the given buffer into this buffer, 
		/// starting at each buffer's current position. The positions of both buffers are then 
		/// incremented by n. 
		/// </summary>
		/// <param name="src"></param>
		public void Put(ByteBuffer src)
		{
			while(src.HasRemaining)
				Put(src.Get()); 
		}

		public void Put(ByteBuffer src, int count)
		{
			for(int i=0; i < count; i++)
			{
				Put(src.Get());
			}
		}

		/// <summary>
		/// Absolute put method.
		/// Writes the given byte into this buffer at the given index. 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="value"></param>
		public void Put(int index, byte value)
		{
			_stream.GetBuffer()[index] = value;
		}
		/// <summary>
		/// Relative get method. Reads the byte at this buffer's current position, and then increments the position.
		/// </summary>
		/// <returns></returns>
		public byte Get()
		{
			return (byte)this.ReadByte();
		}
		/// <summary>
		/// Absolute get method. Reads the byte at the given index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public byte Get(int index)
		{
			return _stream.GetBuffer()[index];
		}

		public byte[] ToArray()
		{
			return _stream.ToArray();
		}
		/// <summary>
		/// Compacts this buffer
		/// 
		/// The bytes between the buffer's current position and its limit, if any, 
		/// are copied to the beginning of the buffer. That is, the byte at 
		/// index p = position() is copied to index zero, the byte at index p + 1 is copied 
		/// to index one, and so forth until the byte at index limit() - 1 is copied 
		/// to index n = limit() - 1 - p. 
		/// The buffer's position is then set to n+1 and its limit is set to its capacity. 
		/// The mark, if defined, is discarded. 
		/// The buffer's position is set to the number of bytes copied, rather than to zero, 
		/// so that an invocation of this method can be followed immediately by an invocation of 
		/// another relative put method. 
		/// </summary>
		public void Compact()
		{
			for(int i = (int)this.Position; i < this.Limit; i++)
			{
				byte value = this.Get(i);
				this.Put(i - (int)this.Position, value);
			}
			this.Position = this.Limit - this.Position;
			this.Limit = this.Capacity;
		}

		/// <summary>
		/// Forwards the position of this buffer as the specified size bytes.
		/// </summary>
		/// <param name="size"></param>
		public void Skip(int size)
		{
			this.Position += size;
		}

		public void Fill(byte value, int count)
		{
			for(int i = 0; i < count; i++)
				this.Put(value);
		}

		#region Stream

		public override bool CanRead
		{
			get
			{
				return _stream.CanRead;
			}
		}

		public override bool CanSeek
		{
			get
			{
				return _stream.CanSeek;
			}
		}

		public override bool CanWrite
		{
			get
			{
				return _stream.CanWrite;
			}
		}

		public override void Close()
		{
			_stream.Close ();
		}

		public override void Flush()
		{
			_stream.Flush();
		}

		public override long Length
		{
			get
			{
				return _stream.Length;
			}
		}

		public override long Position
		{
			get
			{
				return _stream.Position;
			}
			set
			{
				_stream.Position = value;
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return _stream.Read(buffer, offset, count);
		}

		public override int ReadByte()
		{
			return _stream.ReadByte();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return _stream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			_stream.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_stream.Write(buffer, offset, count);
		}

		public override void WriteByte(byte value)
		{
			_stream.WriteByte (value);
		}

		#endregion Stream

		/// <summary>
		/// Reads count bytes from the current stream into a byte array and advances the current position by count bytes. 
		/// </summary>
		/// <param name="count"></param>
		/// <returns></returns>
		public byte[] ReadBytes( int count )
		{
			byte[] bytes = new byte[count];
			for(int i = 0; i < count; i++)
			{
				bytes[i] = (byte)this.ReadByte();
			}
			return bytes;
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

		public int ReadUnsignedMediumInt()
		{
			byte[] bytes = this.ReadBytes(3);
			int val = 0;
			// Fix unsigned values
			if(bytes[0] < 0) 
			{
				val += ((bytes[0] + 256) << 16);
			} 
			else 
			{
				val += (bytes[0] << 16);
			}
			if (bytes[1] < 0) 
			{
				val += ((bytes[1] + 256) << 8);
			} 
			else 
			{
				val += (bytes[1] << 8);
			}
			if (bytes[2] < 0) 
			{
				val += bytes[2] + 256;
			} 
			else 
			{
				val += bytes[2];
			}
			return val;
		}

		public int ReadMediumInt()
		{
			byte[] bytes = this.ReadBytes(3);
			// Fix unsigned values
			int val = 0;
			if (bytes[0] < 0) 
			{
				val += ((bytes[0] + 256) << 16);
			} 
			else 
			{
				val += (bytes[0] << 16);
			}
			if (bytes[1] < 0) 
			{
				val += ((bytes[1] + 256) << 8);
			} 
			else 
			{
				val += (bytes[1] << 8);
			}
			if (bytes[2] < 0) 
			{
				val += bytes[2] + 256;
			} 
			else 
			{
				val += bytes[2];
			}
			return val;
		}

		public int ReadReverseInt()
		{
			byte[] bytes = this.ReadBytes(4);
			int val = 0;
			val += bytes[3] << 24;
			val += bytes[2] << 16;
			val += bytes[1] << 8;
			val += bytes[0];
			return val;
		}
		/// <summary>
		/// Puts an in buffer stream onto an out buffer stream and returns the bytes written.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="input"></param>
		/// <param name="numBytesMax"></param>
		/// <returns></returns>
		public static int Put(ByteBuffer output, ByteBuffer input, int numBytesMax) 
		{
			int limit = input.Limit;
			int numBytesRead = (numBytesMax > input.Remaining) ? input.Remaining : numBytesMax;
			/*
			input.Limit = (int)input.Position + numBytesRead;
			output.Put(input);
			input.Limit = limit;
			*/
			output.Put(input, numBytesRead);
			return numBytesRead;
		}
	}
}
