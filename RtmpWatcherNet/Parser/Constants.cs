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
namespace RtmpWatcherNet.Parser
{
	public enum ObjectEncoding
	{
		AMF0 = 0,
		AMF3 = 3
	}


	class AMF0TypeCode
	{
		public const int Number = 0;
		public const int Boolean = 1;
		public const int String = 2;
		public const int ASObject = 3;
		public const int Null = 5;
		public const int Reference = 7;
		public const int AssociativeArray = 8;
		public const int Array = 10;
		public const int DateTime = 11;
		public const int LongString = 12;
		public const int Xml = 15;
		public const int CustomClass = 16;
		public const int AMF3Tag = 17;
	}

	class AMF3TypeCode
	{
		public const int Undefined = 0;
		public const int Null = 1;
		public const int BooleanFalse = 2;
		public const int BooleanTrue = 3;
		public const int Integer = 4;
		public const int Number = 5;
		public const int String = 6;
		public const int DateTime = 8;
		public const int Array = 9;
		public const int Object = 10;
		public const int Xml = 11;
		public const int Xml2 = 7;
		public const int ByteArray = 12;
	}

}
