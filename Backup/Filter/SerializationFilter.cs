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

using com.TheSilentGroup.Fluorine.Gateway;

namespace com.TheSilentGroup.Fluorine.Filter
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class SerializationFilter : AbstractFilter
	{
		bool _useLegacyCollection = false;
		/// <summary>
		/// Initializes a new instance of the SerializationFilter class.
		/// </summary>
		public SerializationFilter()
		{
		}

		public bool UseLegacyCollection
		{
			get{ return _useLegacyCollection; }
			set{ _useLegacyCollection = value; }
		}

		#region IFilter Members

		public override void Invoke(AMFContext context)
		{
			AMFSerializer serializer = new AMFSerializer(context.ApplicationContext.OutputStream);
			serializer.UseLegacyCollection = _useLegacyCollection;
			serializer.WriteMessage(context.ApplicationContext, context.MessageOutput);
			serializer.Flush();

			/*
			MemoryStream memoryStream = new MemoryStream();
			AMFSerializer serializer = new AMFSerializer(memoryStream);
			serializer.UseLegacyCollection = _useLegacyCollection;
			serializer.WriteMessage(context.ApplicationContext, context.MessageOutput);
			serializer.Flush();
			memoryStream.SetLength(memoryStream.Position);
			byte[] buffer = memoryStream.ToArray();
			context.ApplicationContext.OutputStream.Write( buffer, 0, buffer.Length);
			*/

			//TEST only
			//TestSerialization(context);
		}

		private void TestSerialization(AMFContext context)
		{
			MemoryStream ms = new MemoryStream();
			AMFSerializer testSerializer = new AMFSerializer(ms);
			testSerializer.WriteMessage(context.ApplicationContext, context.MessageOutput);
			testSerializer.Flush();
			ms.Position = 0;
			AMFDeserializer testDeserializer = new AMFDeserializer(ms);
			AMFMessage amfMessageOut = testDeserializer.ReadAMFMessage(context.ApplicationContext);
		}

		#endregion
	}
}
