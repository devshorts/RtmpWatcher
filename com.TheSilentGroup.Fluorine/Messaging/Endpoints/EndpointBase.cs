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
using System.Text;
using System.Collections;
using System.Security;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using System.Web.Configuration;
using System.Web.Caching;
using System.Threading;

using com.TheSilentGroup.Fluorine.Messaging;
using com.TheSilentGroup.Fluorine.Messaging.Config;
using com.TheSilentGroup.Fluorine.Messaging.Messages;
using com.TheSilentGroup.Fluorine.Messaging.Services;
using com.TheSilentGroup.Fluorine.Messaging.Security;

namespace com.TheSilentGroup.Fluorine.Messaging.Endpoints
{
	/// <summary>
	/// Summary description for EndpointBase.
	/// </summary>
	internal class EndpointBase : IEndpoint
	{
		protected MessageBroker _messageBroker;
		protected ChannelSettings _channelSettings;
		string _id;

		public EndpointBase(MessageBroker messageBroker, ChannelSettings channelSettings)
		{
			_messageBroker = messageBroker;
			_channelSettings = channelSettings;
			_id = _channelSettings.Id;
		}

		#region IEndpoint Members

		public string Id
		{
			get
			{
				return _id;
			}
			set
			{
				_id = value;
			}
		}

		public MessageBroker GetMessageBroker()
		{
			return _messageBroker;
		}

		public ChannelSettings GetSettings()
		{
			return _channelSettings;
		}

		public virtual void Start()
		{
		}

		public virtual void Stop()
		{
		}

		public virtual void Push(IMessage message, MessageClient messageclient)
		{
			throw new NotSupportedException();
		}

		public virtual void Service(IApplicationContext applicationContext)
		{
		}

		public virtual bool IsSecure()
		{
			return false;
		}

		#endregion

		public virtual IMessage ServiceMessage(IApplicationContext applicationContext, IMessage message)
		{
			IMessage response = null;
			response = _messageBroker.RouteMessage(applicationContext, message, this);
			return response;
		}

	}
}
