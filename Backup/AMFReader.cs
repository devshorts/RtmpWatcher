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
using System.Configuration;
using System.Xml;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Reflection;
// Import log4net classes.
using log4net;
using log4net.Config;

using com.TheSilentGroup.Fluorine.Exceptions;
using com.TheSilentGroup.Fluorine.Activation;
using com.TheSilentGroup.Fluorine.AMF3;

namespace com.TheSilentGroup.Fluorine
{
	/// <summary>
	/// AMFReader reads binary data from the input stream.<br/>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	public class AMFReader : BinaryReader
	{
		private ILog _log;

		bool _useLegacyCollection = true;

		Hashtable _amf0ObjectReferences;
		Hashtable _objectReferences;
		Hashtable _stringReferences;
		Hashtable _classDefinitions;

		/// <summary>
		/// Initializes a new instance of the AMFReader class based on the supplied stream and using UTF8Encoding.
		/// </summary>
		/// <param name="stream"></param>
		public AMFReader(Stream stream) : base(stream)
		{
			try
			{
				_log = LogManager.GetLogger(typeof(AMFReader));
			}
			catch{}
			Reset();
		}

		public void Reset()
		{
			_amf0ObjectReferences = new Hashtable(5);
			_objectReferences = new Hashtable(15);
			_stringReferences = new Hashtable(15);
			_classDefinitions = new Hashtable(2);
		}

		public bool UseLegacyCollection
		{
			get{ return _useLegacyCollection; }
			set{ _useLegacyCollection = value; }
		}

		public object ReadData()
		{
			return ReadData(null);
		}

		public object ReadData(IApplicationContext applicationContext)
		{
			int typeCode = this.BaseStream.ReadByte();
			return this.ReadData(typeCode, applicationContext);
		}

		/// <summary>
		/// Maps a type code to an access method.
		/// </summary>
		/// <param name="typeCode"></param>
		/// <param name="applicationContext"></param>
		/// <returns></returns>
		internal object ReadData(int typeCode, IApplicationContext applicationContext)
		{
			object obj = null;
			switch(typeCode)
			{
					// Number
				case AMF0TypeCode.Number:
				{
					double value = ReadDouble();
					return value;
				}
					// Boolean
				case AMF0TypeCode.Boolean:
				{
					bool value = ReadBoolean();
					return value;
				}
					// String
				case AMF0TypeCode.String:
					return ReadString();
					// Object "Object"
				case AMF0TypeCode.ASObject:
					return ReadASObject(applicationContext);
				case 4:
					throw new UnexpectedAMF();
					// Null
				case AMF0TypeCode.Null:
					return obj;
					// Undefined
				case 6:
					return obj;
					// Circular references
				case AMF0TypeCode.Reference:
				{
					int reference = ReadUInt16();
					return _amf0ObjectReferences[reference];
				}				
					//AssociativeArray
				case AMF0TypeCode.AssociativeArray:
					return ReadAssociativeArray(applicationContext);
				case 9:
					throw new UnexpectedAMF();
					// Array
				case AMF0TypeCode.Array:
					IList list = ReadArray(applicationContext);
					return list;
					// DateTime
				case AMF0TypeCode.DateTime:
				{
					DateTime value = ReadDateValue();
					return value;
				}
				case AMF0TypeCode.LongString:
					int length = this.ReadInt32();
					return ReadUTF(length);
				case 13:
					throw new UnexpectedAMF();
				case 14:
					throw new UnexpectedAMF();
					// XML
				case AMF0TypeCode.Xml:
					return ReadXmlDocument();
					// Custom Class
				case AMF0TypeCode.CustomClass:
				{
					//We have a custom type.
					object amfObject = ReadObject(applicationContext);
					return amfObject;
				}
					// AMF3 Data
				case AMF0TypeCode.AMF3Tag:
				{
					return ReadAMF3Data(applicationContext);
				}
				default:
					throw new UnexpectedAMF();
			}
		}

		/// <summary>
		/// Reads a 2-byte unsigned integer from the current stream using little endian encoding and advances the position of the stream by two bytes.
		/// </summary>
		/// <returns></returns>
		public override ushort ReadUInt16()
		{
			//Read the next 2 bytes, shift and add.
			byte[] bytes = this.ReadBytes(2);
			return (ushort)(((bytes[0] & 0xff) << 8) | (bytes[1] & 0xff));
		}

		public override short ReadInt16()
		{
			//Read the next 2 bytes, shift and add.
			byte[] bytes = this.ReadBytes(2);
			return (short)((bytes[0] << 8) | bytes[1]);
		}

		public override string ReadString()
		{
			//Get the length of the string (first 2 bytes).
			int length = ReadUInt16();
			return this.ReadUTF(length);
		}

		public override bool ReadBoolean()
		{
			return base.ReadBoolean();
		}

		public override int ReadInt32()
		{
			// Read the next 4 bytes, shift and add
			byte[] bytes = this.ReadBytes(4);
			return ((bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3]);
		}
 
		public override double ReadDouble()
		{			
			byte[] bytes = this.ReadBytes(8);
			byte[] reverse = new byte[8];
			//Grab the bytes in reverse order 
			for(int i = 7, j = 0 ; i >= 0 ; i--, j++)
			{
				reverse[j] = bytes[i];
			}
			double value = BitConverter.ToDouble(reverse, 0);
			return value;
		}

		public float ReadFloat()
		{			
			byte[] bytes = this.ReadBytes(4);
			byte[] invertedBytes = new byte[4];
			//Grab the bytes in reverse order from the backwards index
			for(int i = 3, j = 0 ; i >= 0 ; i--, j++)
			{
				invertedBytes[j] = bytes[i];
			}
			float value = BitConverter.ToSingle(invertedBytes, 0);
			return value;
		}

		public object ReadObject(IApplicationContext applicationContext)
		{
			string typeIdentifier = ReadString();

			if( _log != null && _log.IsDebugEnabled )
				_log.Debug("Attempt to read custom object " + typeIdentifier + ".");

			//First try to locate the type.
			Type type = ObjectFactory.Locate(applicationContext, typeIdentifier);
			if( type != null )
			{
				object obj = ObjectFactory.CreateInstance(applicationContext, type);
				_amf0ObjectReferences.Add( _amf0ObjectReferences.Count, obj);

				string key = CustomMemberMapper.Instance.ToDotNet(ReadString());
				for(int typeCode = ReadByte(); typeCode != 9; typeCode = ReadByte())
				{
					object value = ReadData(typeCode, applicationContext);
					
					PropertyInfo pi = type.GetProperty(key);
					if( pi != null )
					{
						object castValue = TypeHelper.ChangeType(applicationContext, value, pi.PropertyType);
						pi.SetValue( obj, castValue, null );
					}
					else
					{
						FieldInfo fi = type.GetField(key, BindingFlags.Public | BindingFlags.Instance);
						if( fi != null )
						{
							object castValue = TypeHelper.ChangeType(applicationContext, value, fi.FieldType);
							fi.SetValue( obj, castValue );
						}
						else
							if( _log != null && _log.IsWarnEnabled )
								_log.Warn("Custom object " + typeIdentifier + " property or field " + key + " not found.");
					}
					key = CustomMemberMapper.Instance.ToDotNet(ReadString());
				}
				return obj;
			}
			else
			{
				if( _log != null && _log.IsWarnEnabled )
					_log.Warn("Custom object " + typeIdentifier + " could not be loaded.");

				ASObject asObject;
				asObject = ReadASObject(applicationContext);
				asObject.TypeName = typeIdentifier;
				return asObject;
			}
		}

		public ASObject ReadASObject(IApplicationContext applicationContext)
		{
			ASObject asObject = new ASObject();
			_amf0ObjectReferences.Add( _amf0ObjectReferences.Count, asObject);
			string key = this.ReadString();
			for(int typeCode = this.BaseStream.ReadByte(); typeCode != 9; typeCode = this.BaseStream.ReadByte())
			{
				asObject.Add(key, this.ReadData(typeCode, applicationContext));
				key = this.ReadString();
			}
			return asObject;
		}

		
		public string ReadUTF(int length)
		{
			if( length == 0 )
				return string.Empty;
			UTF8Encoding utf8 = new UTF8Encoding(false, true);
			byte[] encodedBytes = this.ReadBytes(length);
			string decodedString = utf8.GetString(encodedBytes);
			return LineEndingConverter.Convert(decodedString, Environment.NewLine);
		}
		
		public string ReadLongUTFString()
		{
			int length = this.ReadInt32();
			return this.ReadUTF(length);
		}

		private Hashtable ReadAssociativeArray(IApplicationContext applicationContext)
		{
			// Get the length property set by flash.
			int length = this.ReadInt32();
			Hashtable result = new Hashtable(length);
			_amf0ObjectReferences.Add( _amf0ObjectReferences.Count, result);
			string key = ReadString();
			for(int typeCode = this.BaseStream.ReadByte(); typeCode != 9; typeCode = this.BaseStream.ReadByte())
			{
				object value = ReadData(typeCode, applicationContext);
				result.Add(key, value);
				key = ReadString();
			}
			return result;
		}

		private IList ReadArray(IApplicationContext applicationContext)
		{
			//Get the length of the array.
			int length = ReadInt32();
			//object[] array = new object[length];
			ArrayList array = new ArrayList(length);
			_amf0ObjectReferences.Add( _amf0ObjectReferences.Count, array);
			for(int i = 0; i < length; i++)
			{
				//array[i] = ReadData(typeCode, applicationContext);
				array.Add( ReadData(applicationContext) );
			}
			return array;
		} 

		private DateTime ReadDateValue()
		{
			double milliseconds = this.ReadDouble();
			DateTime start = new DateTime(1970, 1, 1);

			DateTime date = start.AddMilliseconds(milliseconds);
			int tmp = ReadUInt16();
			//Note for the latter than values greater than 720 (12 hours) are 
			//represented as 2^16 - the value.
			//Thus GMT+1 is 60 while GMT-5 is 65236
			if(tmp > 720)
			{
				tmp = (65536 - tmp);
			}
			int tz = tmp / 60;
			
			string timezoneCompensation = System.Configuration.ConfigurationSettings.AppSettings["timezoneCompensation"];

			DateWrapper.SetTimeZone(tz);
			if( timezoneCompensation != null )
			{
				switch(timezoneCompensation.ToLower())
				{
					case "none":
						break;
					case "auto":
						date = date.AddHours(tz);
						
						//if(TimeZone.CurrentTimeZone.IsDaylightSavingTime(date))
						//	date = date.AddMilliseconds(-3600000);
						
						break;
				}
			}

			return date;
		}
 
		public XmlDocument ReadXmlDocument()
		{
			string text = this.ReadLongUTFString();
			XmlDocument document = new XmlDocument();
			document.LoadXml(text);
			return document;
		}


		#region AMF3

		public object ReadAMF3Data(IApplicationContext applicationContext)
		{
			byte typeCode = this.ReadByte();
			return this.ReadAMF3Data(typeCode, applicationContext);
		}

		/// <summary>
		/// Maps a type code to an access method.
		/// </summary>
		/// <param name="typeCode"></param>
		/// <param name="applicationContext"></param>
		/// <returns></returns>
		internal object ReadAMF3Data(byte typeCode, IApplicationContext applicationContext)
		{
			switch(typeCode)
			{
					// null
				case AMF3TypeCode.Undefined:
					return null;
					// null
				case AMF3TypeCode.Null:
					return null;
					// boolean-false
				case AMF3TypeCode.BooleanFalse:
					return false;
					// boolean-true
				case AMF3TypeCode.BooleanTrue:
					return true;
					// integer
				case AMF3TypeCode.Integer:
				{
					int value = ReadAMF3Int();
					return value;
				}
					// number
				case AMF3TypeCode.Number:
				{
					double value = this.ReadDouble();
					return value;
				}
					// string
				case AMF3TypeCode.String:
					return ReadAMF3String();
					// date
				case AMF3TypeCode.DateTime:
				{
					DateTime value = ReadAMF3Date();
					return value;
				}
					// array
				case AMF3TypeCode.Array:
					return ReadAMF3Array(applicationContext);
					// object
				case AMF3TypeCode.Object:
				{
					int handle = ReadAMF3IntegerData();
					object obj = ReadAMF3Object(handle, applicationContext);
					return obj;
				}
					//xml
				case AMF3TypeCode.Xml2://Coldfusion xml?
				case AMF3TypeCode.Xml:
				{
					XmlDocument xmlDocument = ReadAMF3XmlDocument();
					return xmlDocument;
				}
					//ByteArray
				case AMF3TypeCode.ByteArray:
				{
					return ReadAMF3ByteArray(applicationContext);
				}
				default:
					throw new UnexpectedAMF();
			}
		}

		/// <summary>
		/// Handle decoding of the variable-length representation
		/// which gives seven bits of value per serialized byte by using the high-order bit 
		/// of each byte as a continuation flag.
		/// </summary>
		/// <returns></returns>
		public int ReadAMF3IntegerData()
		{
			int acc = this.ReadByte();
			int tmp;
			if(acc < 128)
				return acc;
			else
			{
				acc = (acc & 0x7f) << 7;
				tmp = this.ReadByte();
				if(tmp < 128)
					acc = acc | tmp;
				else
				{
					acc = (acc | tmp & 0x7f) << 7;
					tmp = this.ReadByte();
					if(tmp < 128)
						acc = acc | tmp;
					else
					{
						acc = (acc | tmp & 0x7f) << 8;
						tmp = this.ReadByte();
						acc = acc | tmp;
					}
				}
			}
			//To sign extend a value from some number of bits to a greater number of bits just copy the sign bit into all the additional bits in the new format.
			//convert/sign extend the 29bit two's complement number to 32 bit
			int mask = 1 << 28; // mask
			int r = -(acc & mask) | acc;
			return r;

			//The following variation is not portable, but on architectures that employ an 
			//arithmetic right-shift, maintaining the sign, it should be fast. 
			//s = 32 - 29;
			//r = (x << s) >> s;
		}

		public int ReadAMF3Int()
		{
			int intData = ReadAMF3IntegerData();
			return intData;
		}

		public DateTime ReadAMF3Date()
		{
			int handle = ReadAMF3IntegerData();
			bool inline = ((handle & 1)  != 0 );
			handle = handle >> 1;
			if( inline )
			{
				double milliseconds = this.ReadDouble();
				DateTime start = new DateTime(1970, 1, 1, 0, 0, 0);

				DateTime date = start.AddMilliseconds(milliseconds);
				string timezoneCompensation = System.Configuration.ConfigurationSettings.AppSettings["timezoneCompensation"];
				if( timezoneCompensation != null )
				{
					switch(timezoneCompensation.ToLower())
					{
						case "none":
							break;
						case "auto":
							date = date.ToLocalTime();
							break;
					}
				}				
				_objectReferences.Add( _objectReferences.Count, date);
				return date;
			}
			else
			{
				return (DateTime)_objectReferences[handle];
			}
		}

		public string ReadAMF3String()
		{
			int handle = ReadAMF3IntegerData();
			bool inline = ((handle & 1) != 0 );
			handle = handle >> 1;
			if( inline )
			{
				int length = handle;
				if( length == 0 )
					return string.Empty;
				string str = this.ReadUTF(length);
				_stringReferences.Add( _stringReferences.Count, str);
				return str;
			}
			else
			{
				return _stringReferences[handle] as string;
			}
		}

		public XmlDocument ReadAMF3XmlDocument()
		{
			//var x:XML = new XML("<a>test</a>"); will not work.....?

			int handle = ReadAMF3IntegerData();
			bool inline = ((handle & 1) != 0 );
			handle = handle >> 1;
			string xml = null;
			if( inline )
			{
				xml = this.ReadUTF(handle);
				_objectReferences.Add( _objectReferences.Count, xml);
			}
			else
			{
				xml = _objectReferences[handle] as string;
			}
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xml);
			return xmlDocument;
		}

