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
using System.ComponentModel;

using com.TheSilentGroup.Fluorine;

namespace com.TheSilentGroup.Fluorine.AMF3
{
	/// <summary>
	/// Provides a type converter to convert ByteArray objects to and from various other representations.
	/// </summary>
	public class ByteArrayConverter : TypeConverter
	{
		/// <summary>
		/// Overloaded. Returns whether this converter can convert the object to the specified type.
		/// </summary>
		/// <param name="context">An ITypeDescriptorContext that provides a format context.</param>
		/// <param name="destinationType">A Type that represents the type you want to convert to.</param>
		/// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if( destinationType == typeof(byte[]) )
				return true;
			return base.CanConvertTo(context, destinationType);
		}
		/// <summary>
		/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
		/// </summary>
		/// <param name="context">An ITypeDescriptorContext that provides a format context.</param>
		/// <param name="culture">A CultureInfo object. If a null reference (Nothing in Visual Basic) is passed, the current culture is assumed.</param>
		/// <param name="value">The Object to convert.</param>
		/// <param name="destinationType">The Type to convert the value parameter to.</param>
		/// <returns>An Object that represents the converted value.</returns>
		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			if( destinationType == typeof(byte[]) )
			{
				return (value as ByteArray).MemoryStream.ToArray();
			}
			return base.ConvertTo (context, culture, value, destinationType);
		}
	}

	/// <summary>
	/// Flex ByteArray. The ByteArray class provides methods and properties to optimize reading, writing, and working with binary data.
	/// </summary>
	[TypeConverter(typeof(ByteArrayConverter))]
	public class ByteArray : IDataInput, IDataOutput
	{
		private MemoryStream _memoryStream;
		private DataOutput _dataOutput;
		private DataInput _dataInput;
		private IApplicationContext _applicationContext;

		/// <summary>
		/// Initializes a new instance of the ByteArray class.
		/// </summary>
		/// <param name="applicationContext"></param>
		/// <param name="buffer"></param>
		internal ByteArray(IApplicationContext applicationContext, byte[] buffer)
		{
			_applicationContext = applicationContext;

			_memoryStream = new MemoryStream();
			_memoryStream.Write(buffer, 0, buffer.Length);
			_memoryStream.Position = 0;
			AMFReader amfReader = new AMFReader(_memoryStream);
			AMFWriter amfWriter = new AMFWriter(_memoryStream);
			_dataOutput = new DataOutput(applicationContext, amfWriter);
			_dataInput = new DataInput(applicationContext, amfReader);
		}
		/// <summary>
		/// Gets the length of the ByteArray object, in bytes.
		/// </summary>
		public uint Length
		{
			get
			{ 
				return (uint)_memoryStream.Length;
			}
		}
		/// <summary>
		/// Gets or sets the current position, in bytes, of the file pointer into the ByteArray object.
		/// </summary>
		public int Position
		{
			get{ return (int)_memoryStream.Position; }
			set{ _memoryStream.Position = value; }
		}

		internal MemoryStream MemoryStream{ get{ return _memoryStream; } }

		#region IDataInput Members

		/// <summary>
		/// Reads a Boolean from the byte stream or byte array. 
		/// </summary>
		/// <returns></returns>
		public bool ReadBoolean()
		{
			return _dataInput.ReadBoolean();
		}
		/// <summary>
		/// Reads a signed byte from the byte stream or byte array. 
		/// </summary>
		/// <returns></returns>
		public byte ReadByte()
		{
			return _dataInput.ReadByte();
		}
		/// <summary>
		/// Reads length bytes of data from the byte stream or byte array. 
		/// </summary>
		/// <param name="bytes"></param>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		public void ReadBytes(byte[] bytes, uint offset, uint length)
		{
			_dataInput.ReadBytes(bytes, offset, length);
		}
		/// <summary>
		/// Reads an IEEE 754 double-precision floating point number from the byte stream or byte array. 
		/// </summary>
		/// <returns></returns>
		public double ReadDouble()
		{
			return _dataInput.ReadDouble();
		}
		/// <summary>
		/// Reads an IEEE 754 single-precision floating point number from the byte stream or byte array. 
		/// </summary>
		/// <returns></returns>
		public float ReadFloat()
		{
			return _dataInput.ReadFloat();
		}
		/// <summary>
		/// Reads a signed 32-bit integer from the byte stream or byte array. 
		/// </summary>
		/// <returns></returns>
		public int ReadInt()
		{
			return _dataInput.ReadInt();
		}
		/// <summary>
		/// Reads an object from the byte stream or byte array, encoded in AMF serialized format. 
		/// </summary>
		/// <returns></returns>
		public object ReadObject()
		{
			return _dataInput.ReadObject();
		}
		/// <summary>
		/// Reads a signed 16-bit integer from the byte stream or byte array. 
		/// </summary>
		/// <returns></returns>
		public short ReadShort()
		{
			return _dataInput.ReadShort();
		}
		/// <summary>
		/// Reads an unsigned byte from the byte stream or byte array. 
		/// </summary>
		/// <returns></returns>
		public byte ReadUnsignedByte()
		{
			return _dataInput.ReadUnsignedByte();
		}
		/// <summary>
		/// Reads an unsigned 32-bit integer from the byte stream or byte array. 
		/// </summary>
		/// <returns></returns>
		public uint ReadUnsignedInt()
		{
			return _dataInput.ReadUnsignedInt();
		}
		/// <summary>
		/// Reads an unsigned 16-bit integer from the byte stream or byte array. 
		/// </summary>
		/// <returns></returns>
		public ushort ReadUnsignedShort()
		{
			return _dataInput.ReadUnsignedShort();
		}
		/// <summary>
		/// Reads a UTF-8 string from the byte stream or byte array. 
		/// </summary>
		/// <returns></returns>
		public string ReadUTF()
		{
			return _dataInput.ReadUTF();
		}
		/// <summary>
		/// Reads a sequence of length UTF-8 bytes from the byte stream or byte array, and returns a string. 
		/// </summary>
		/// <param name="length"></param>
		/// <returns></returns>
		public string ReadUTFBytes(uint length)
		{
			return _dataInput.ReadUTFBytes(length);
		}

		#endregion

		#region IDataOutput Members

		public void WriteBoolean(bool value)
		{
			_dataOutput.WriteBoolean(value);
		}

		public void WriteByte(byte value)
		{
			_dataOutput.WriteByte(value);
		}

		public void WriteBytes(byte[] bytes, int offset, int length)
		{
			_dataOutput.WriteBytes(bytes, offset, length);
		}

		public void WriteDouble(double value)
		{
			_dataOutput.WriteDouble(value);
		}

		public void WriteFloat(float value)
		{
			_dataOutput.WriteFloat(value);
		}

		public void WriteInt(int value)
		{
			_dataOutput.WriteInt(value);
		}

		public void WriteObject(object value)
		{
			_dataOutput.WriteObject(value);
		}

		public void WriteShort(short value)
		{
			_dataOutput.WriteShort(value);
		}

		public void WriteUnsignedInt(uint value)
		{
			_dataOutput.WriteUnsignedInt(value);
		}

		public void WriteUTF(string value)
		{
			_dataOutput.WriteUTF(value);
		}

		public void WriteUTFBytes(string value)
		{
			_dataOutput.WriteUTFBytes(value);
		}

		#endregion
	}
}
