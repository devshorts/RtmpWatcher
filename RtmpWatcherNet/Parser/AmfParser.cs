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
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RtmpWatcherNet.Parser
{
    /// <summary>
    /// AMFReader reads binary data from the input stream.<br/>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public class AMFReader : BinaryReader
    {
     
        Hashtable _amf0ObjectReferences;
        Hashtable _objectReferences;
        Hashtable _stringReferences;
        Hashtable _classDefinitions;

        /// <summary>
        /// Initializes a new instance of the AMFReader class based on the supplied stream and using UTF8Encoding.
        /// </summary>
        /// <param name="stream"></param>
        public AMFReader(Stream stream)
            : base(stream)
        {
            
            Reset();
        }

        public void Reset()
        {
            _amf0ObjectReferences = new Hashtable(5);
            _objectReferences = new Hashtable(15);
            _stringReferences = new Hashtable(15);
            _classDefinitions = new Hashtable(2);
        }


        public object ReadData()
        {
            int typeCode = this.BaseStream.ReadByte();
            return this.ReadData(typeCode);
        }

        /// <summary>
        /// Maps a type code to an access method.
        /// </summary>
        /// <param name="typeCode"></param>
        /// <param name="applicationContext"></param>
        /// <returns></returns>
        internal object ReadData(int typeCode)
        {
            object obj = null;
            switch (typeCode)
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
                    return ReadASObject();
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
                    return ReadAssociativeArray();
                case 9:
                    throw new UnexpectedAMF();
                // Array
                case AMF0TypeCode.Array:
                    IList list = ReadArray();
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
                    throw new UnexpectedAMF();
                // Custom Class
                case AMF0TypeCode.CustomClass:
                    {
                        //We have a custom type.
                        object amfObject = ReadObject();
                        return amfObject;
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
            for (int i = 7, j = 0; i >= 0; i--, j++)
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
            for (int i = 3, j = 0; i >= 0; i--, j++)
            {
                invertedBytes[j] = bytes[i];
            }
            float value = BitConverter.ToSingle(invertedBytes, 0);
            return value;
        }

        public object ReadObject( )
        {
            var container = new Container();

            string typeIdentifier = ReadString();

            container.Name = typeIdentifier;
            container.Values = new List<Container>();

            string key = ReadString();

            var subContainer = new Container();
            subContainer.Name = key;

            for (int typeCode = ReadByte(); typeCode != 9; typeCode = ReadByte())
            {
                object value = ReadData(typeCode);

                subContainer.Value = value;

                container.Values.Add(subContainer);

                subContainer = new Container();

                key = ReadString();

                subContainer.Name = key;

                Console.WriteLine("{0} - {1}", key, value);
            }

            return container;
        }

        public ASObject ReadASObject()
        {
            ASObject asObject = new ASObject();
            _amf0ObjectReferences.Add(_amf0ObjectReferences.Count, asObject);
            string key = this.ReadString();
            for (int typeCode = this.BaseStream.ReadByte(); typeCode != 9; typeCode = this.BaseStream.ReadByte())
            {
                asObject.Add(key, this.ReadData(typeCode));
                key = this.ReadString();
            }
            return asObject;
        }


        public string ReadUTF(int length)
        {
            if (length == 0)
                return string.Empty;
            UTF8Encoding utf8 = new UTF8Encoding(false, true);
            byte[] encodedBytes = this.ReadBytes(length);
            try
            {
                string decodedString = utf8.GetString(encodedBytes);
                return LineEndingConverter.Convert(decodedString, Environment.NewLine);
            }
            catch (DecoderFallbackException)
            {
                
                throw;
            }
        }

        public string ReadLongUTFString()
        {
            int length = this.ReadInt32();
            return this.ReadUTF(length);
        }

        private Hashtable ReadAssociativeArray()
        {
            // Get the length property set by flash.
            int length = this.ReadInt32();
            Hashtable result = new Hashtable(length);
            _amf0ObjectReferences.Add(_amf0ObjectReferences.Count, result);
            string key = ReadString();
            for (int typeCode = this.BaseStream.ReadByte(); typeCode != 9; typeCode = this.BaseStream.ReadByte())
            {
                object value = ReadData(typeCode);
                result.Add(key, value);
                key = ReadString();
            }
            return result;
        }

        private IList ReadArray( )
        {
            //Get the length of the array.
            int length = ReadInt32();
            //object[] array = new object[length];
            ArrayList array = new ArrayList(length);
            _amf0ObjectReferences.Add(_amf0ObjectReferences.Count, array);
            for (int i = 0; i < length; i++)
            {
                //array[i] = ReadData(typeCode, applicationContext);
                array.Add(ReadData());
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
            if (tmp > 720)
            {
                tmp = (65536 - tmp);
            }
            

            return date;
        }



    }
}
