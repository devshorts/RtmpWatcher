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
using System.Xml;
using System.IO;
using System.Text;
using System.Collections;
using System.Data;
using System.ComponentModel;
using System.Reflection;
// Import log4net classes.
using log4net;
using log4net.Config;

using com.TheSilentGroup.Fluorine.Exceptions;
using com.TheSilentGroup.Fluorine.AMF3;

namespace com.TheSilentGroup.Fluorine
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	public class AMFWriter : BinaryWriter
	{
		private ILog _log;

		bool _useLegacyCollection = true;

		Hashtable	_amf0ObjectReferences;
		Hashtable	_objectReferences;
		Hashtable	_stringReferences;
		Hashtable	_classDefinitions;
		Hashtable	_classDefinitionReferences;

        private readonly TransientAttribute _transientAttribute = new TransientAttribute();
		
		/// <summary>
		/// Initializes a new instance of the AMFReader class based on the supplied stream and using UTF8Encoding.
		/// </summary>
		/// <param name="stream"></param>
		public AMFWriter(Stream stream) : base(stream)
		{
			try
			{
				_log = LogManager.GetLogger(typeof(AMFWriter));
			}
			catch{}
			Reset();
		}

		public void Reset()
		{
			_amf0ObjectReferences = new Hashtable(5);
			_objectReferences = new Hashtable(5);
			_stringReferences = new Hashtable(5);
			_classDefinitions = new Hashtable();
			_classDefinitionReferences = new Hashtable();
		}

		public bool UseLegacyCollection
		{
			get{ return _useLegacyCollection; }
			set{ _useLegacyCollection = value; }
		}

		public void WriteByte(byte value)
		{
			this.BaseStream.WriteByte(value);
		}

		public void WriteByte(int value)
		{
			this.BaseStream.WriteByte((byte)value);
		}

		public void WriteBytes(byte[] buffer)
		{
			for(int i = 0; buffer != null && i < buffer.Length; i++)
				this.BaseStream.WriteByte(buffer[i]);
		}

		public void WriteShort(int n)
		{
			byte[] bytes = BitConverter.GetBytes((ushort)n);
			WriteBigEndian(bytes);
		}

		public void WriteString(string str)
		{
			UTF8Encoding utf8Encoding = new UTF8Encoding(true, true);
			int byteCount = utf8Encoding.GetByteCount(str);
			if( byteCount < 65536 )
			{
				WriteByte(AMF0TypeCode.String);
				WriteUTF(str);
			}
			else
			{
				WriteByte(AMF0TypeCode.LongString);
				WriteLongUTF(str);
			}
		}

		public void WriteUTF(string str)
		{
			//null string is not accepted
			//in case of custom serialization TypeError: Error #2007: Parameter value must be non-null.  at flash.utils::ObjectOutput/writeUTF()
			//Length - max 65536.
			
			str = LineEndingConverter.Convert(str, LineEndingConverter.CR_STRING);

			UTF8Encoding utf8Encoding = new UTF8Encoding();
			int byteCount = utf8Encoding.GetByteCount(str);
			byte[] buffer = utf8Encoding.GetBytes(str);
			this.WriteShort(byteCount);
            if (buffer.Length > 0)
                base.Write(buffer);
		}

		public void WriteUTFBytes(string value)
		{
			value = LineEndingConverter.Convert(value, LineEndingConverter.CR_STRING);
			//Length - max 65536.
			UTF8Encoding utf8Encoding = new UTF8Encoding();
			byte[] buffer = utf8Encoding.GetBytes(value);
			if (buffer.Length > 0)
				base.Write(buffer);
		}

		protected void WriteLongUTF(string str)
		{
			str = LineEndingConverter.Convert(str, LineEndingConverter.CR_STRING);
			UTF8Encoding utf8Encoding = new UTF8Encoding(true, true);
			uint byteCount = (uint)utf8Encoding.GetByteCount(str);
			byte[] buffer = new Byte[byteCount+4];
			//unsigned long (always 32 bit, big endian byte order)
			buffer[0] = (byte)((byteCount >> 0x18) & 0xff);
			buffer[1] = (byte)((byteCount >> 0x10) & 0xff);
			buffer[2] = (byte)((byteCount >> 8) & 0xff);
			buffer[3] = (byte)((byteCount & 0xff));
			int bytesEncodedCount = utf8Encoding.GetBytes(str, 0, str.Length, buffer, 4);

            if (buffer.Length > 0)
                base.BaseStream.Write(buffer, 0, buffer.Length);
		}

		
		public void WriteData(object data, ObjectEncoding objectEncoding)
		{
			WriteData(null, objectEncoding, data);
		}

		public void WriteData(IApplicationContext applicationContext, ObjectEncoding objectEncoding, object data)
		{
			if( applicationContext != null && applicationContext.NullableValues != null && data != null )
			{
				Type type = data.GetType();
				if( applicationContext.NullableValues.ContainsKey(type) &&
					data.Equals(applicationContext.NullableValues[type]) )
					data = null;
			}

			if( data == null )
			{
				//Write the null code (0x05) to the output stream.
				WriteNull();
				return;
			}
			if(data is DBNull )
			{
				this.WriteNull();
				return;
			}
			if(data is System.Data.SqlTypes.INullable )
			{
				System.Data.SqlTypes.INullable nullable = data as System.Data.SqlTypes.INullable;
				if( nullable.IsNull )
				{
					this.WriteNull();
					return;
				}
			}

			if(data is Guid )
			{
				WriteString( ((Guid)data).ToString("N") );
				return;
			}
			if(data is CacheResult)
			{
				WriteBytes( (data as CacheResult).Result );
				return;
			}
			//Check common types first.
			if(data is string)
			{
				WriteString((string)data);
				return;
			}
			if(data is bool)
			{
				WriteByte(AMF0TypeCode.Boolean);
				this.WriteBoolean((bool)data);
				return;
			}
			if(data is sbyte || data is short || data is int || data is long || data is byte || data is ushort || data is uint || data is ulong || data is float || data is double || data is decimal)
			{
				WriteByte(AMF0TypeCode.Number);
				WriteDouble(Convert.ToDouble(data));
				return;
			}

			if(data is Enum)
			{
				WriteByte(AMF0TypeCode.Number);
				double dbl = (double)Convert.ToInt32(data);
				WriteDouble(dbl);
				return;
			}

			if(data is char)
			{
				WriteByte(AMF0TypeCode.String);
				this.WriteUTF( new String( (char)data, 1)  );
				return;
			}

			if( objectEncoding == ObjectEncoding.AMF3 )
			{
				WriteByte(AMF0TypeCode.AMF3Tag);
				WriteAMF3Data(applicationContext, data);
				return;
			}

			if( _amf0ObjectReferences.Contains( data ) )
			{
				WriteReference( data );
				return;
			}

			if( data.GetType().IsArray )
			{
				//possibly should check for typeof(Array).IsAssignableFrom(data.GetType())

				_amf0ObjectReferences.Add( data, _amf0ObjectReferences.Count);
				this.WriteArray(applicationContext, objectEncoding, ((Array)data));
				return;
			}

			if( data is IList )
			{
				_amf0ObjectReferences.Add( data, _amf0ObjectReferences.Count);
				IList list = data as IList;
				object[] objList = new object[list.Count];
				list.CopyTo(objList, 0);
				this.WriteArray(applicationContext, objectEncoding, objList);
				return;
			}

			if(data is DateTime)
			{
				// Write date code.
				WriteByte(AMF0TypeCode.DateTime);
				this.WriteDateTime((DateTime)data);
				return;
			}

			if(data is XmlDocument)
			{
				_amf0ObjectReferences.Add( data, _amf0ObjectReferences.Count);
				this.WriteXmlDocument((XmlDocument)data);
				return;
			}

			if(data is ASObject)
			{
				_amf0ObjectReferences.Add( data, _amf0ObjectReferences.Count);
				this.WriteASO(applicationContext, objectEncoding, data as ASObject);
				return;
			}
			if(data is IDictionary)
			{
				_amf0ObjectReferences.Add( data, _amf0ObjectReferences.Count);
				this.WriteAssociativeArray(applicationContext, objectEncoding, data as IDictionary);
				return;
			}
			if(data is DataTable)
			{
				_amf0ObjectReferences.Add( data, _amf0ObjectReferences.Count);
				WriteASO( applicationContext, objectEncoding, ConvertDataTableToASO( data as DataTable, true ) );
				return;
			}
			if(data is DataSet)
			{
				_amf0ObjectReferences.Add( data, _amf0ObjectReferences.Count);
				WriteASO( applicationContext, objectEncoding, ConvertDataSetToASO( data as DataSet, true ) );
				return;
			}
			if(data is Exception)
			{
				_amf0ObjectReferences.Add( data, _amf0ObjectReferences.Count);
				WriteASO( applicationContext, objectEncoding, new ExceptionASO(data as Exception) );
				return;
			}

			//We have a custom type.
			_amf0ObjectReferences.Add( data, _amf0ObjectReferences.Count);
			WriteObject(applicationContext, objectEncoding, data);
		}

		private void WriteReference(object value)
		{
			//Circular references
			WriteByte(AMF0TypeCode.Reference);
			WriteShort((int)_amf0ObjectReferences[value]);
		}

		public void WriteNull()
		{
			//Write the null code (0x05) to the output stream.
			WriteByte(AMF0TypeCode.Null);
		}

		public void WriteDouble(double value)
		{
			long tmp = BitConverter.DoubleToInt64Bits( value );
			this.WriteLong(tmp);
		}

		public void WriteFloat(float value)
		{
			byte[] bytes = BitConverter.GetBytes(value);			
			WriteBigEndian(bytes);
		}

		public void WriteInt32(int number)
		{
			byte[] bytes = BitConverter.GetBytes(number);
			WriteBigEndian(bytes);
		}

		public void WriteBoolean(bool b)
		{
			this.BaseStream.WriteByte(b ? ((byte) 1) : ((byte) 0));
		}

		public void WriteLong(long number)
		{
			byte[] bytes = BitConverter.GetBytes(number);
			WriteBigEndian(bytes);
		}

		private void WriteBigEndian(byte[] bytes)
		{
			if( bytes == null )
				return;
			for(int i = bytes.Length-1; i >= 0; i--)
			{
				base.BaseStream.WriteByte( bytes[i] );
			}
		}

		public void WriteDateTime(DateTime date)
		{
			string timezoneCompensation = System.Configuration.ConfigurationManager.AppSettings["timezoneCompensation"];
			if( timezoneCompensation != null && ( timezoneCompensation.ToLower() == "auto" ) )
			{
				date = date.Subtract( DateWrapper.ClientTimeZone );
			}


			// Write date (milliseconds from 1970).
			DateTime timeStart = new DateTime(1970, 1, 1);
			TimeSpan span = date.Subtract(timeStart);
			long milliSeconds = (long)span.TotalMilliseconds;
			long value = BitConverter.DoubleToInt64Bits((double)milliSeconds);
			this.WriteLong(value);

			span = TimeZone.CurrentTimeZone.GetUtcOffset(date);

			//whatever we write back, is ignored
			//this.WriteLong(span.TotalMinutes);
			//this.WriteShort((int)span.TotalHours);
			//this.WriteShort(65236);
			if( timezoneCompensation == null || timezoneCompensation.ToLower() == "none" )
			{
				this.WriteShort(0);
			}
			else
				this.WriteShort((int)(span.TotalMilliseconds/60000));
		}

		public void WriteXmlDocument(XmlDocument xmlDocument)
		{
			if(xmlDocument != null)
			{
				this.BaseStream.WriteByte((byte)15);//xml code (0x0F)
				string xml = xmlDocument.DocumentElement.OuterXml;
				this.WriteLongUTF(xml);
			}
			else
				this.WriteNull();
		}

		public void WriteArray(IApplicationContext applicationContext, ObjectEncoding objectEcoding, Array array)
		{
			if(array == null)
				this.WriteNull();
			else
			{
				base.BaseStream.WriteByte(10);
				this.WriteInt32(array.Length);
				for(int i = 0; i < array.Length; i++)
				{
					this.WriteData(applicationContext, objectEcoding, array.GetValue(i));
				}
			}
		}

		public void WriteAssociativeArray(IApplicationContext applicationContext, ObjectEncoding objectEncoding, IDictionary dictionary)
		{
			if(dictionary == null)
				this.WriteNull();
			else
			{
				this.BaseStream.WriteByte(8);
				this.WriteInt32(dictionary.Count);
				foreach(DictionaryEntry entry in dictionary)
				{
					this.WriteUTF(entry.Key.ToString());
					this.WriteData(applicationContext, objectEncoding, entry.Value);
				}
				this.WriteEndMarkup();
			}
		}

		public void WriteObject(IApplicationContext applicationContext, ObjectEncoding objectEncoding, object obj)
		{
			if( obj == null )
			{
				WriteNull();
				return;
			}

			Type type = obj.GetType();

			WriteByte(16);
			string customClass = type.FullName;
			if( applicationContext != null )
				customClass = applicationContext.GetCustomClass(customClass);

			if( _log != null && _log.IsDebugEnabled )
				_log.Debug("Write custom object " + type.FullName + " mapping to " + customClass + ".");

			WriteUTF( customClass );

			PropertyDescriptorCollection collection;
			collection = TypeDescriptor.GetProperties(obj, new Attribute[1] { DesignerSerializationVisibilityAttribute.Visible }, false );
			WritePropertyDescriptorCollection(applicationContext, objectEncoding, obj, collection);

			//Specifies that a visual designer serialize the contents of this property, rather than the property itself
			collection = TypeDescriptor.GetProperties(obj, new Attribute[1] { DesignerSerializationVisibilityAttribute.Content }, false);
			WritePropertyDescriptorCollection(applicationContext, objectEncoding, obj, collection);

			FieldInfo[] fieldInfos = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
			if(fieldInfos != null && fieldInfos.Length > 0)
			{
				for(int i = 0; i < fieldInfos.Length; i++)
				{
					FieldInfo fieldInfo = fieldInfos[i];
                    
                    //B-Line Custom: Ignore fields/properties having TransientAttribute
				    if (fieldInfo.GetCustomAttributes(typeof (TransientAttribute), false).Length == 0)
                    {
                        WriteUTF(CustomMemberMapper.Instance.ToFlash(fieldInfo.Name));
                        WriteData(applicationContext, objectEncoding, fieldInfo.GetValue(obj));
                    }
				}
			}

			WriteEndMarkup();
		}

		private void WritePropertyDescriptorCollection(IApplicationContext applicationContext, ObjectEncoding objectEncoding, object obj, PropertyDescriptorCollection collection)
		{
			foreach (PropertyDescriptor descriptor in collection)
			{
                //B-Line Custom: Ignore fields/properties having TransientAttribute
                if (!descriptor.IsReadOnly && !descriptor.Attributes.Matches(_transientAttribute))
				{
					WriteUTF(CustomMemberMapper.Instance.ToFlash(descriptor.Name));
					object value = descriptor.GetValue(obj);
					WriteData( applicationContext, objectEncoding, value);
				}
			}
		}

		public void WriteEndMarkup()
		{
			//Write the end object flag 0x00, 0x00, 0x09
			base.BaseStream.WriteByte(0);
			base.BaseStream.WriteByte(0);
			base.BaseStream.WriteByte(9);
		}

		public void WriteASO(IApplicationContext applicationContext, ObjectEncoding objectEncoding, ASObject asObject)
		{
			if(asObject.TypeName == null)
			{
				// Object "Object"
				this.BaseStream.WriteByte(3);
			}
			else
			{
				this.BaseStream.WriteByte(16);
				this.WriteUTF(asObject.TypeName);
			}
			foreach(DictionaryEntry entry in asObject)
			{
				this.WriteUTF(entry.Key.ToString());
				this.WriteData(applicationContext, objectEncoding, entry.Value);
			}
			WriteEndMarkup();
		}

		public ASObject ConvertDataTableToASO(DataTable dataTable, bool stronglyTyped)
		{
			if( dataTable.ExtendedProperties.Contains("DynamicPage") )
				return ConvertPageableDataTableToASO(dataTable, stronglyTyped);
			ASObject recordset = new ASObject();
			if( stronglyTyped )
				recordset.TypeName = "RecordSet";

			ASObject asObject = new ASObject();
			if( dataTable.ExtendedProperties["TotalCount"] != null )
				asObject["totalCount"] = (int)dataTable.ExtendedProperties["TotalCount"];
			else
				asObject["totalCount"] = dataTable.Rows.Count;
			
			if( dataTable.ExtendedProperties["Service"] != null )
				asObject["serviceName"] = AMFBody.Recordset + dataTable.ExtendedProperties["Service"] as string;
			else
				asObject["serviceName"] = "com.TheSilentGroup.Fluorine.PageableResult";
			asObject["version"] = 1;
			asObject["cursor"] = 1;
			if( dataTable.ExtendedProperties["RecordsetId"] != null )
				asObject["id"] = dataTable.ExtendedProperties["RecordsetId"] as string;
			else
				asObject["id"] = null;
			string[] columnNames = new string[dataTable.Columns.Count];
			for(int i = 0; i < dataTable.Columns.Count; i++)
			{
				columnNames[i] = dataTable.Columns[i].ColumnName;
			}
			asObject["columnNames"] = columnNames;
			object[] rows = new object[dataTable.Rows.Count];
			for(int i = 0; i < dataTable.Rows.Count; i++)
			{
				rows[i] = dataTable.Rows[i].ItemArray;
			}
			asObject["initialData"] = rows;

			recordset["serverInfo"] = asObject;
			return recordset;
		}

		public ASObject ConvertPageableDataTableToASO(DataTable dataTable, bool stronglyTyped)
		{
			ASObject recordSetPage = new ASObject();
			if( stronglyTyped )
				recordSetPage.TypeName = "RecordSetPage";
			recordSetPage["Cursor"] = (int)dataTable.ExtendedProperties["Cursor"];//pagecursor

			ArrayList rows = new ArrayList();
			for(int i = 0; i < dataTable.Rows.Count; i++)
			{
				rows.Add( dataTable.Rows[i].ItemArray );
			}
			recordSetPage["Page"] = rows;;
			return recordSetPage;
		}

		public ASObject ConvertDataSetToASO(DataSet dataSet, bool stronglyTyped)
		{
			ASObject asDataSet = new ASObject();
			if( stronglyTyped )
				asDataSet.TypeName = "DataSet";
			DataTableCollection dataTableCollection = dataSet.Tables;
			foreach(DataTable dataTable in dataTableCollection)
			{
				asDataSet[dataTable.TableName] = ConvertDataTableToASO( dataTable, stronglyTyped );
			}
			return asDataSet;
		}

		#region AMF3


		public void WriteAMF3Data(IApplicationContext applicationContext, object data)
		{
			if( applicationContext != null && applicationContext.NullableValues != null && data != null )
			{
				Type type = data.GetType();
				if( applicationContext.NullableValues.ContainsKey(type) &&
					data.Equals(applicationContext.NullableValues[type]) )
					data = null;
			}

			if( data == null )
			{
				WriteAMF3Null();
				return;
			}
			if(data is DBNull )
			{
				WriteAMF3Null();
				return;
			}
			if(data is System.Data.SqlTypes.INullable )
			{
				System.Data.SqlTypes.INullable nullable = data as System.Data.SqlTypes.INullable;
				if( nullable.IsNull )
				{
					WriteAMF3Null();
					return;
				}
			}
			if(data is Guid )
			{
				if ((Guid)data == Guid.Empty)
				{
					WriteAMF3Null();
					return;
				}
				WriteByte(AMF3TypeCode.String);
				WriteAMF3String( ((Guid)data).ToString("N") );
				return;
			}

			if(data is CacheResult)
			{
				WriteBytes( (data as CacheResult).Result );
				return;
			}

			//Check common types first.
			if(data is bool)
			{
				WriteAMF3Bool((bool)data);
				return;
			}

			if(data is string)
			{
				WriteByte(AMF3TypeCode.String);
				WriteAMF3String((string)data);
				return;
			}

			if(data is sbyte || data is short || data is int || data is byte || data is ushort || data is uint )
			{
				WriteAMF3Int(Convert.ToInt32(data));
				return;
			}

			if(data is long || data is ulong || data is float || data is double || data is decimal)
			{
				WriteAMF3Double(Convert.ToDouble(data));
				return;
			}

			if(data is Enum)
			{
				WriteAMF3Int(Convert.ToInt32(data));
				return;
			}

			if(data is char)
			{
				WriteByte(AMF3TypeCode.String);
				WriteAMF3String( new String( (char)data, 1)  );
				return;
			}
			if(data is DateTime)
			{
				if ((DateTime)data == DateTime.MinValue)
				{
					WriteAMF3Null();
					return;
				}
				WriteByte(AMF3TypeCode.DateTime);
				WriteAMF3DateTime((DateTime)data);
				return;
			}

			if(data is byte[])
			{
				data = new ByteArray(applicationContext, data as byte[]);
			}

			if(data is ByteArray)
			{
				_objectReferences.Add(data, _objectReferences.Count);
				ByteArray byteArray = data as ByteArray;
				WriteByte(AMF3TypeCode.ByteArray);
				int handle = (int)byteArray.Length;
				handle = handle << 1;
				handle = handle | 1;
				WriteAMF3IntegerData(handle);
				WriteBytes( byteArray.MemoryStream.ToArray() );
				return;
			}

			if(data is IExternalizable)
			{
				WriteByte(AMF3TypeCode.Object);
				WriteAMF3Object(applicationContext, data);
				return;
			}

			if( data.GetType().IsArray )
			{
				WriteByte(AMF3TypeCode.Array);
				WriteAMF3Array(applicationContext, (Array)data);
				return;
			}

			if( data is IList )
			{
				//http://livedocs.macromedia.com/flex/2/docs/wwhelp/wwhimpl/common/html/wwhelp.htm?context=LiveDocs_Parts&file=00001104.html#270405
				//http://livedocs.macromedia.com/flex/2/docs/wwhelp/wwhimpl/common/html/wwhelp.htm?context=LiveDocs_Parts&file=00001105.html#268711

				if( _useLegacyCollection )
				{
					WriteByte(AMF3TypeCode.Array);
					WriteAMF3Array(applicationContext, data as IList);
				}
				else
				{
					WriteByte(AMF3TypeCode.Object);
					data = new ArrayCollection(data as IList);
					WriteAMF3Object(applicationContext, data);
				}
				return;
			}

			if(data is XmlDocument)
			{
				WriteByte(AMF3TypeCode.Xml);
				WriteAMF3XmlDocument(data as XmlDocument);
				return;
			}

			if(data is DataTable)
			{
				//TODO
				data = ConvertDataTableToASO( data as DataTable, false );
			}

			if(data is DataSet)
			{
				//TODO
				data = ConvertDataSetToASO( data as DataSet, false );
			}

			if(data is ASObject)
			{
				WriteByte(AMF3TypeCode.Object);
				WriteAMF3Object(applicationContext, data);
				return;
			}

			if(data is IDictionary)
			{
				//WriteByte(AMF3TypeCode.Array);
				//WriteAMF3AssociativeArray(applicationContext, data as IDictionary);
				WriteByte(AMF3TypeCode.Object);
				WriteAMF3Object(applicationContext, data);
				return;
			}

			if(data is Exception)
			{
				WriteByte(AMF3TypeCode.Object);
				WriteAMF3Object(applicationContext, new ExceptionASO(data as Exception));
				return;
			}

			//We have a custom type.
			WriteByte(AMF3TypeCode.Object);
			WriteAMF3Object(applicationContext, data);
		}

		public void WriteAMF3Null()
		{
			//Write the null code (0x1) to the output stream.
			WriteByte(AMF3TypeCode.Null);
		}

		public void WriteAMF3Bool(bool value)
		{
			WriteByte( (byte)(value ? AMF3TypeCode.BooleanTrue : AMF3TypeCode.BooleanFalse));
		}

		public void WriteAMF3Array(IApplicationContext applicationContext, Array array)
		{
			if( _amf0ObjectReferences.Contains( array ))
			{
				WriteReference(array);
				return;
			}

			if( !_objectReferences.Contains(array) )
			{
				_objectReferences.Add(array, _objectReferences.Count);
				int handle = array.Length;
				handle = handle << 1;
				handle = handle | 1;
				WriteAMF3IntegerData(handle);
				WriteAMF3String(string.Empty);//hash name
				for(int i = 0; i < array.Length; i++)
				{
					WriteAMF3Data(applicationContext, array.GetValue(i));
				}
			}
			else
			{
				int handle = (int)_objectReferences[array];
				handle = handle << 1;
				WriteAMF3IntegerData(handle);
			}
		}

		public void WriteAMF3Array(IApplicationContext applicationContext, IList value)
		{
			if( _amf0ObjectReferences.Contains( value ))//TODO ???
			{
				WriteReference(value);
				return;
			}

			if( !_objectReferences.Contains(value) )
			{
				_objectReferences.Add(value, _objectReferences.Count);
				int handle = value.Count;
				handle = handle << 1;
				handle = handle | 1;
				WriteAMF3IntegerData(handle);
				WriteAMF3String(string.Empty);//hash name
				for(int i = 0; i < value.Count; i++)
				{
					WriteAMF3Data(applicationContext, value[i]);
				}
			}
			else
			{
				int handle = (int)_objectReferences[value];
				handle = handle << 1;
				WriteAMF3IntegerData(handle);
			}
		}

		public void WriteAMF3AssociativeArray(IApplicationContext applicationContext, IDictionary value)
		{
			if( _amf0ObjectReferences.Contains( value ))//TODO ???
			{
				WriteReference(value);
				return;
			}

			if( !_objectReferences.Contains(value) )
			{
				_objectReferences.Add(value, _objectReferences.Count);
				WriteAMF3IntegerData(1);
				foreach(DictionaryEntry entry in value)
				{
					WriteAMF3String(entry.Key.ToString());
					WriteAMF3Data(applicationContext, entry.Value);
				}
				WriteAMF3String(string.Empty);
			}
			else
			{
				int handle = (int)_objectReferences[value];
				handle = handle << 1;
				WriteAMF3IntegerData(handle);
			}
		}

		private void WriteAMF3UTF(string str)
		{
			//Length - max 65536.
			UTF8Encoding utf8Encoding = new UTF8Encoding();
			int byteCount = utf8Encoding.GetByteCount(str);
			byte[] buffer = utf8Encoding.GetBytes(str);
			if (buffer.Length > 0)
				base.Write(buffer);
		}

		public void WriteAMF3String(string value)
		{
			if( value == string.Empty )
			{
				WriteAMF3IntegerData(1);
			}
			else
			{
				if( !_stringReferences.Contains(value) )
				{
					_stringReferences.Add(value, _stringReferences.Count);
					value = LineEndingConverter.Convert(value, LineEndingConverter.CR_STRING);
					UTF8Encoding utf8Encoding = new UTF8Encoding();
					int handle = utf8Encoding.GetByteCount(value);
					handle = handle << 1;
					handle = handle | 1;
					WriteAMF3IntegerData(handle);
					WriteAMF3UTF(value);
				}
				else
				{
					int handle = (int)_stringReferences[value];
					handle = handle << 1;
					WriteAMF3IntegerData(handle);
				}
			}
		}

		public void WriteAMF3DateTime(DateTime value)
		{
			if( !_objectReferences.Contains(value) )
			{
				_objectReferences.Add(value, _objectReferences.Count);
				int handle = 1;
				WriteAMF3IntegerData(handle);

				// Write date (milliseconds from 1970).
				DateTime timeStart = new DateTime(1970, 1, 1, 0, 0, 0);

				string timezoneCompensation = System.Configuration.ConfigurationManager.AppSettings["timezoneCompensation"];
				if( timezoneCompensation != null && ( timezoneCompensation.ToLower() == "auto" ) )
				{
					value = value.ToUniversalTime();
				}

				TimeSpan span = value.Subtract(timeStart);
				long milliSeconds = (long)span.TotalMilliseconds;
				long date = BitConverter.DoubleToInt64Bits((double)milliSeconds);
				this.WriteLong(date);
			}
			else
			{
				int handle = (int)_objectReferences[value];
				handle = handle << 1;
				WriteAMF3IntegerData(handle);
			}
		}

		private void WriteAMF3IntegerData(int value)
		{
			//Sign contraction - the high order bit of the resulting value must match every bit removed from the number
			//Clear 3 bits 
			value &= 0x1fffffff;
			if(value < 0x80)
				this.WriteByte(value);
			else
				if(value < 0x4000)
			{
					this.WriteByte(value >> 7 & 0x7f | 0x80);
					this.WriteByte(value & 0x7f);
			}
			else
				if(value < 0x200000)
			{
				this.WriteByte(value >> 14 & 0x7f | 0x80);
				this.WriteByte(value >> 7 & 0x7f | 0x80);
				this.WriteByte(value & 0x7f);
			} 
			else
			{
				this.WriteByte(value >> 22 & 0x7f | 0x80);
				this.WriteByte(value >> 15 & 0x7f | 0x80);
				this.WriteByte(value >> 8 & 0x7f | 0x80);
				this.WriteByte(value & 0xff);
			}
		}

		public void WriteAMF3Int(int value)
		{
			if(value >= -268435456 && value <= 268435455)//check valid range for 29bits
			{
				WriteByte(AMF3TypeCode.Integer);
				WriteAMF3IntegerData(value);
			}
			else
			{
				//overflow condition would occur upon int conversion
				WriteAMF3Double((double)value);
			}
		}

		public void WriteAMF3Double(double value)
		{
			WriteByte(AMF3TypeCode.Number);
			long tmp = BitConverter.DoubleToInt64Bits(value);
			this.WriteLong(tmp);
		}

		public void WriteAMF3XmlDocument(XmlDocument xmlDocument)
		{
			string value = xmlDocument.DocumentElement.OuterXml;
			//WriteAMF3String(xml);

			if( !_objectReferences.Contains(value) )
			{
				_objectReferences.Add(value, _objectReferences.Count);
				value = LineEndingConverter.Convert(value, LineEndingConverter.CR_STRING);
				UTF8Encoding utf8Encoding = new UTF8Encoding();
				int handle = utf8Encoding.GetByteCount(value);
				handle = handle << 1;
				handle = handle | 1;
				WriteAMF3IntegerData(handle);
				WriteAMF3UTF(value);
			}
			else
			{
				int handle = (int)_objectReferences[value];
				handle = handle << 1;
				WriteAMF3IntegerData(handle);
			}

		}

		public void WriteAMF3Object(IApplicationContext applicationContext, object value)
		{
			if( !_objectReferences.Contains(value) )
			{
				_objectReferences.Add(value, _objectReferences.Count);

				ClassDefinition classDefinition = GetClassDefinition(value);
				if( classDefinition != null )
				{//existing class-def

					//handle = classRef 0 1
					int handle = (int)_classDefinitionReferences[classDefinition];
					handle = handle << 2;
					handle = handle | 1;
					WriteAMF3IntegerData(handle);
				}
				else
				{//inline class-def
					
					classDefinition = CreateClassDefinition(applicationContext, value);
					//handle = memberCount dynamic externalizable 1 1
					int handle = classDefinition.MemberCount;
					handle = handle << 1;
					handle = handle | (classDefinition.IsDynamic ? 1 : 0);
					handle = handle << 1;
					handle = handle | (classDefinition.IsExternalizable ? 1 : 0);
					handle = handle << 2;
					handle = handle | 3;
					WriteAMF3IntegerData(handle);
					WriteAMF3String(classDefinition.ClassName);
					for(int i = 0; i < classDefinition.MemberCount; i++)
					{
						string key = CustomMemberMapper.Instance.ToFlash(classDefinition.ClassMemberDefinitions[i].ClassMember);
						WriteAMF3String(key);
					}
				}
				//write inline object
				if( classDefinition.IsExternalizable )
				{
					if( value is IExternalizable )
					{
						IExternalizable externalizable = value as IExternalizable;
						DataOutput dataOutput = new DataOutput(applicationContext, this);
						externalizable.WriteExternal(dataOutput);
					}
					else
						throw new FluorineException("Object does not implement IExternalizable.");
				}
				else
				{
					for(int i = 0; i < classDefinition.MemberCount; i++)
					{
						ClassMemberDefinition member = classDefinition.ClassMemberDefinitions[i];
						object propertyValue = classDefinition.GetValue(member, value);
						WriteAMF3Data(applicationContext, propertyValue);
						/*
						string key = classDefinition.ClassMemberDefinitions[i].ClassMember;
						PropertyInfo propertyInfo = value.GetType().GetProperty(key);
						if( propertyInfo != null )
						{
							object propertyValue = propertyInfo.GetValue(value, null);
							WriteAMF3Data(applicationContext, propertyValue);
						}
						else
						{
							FieldInfo fieldInfo = value.GetType().GetField(key);
							if( fieldInfo != null )
							{
								object propertyValue = fieldInfo.GetValue(value);
								WriteAMF3Data(applicationContext, propertyValue);
							}
							else
								throw new FluorineException("Class member " + key + " not found.");
						}
						*/
					}

					if(classDefinition.IsDynamic)
					{
						IDictionary dictionary = value as IDictionary;
						foreach(DictionaryEntry entry in dictionary)
						{
							WriteAMF3String(entry.Key.ToString());
							WriteAMF3Data(applicationContext, entry.Value);
						}
						WriteAMF3String(string.Empty);
					}
				}
			}
			else
			{
				//handle = objectRef 0
				int handle = (int)_objectReferences[value];
				handle = handle << 1;
				WriteAMF3IntegerData(handle);
			}
		}

		private ClassDefinition GetClassDefinition(object obj)
		{
			if( obj is ASObject )
			{
				ASObject asObject = obj as ASObject;
				if( asObject.IsTypedObject )
					return _classDefinitions[asObject.TypeName] as ClassDefinition;
				else
					return null;
			}
			else
			{
				return _classDefinitions[obj.GetType().FullName] as ClassDefinition;
			}
		}

		private ClassDefinition CreateClassDefinition(IApplicationContext applicationContext, object obj)
		{
			ArrayList classMemberDefinitions = null;
			ClassDefinition classDefinition = null;
			bool externalizable = obj.GetType().GetInterface(typeof(com.TheSilentGroup.Fluorine.AMF3.IExternalizable).FullName) != null;
			bool dynamic = false;
			string customClassName = null;
			if( obj is IDictionary )//ASObject, ObjectProxy, Hashtable, Dictionary
			{
				if( obj is ASObject && (obj as ASObject).IsTypedObject)//ASObject
				{
					ASObject asObject = obj as ASObject;
					classMemberDefinitions = new ArrayList(asObject.Count);
					foreach(DictionaryEntry entry in asObject)
					{
						classMemberDefinitions.Add(new ClassMemberDefinition(entry.Key as string));
					}
					customClassName = asObject.TypeName;
				}
				else
				{
					dynamic = true;
					customClassName = string.Empty;
					classMemberDefinitions = new ArrayList();
				}
				classDefinition = new ClassDefinition(customClassName, obj.GetType().FullName, classMemberDefinitions.Count, (ClassMemberDefinition[])classMemberDefinitions.ToArray(typeof(ClassMemberDefinition)), externalizable, dynamic);
				_classDefinitions[obj.GetType().FullName] = classDefinition;
				_classDefinitionReferences.Add(classDefinition, _classDefinitionReferences.Count);
			}
			else if( obj is IExternalizable )
			{
				customClassName = obj.GetType().FullName;
				if( applicationContext != null )
					customClassName = applicationContext.GetCustomClass(customClassName);

				classDefinition = new ClassDefinition(customClassName, obj.GetType().FullName, 0, new ClassMemberDefinition[0], true, false);
				_classDefinitions[obj.GetType().FullName] = classDefinition;
				_classDefinitionReferences.Add(classDefinition, _classDefinitionReferences.Count);
			}
			else
			{
				PropertyDescriptorCollection collection1;
				PropertyDescriptorCollection collection2;
				collection1 = TypeDescriptor.GetProperties(obj, new Attribute[1] { DesignerSerializationVisibilityAttribute.Visible }, false );
				//Specifies that a visual designer serialize the contents of this property, rather than the property itself
				collection2 = TypeDescriptor.GetProperties(obj, new Attribute[1] { DesignerSerializationVisibilityAttribute.Content }, false);
				FieldInfo[] fieldInfos = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
				
				classMemberDefinitions = new ArrayList(collection1.Count + collection2.Count + fieldInfos.Length);
				foreach (PropertyDescriptor descriptor in collection1)
				{
                    //B-Line Custom: Ignore fields/properties having TransientAttribute
                    if (!descriptor.IsReadOnly && !descriptor.Attributes.Matches(_transientAttribute))
					{
						classMemberDefinitions.Add(new ClassMemberDefinition(descriptor.Name));
					}
				}
				foreach (PropertyDescriptor descriptor in collection2)
				{
                    //B-Line Custom: Ignore fields/properties having TransientAttribute
                    if (!descriptor.IsReadOnly && !descriptor.Attributes.Matches(_transientAttribute))
					{
						classMemberDefinitions.Add(new ClassMemberDefinition(descriptor.Name));
					}
				}
				for(int j = 0; j < fieldInfos.Length; j++)
				{					
					FieldInfo fieldInfo = fieldInfos[j];
                    //B-Line Custom: Ignore fields/properties having TransientAttribute
                    if (fieldInfo.GetCustomAttributes(typeof(TransientAttribute), false).Length == 0)
                    {
                        classMemberDefinitions.Add(new ClassMemberDefinition(fieldInfo.Name));
                    }
				}
				customClassName = obj.GetType().FullName;
				if( applicationContext != null )
					customClassName = applicationContext.GetCustomClass(customClassName);

				classDefinition = new ClassDefinition(customClassName, obj.GetType().FullName, classMemberDefinitions.Count,(ClassMemberDefinition[])classMemberDefinitions.ToArray(typeof(ClassMemberDefinition)), externalizable, dynamic);
				_classDefinitions[obj.GetType().FullName] = classDefinition;
				_classDefinitionReferences.Add(classDefinition, _classDefinitionReferences.Count);
			}

			return classDefinition;
		}

		#endregion AMF3
	}
}
