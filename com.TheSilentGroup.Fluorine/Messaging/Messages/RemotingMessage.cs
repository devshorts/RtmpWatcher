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
using System.Text;

namespace com.TheSilentGroup.Fluorine.Messaging.Messages
{
	/// <summary>
	/// RemotingMessages are used to send RPC requests to a remote endpoint. These messages use the operation property to specify which method to call on the remote object. The destination property indicates what object/service should be used.
	/// </summary>
	public class RemotingMessage : RPCMessage
	{
		string _source;
		string _operation;
		/// <summary>
		/// Initializes a new instance of the RemotingMessage class.
		/// </summary>
		public RemotingMessage()
		{
		}
		/// <summary>
		/// Gets or sets the underlying source of a RemoteObject destination.
		/// </summary>
		/// <remarks>
		/// This property is provided for backwards compatibility. The best practice, however, is 
		/// to not expose the underlying source of a RemoteObject destination on the client 
		/// and only one source to a destination. Some types of Remoting Services may even ignore 
		/// this property for security reasons.
		/// </remarks>
		public string source
		{
			get{ return _source; }
			set{ _source = value; }
		}
		/// <summary>
		/// Gets or sets the name of the remote method/operation that should be called.
		/// </summary>
		public string operation
		{
			get{ return _operation; }
			set{ _operation = value; }
		}

		public override string GetMessageSource()
		{
			if( _source != null && _source != string.Empty && _operation != null && _operation != string.Empty)
				return _source + "." + _operation;
			return null;
		}
	}
}