		public ByteArray ReadAMF3ByteArray(IApplicationContext applicationContext)
		{
			int handle = ReadAMF3IntegerData();
			bool inline = ((handle & 1) != 0 );
			handle = handle >> 1;
			if( inline )
			{
				int length = handle;
				byte[] buffer = ReadBytes(length);
				ByteArray ba = new ByteArray(applicationContext, buffer);
				_objectReferences.Add( _objectReferences.Count, ba);
				return ba;
			}
			else
				return _objectReferences[handle] as ByteArray;
		}

		public object ReadAMF3Array(IApplicationContext applicationContext)
		{
			int handle = ReadAMF3IntegerData();
			bool inline = ((handle & 1)  != 0 ); handle = handle >> 1;
			if( inline )
			{
				Hashtable hashtable = null;
				string key = ReadAMF3String();
				while( key != string.Empty )
				{
					if( hashtable == null )
					{
						hashtable = new Hashtable();
						_objectReferences.Add(_objectReferences.Count, hashtable);
					}
					object value = ReadAMF3Data(applicationContext);
					hashtable.Add(key, value);
					key = ReadAMF3String();
				}
				//Not an associative array
				if( hashtable == null )
				{
					IList array;
					if(!_useLegacyCollection)
						array = new object[handle];
					else
						array = new ArrayList(handle);
					_objectReferences.Add(_objectReferences.Count, array);
					for(int i = 0; i < handle; i++)
					{
						//Grab the type for each element.
						byte typeCode = this.ReadByte();
						object value = ReadAMF3Data(typeCode, applicationContext);
						if( array is ArrayList )
							array.Add(value);
						else
							array[i] = value;
					}
					return array;
				}
				else
				{
					for(int i = 0; i < handle; i++)
					{
						object value = ReadAMF3Data(applicationContext);
						hashtable.Add( i.ToString(), value);
					}
					return hashtable;
				}
			}
			else
			{
				return _objectReferences[handle];
			}
		}

