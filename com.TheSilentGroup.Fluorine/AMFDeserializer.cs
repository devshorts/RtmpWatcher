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

using com.TheSilentGroup.Fluorine.Configuration;

namespace com.TheSilentGroup.Fluorine
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	public class AMFDeserializer : AMFReader
	{
		/// <summary>
		/// Initializes a new instance of the AMFDeserializer class.
		/// </summary>
		/// <param name="stream"></param>
		public AMFDeserializer(Stream stream) : base(stream)
		{
		}

		public AMFMessage ReadAMFMessage()
		{
			return ReadAMFMessage(null);
		}

		public AMFMessage ReadAMFMessage(IApplicationContext applicationContext)
		{
			// Version stored in the first two bytes.
			ushort version = base.ReadUInt16();
			AMFMessage message = new AMFMessage(version);
			// Read header count.
			int headerCount = base.ReadUInt16();
			for (int i = 0; i < headerCount; i++)
			{
				message.AddHeader(this.ReadHeader(applicationContext));
			}
			// Read header count.
			int bodyCount = base.ReadUInt16();
			for (int i = 0; i < bodyCount; i++)
			{
				AMFBody amfBody = this.ReadBody(applicationContext);
				if( amfBody != null )//not failed
					message.AddBody(amfBody);
			}
			return message;
		}

		private AMFHeader ReadHeader(IApplicationContext applicationContext)
		{
			// Read name.
			string name = base.ReadString();
			// Read must understand flag.
			bool mustUnderstand = base.ReadBoolean();
			// Read the length of the header.
			int length = base.ReadInt32();
			// Read content.
			object content = base.ReadData(applicationContext);
			return new AMFHeader(name, mustUnderstand, content);
		}

		private AMFBody ReadBody(IApplicationContext applicationContext)
		{
			this.Reset();
			string target = base.ReadString();

			// Response that the client understands.
			string response = base.ReadString();
			int length = base.ReadInt32();
			if( base.BaseStream.CanSeek )
			{
				long position = base.BaseStream.Position;
				// Read content.
				try
				{
					object content = base.ReadData(applicationContext);
					return new AMFBody(target, response, content);
				}
				catch(Exception exception)
				{
					base.BaseStream.Position = position + length;
					if( applicationContext != null )
						applicationContext.Fail( new AMFBody(target, response, null), exception);
					return null;
				}
			}
			else
			{
				object content = base.ReadData(applicationContext);
				return new AMFBody(target, response, content);
			}
		}
	}
}
