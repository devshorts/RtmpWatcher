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

namespace com.TheSilentGroup.Fluorine.Configuration
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	public sealed class ClassMappings
	{
		private Hashtable _typeToCustomClass;
		private Hashtable _customClassToType;

		public ClassMappings()
		{
			_typeToCustomClass = new Hashtable();
			_customClassToType = new Hashtable();

			Add("com.TheSilentGroup.Fluorine.AMF3.ArrayCollection", "flex.messaging.io.ArrayCollection");
			Add("com.TheSilentGroup.Fluorine.AMF3.ByteArray", "flex.messaging.io.ByteArray");
			Add("com.TheSilentGroup.Fluorine.AMF3.ObjectProxy", "flex.messaging.io.ObjectProxy");

			//FDS
			Add("com.TheSilentGroup.Fluorine.Messaging.Messages.CommandMessage", "flex.messaging.messages.CommandMessage");
			Add("com.TheSilentGroup.Fluorine.Messaging.Messages.RemotingMessage", "flex.messaging.messages.RemotingMessage");
			Add("com.TheSilentGroup.Fluorine.Messaging.Messages.AsyncMessage", "flex.messaging.messages.AsyncMessage");
			Add("com.TheSilentGroup.Fluorine.Messaging.Messages.AcknowledgeMessage", "flex.messaging.messages.AcknowledgeMessage");
			Add("com.TheSilentGroup.Fluorine.Data.Messages.DataMessage", "flex.data.messages.DataMessage");
			Add("com.TheSilentGroup.Fluorine.Data.Messages.PagedMessage", "flex.data.messages.PagedMessage");
			Add("com.TheSilentGroup.Fluorine.Data.Messages.UpdateCollectionMessage", "flex.data.messages.UpdateCollectionMessage");
			Add("com.TheSilentGroup.Fluorine.Data.Messages.SequencedMessage", "flex.data.messages.SequencedMessage");
			Add("com.TheSilentGroup.Fluorine.Messaging.Messages.ErrorMessage", "flex.messaging.messages.ErrorMessage");
			Add("com.TheSilentGroup.Fluorine.Messaging.Messages.RemotingMessage", "flex.messaging.messages.RemotingMessage");
			Add("com.TheSilentGroup.Fluorine.Messaging.Messages.RPCMessage", "flex.messaging.messages.RPCMessage");			

			Add("com.TheSilentGroup.Fluorine.Data.UpdateCollectionRange", "flex.data.UpdateCollectionRange");			
			
			Add("com.TheSilentGroup.Fluorine.Messaging.Services.RemotingService", "flex.messaging.services.RemotingService");
			Add("com.TheSilentGroup.Fluorine.Messaging.Services.MessageService", "flex.messaging.services.MessageService");
			Add("com.TheSilentGroup.Fluorine.Data.DataService", "flex.data.DataService");

			
			Add("com.TheSilentGroup.Fluorine.Messaging.Services.Remoting.DotNetAdapter", "flex.messaging.services.remoting.adapters.JavaAdapter");
		}

		public void Add(string type, string customClass)
		{
			_typeToCustomClass[type] = customClass;
			_customClassToType[customClass] = type;
		}

		public string GetCustomClass(string type)
		{
			if( _typeToCustomClass.Contains( type ) )
				return _typeToCustomClass[type] as string;
			else
				return type;
		}

		public string GetType(string customClass)
		{
			if( customClass == null )
				return null;
			if( _customClassToType.Contains(customClass) )
				return _customClassToType[customClass] as string;
			else
				return customClass;
		}
	}
}