		public object ReadAMF3Object(int handle, IApplicationContext applicationContext)
		{
			bool inline = ((handle & 1) != 0 ); handle = handle >> 1;
			ClassDefinition classDefinition = null;
			if( inline )
			{				
				//an inline object
				bool inlineClassDef = ((handle & 1) != 0 );handle = handle >> 1;
				if( inlineClassDef )
				{
					string typeIdentifier = null;
					int classMemberCount = 0;
					//inline class-def
					typeIdentifier = this.ReadAMF3String();
					bool typedObject = (typeIdentifier != null && typeIdentifier != string.Empty );
					//flags that identify the way the object is serialized/deserialized
					bool externalizable = ((handle & 1) != 0 );handle = handle >> 1;
					bool dynamic = ((handle & 1) != 0 );handle = handle >> 1;
					classMemberCount = handle;

					ClassMemberDefinition[] classMemberDefinitions = new ClassMemberDefinition[classMemberCount];
					for(int i = 0; i < classMemberCount; i++)
					{
						string key = CustomMemberMapper.Instance.ToDotNet(ReadAMF3String());
						classMemberDefinitions[i] = new ClassMemberDefinition(key);
					}
					string mappedTypeName = typeIdentifier;
					if( applicationContext != null )
						mappedTypeName = applicationContext.GetMappedTypeName(typeIdentifier);

					classDefinition = new ClassDefinition(typeIdentifier, mappedTypeName, classMemberCount, classMemberDefinitions, externalizable, dynamic);
					_classDefinitions.Add( _classDefinitions.Count, classDefinition);
				}
				else
				{
					//a reference to a previously passed class-def
					classDefinition = _classDefinitions[handle] as ClassDefinition;
				}
			}
			else
			{
				//an object reference
				int index = handle;
				return _objectReferences[index];
			}
			if( _log != null && _log.IsDebugEnabled )
			{
				if( classDefinition.IsTypedObject )
					_log.Debug("Attempt to read custom object " + classDefinition.ClassName);
				else
					_log.Debug("Attempt to read ASObject");
			}

