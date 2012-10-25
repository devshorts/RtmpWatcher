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

using com.TheSilentGroup.Fluorine;
using com.TheSilentGroup.Fluorine.Messaging;
using com.TheSilentGroup.Fluorine.Messaging.Messages;

namespace com.TheSilentGroup.Fluorine.Data
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	sealed class MessageContext
	{
		IApplicationContext		_applicationContext;
		Stack					_incomingMessageStack;
		ArrayList				_outgoingMessageStack;

		public MessageContext(IApplicationContext applicationContext)
		{
			_applicationContext = applicationContext;
			_incomingMessageStack = new Stack(1);
			_outgoingMessageStack = new ArrayList();
		}

		public IApplicationContext ApplicationContext
		{
			get{ return _applicationContext; }
		}

		public IMessage GetIncomingMessage(string messageId)
		{
			foreach(IMessage message in _incomingMessageStack)
			{
				if(message.messageId == messageId)
					return message;
			}
			return null;
		}

		public void PushIncomingMessage(IMessage message)
		{
			_incomingMessageStack.Push(message);
		}

		public void PushOutgoingMessage(IMessage message)
		{
			_outgoingMessageStack.Add(message);
		}

		public IMessage[] GetOutgoingMessages()
		{
			return _outgoingMessageStack.ToArray(typeof(IMessage)) as IMessage[];
		}

		public void ClearOutgoingMessages()
		{
			_outgoingMessageStack.Clear();
		}
	}
}