			object obj = null;
			Type type = null;
			if( classDefinition.IsTypedObject )
			{
				type = ObjectFactory.Locate(applicationContext, classDefinition.ClassName);
				if( type != null )
					obj = ObjectFactory.CreateInstance(applicationContext, type);
				else
				{
					type = typeof(ASObject);
					obj = new ASObject();
				}
			}
			else
			{
				type = typeof(ASObject);
				obj = new ASObject();
			}

			//Add to references as circular references may search for this object
			_objectReferences.Add( _objectReferences.Count, obj);
			
			if( classDefinition.IsExternalizable )
			{
				if( obj is IExternalizable )
				{
					IExternalizable externalizable = obj as IExternalizable;
					DataInput dataInput = new DataInput(applicationContext, this);
					externalizable.ReadExternal(dataInput);
				}
				else
					throw new FluorineException("Object does not implement IExternalizable.");
			}
			else
			{
				for(int i = 0; i < classDefinition.MemberCount; i++)
				{
					string key = classDefinition.ClassMemberDefinitions[i].ClassMember;
					object value = this.ReadAMF3Data(applicationContext);
					
					PropertyInfo pi = type.GetProperty(key);
					if( pi != null )
					{
						try
						{
							object castValue = TypeHelper.ChangeType(applicationContext, value, pi.PropertyType);
							pi.SetValue( obj, castValue, null );
						}
						catch(Exception ex)
						{
							if( _log != null && _log.IsErrorEnabled )
								_log.Error("Property set value failed " + classDefinition.ClassName + "." + pi.Name + "\r\n" + ex);
							throw new FluorineException("Custom object " + classDefinition.ClassName + " setting property " + key + " failed. " + ex.Message, ex);
						}
					}
					else
					{
						FieldInfo fi = type.GetField(key, BindingFlags.Public | BindingFlags.Instance);
						try
						{
							if( fi != null )
							{
								object castValue = TypeHelper.ChangeType(applicationContext, value, fi.FieldType);
								fi.SetValue( obj, castValue );
							}
							else
								if( _log != null && _log.IsWarnEnabled )
									_log.Warn("Custom object " + classDefinition.ClassName + " property or field " + key + " not found.");
						}
						catch(Exception ex)
						{
							if( _log != null && _log.IsErrorEnabled )
								_log.Error("Field set value failed " + classDefinition.ClassName + "." + fi.Name);
							throw new FluorineException("Custom object " + classDefinition.ClassName + " setting field " + key + " failed. " + ex.Message, ex);
						}
					}
				}

				if(classDefinition.IsDynamic && obj is ASObject)
				{
					ASObject asObject = obj as ASObject;
					string key = ReadAMF3String();
					while( key != string.Empty )
					{
						object value = ReadAMF3Data(applicationContext);
						asObject.Add(key, value);
						key = ReadAMF3String();
					}
				}
			}
			return obj;
		}

		#endregion AMF3

 	}
}
