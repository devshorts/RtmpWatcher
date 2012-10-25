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

using com.TheSilentGroup.Fluorine.Messaging.Config;
using com.TheSilentGroup.Fluorine.Messaging.Messages;

namespace com.TheSilentGroup.Fluorine.Messaging.Services
{
	/// <summary>
	/// Summary description for ServiceBase.
	/// </summary>
	internal class ServiceBase : IService
	{
		protected MessageBroker			_messageBroker;
		protected ServiceSettings		_serviceSettings;
		protected Hashtable				_destinations;
		object							_objLock = new object();
		protected Destination			_defaultDestination;

		public ServiceBase(IApplicationContext applicationContext, MessageBroker messageBroker, ServiceSettings serviceSettings)
		{
			_messageBroker = messageBroker;
			_serviceSettings = serviceSettings;

			_destinations = new Hashtable();
			foreach(DictionaryEntry entry in serviceSettings.DestinationSettings)
			{
				DestinationSettings destinationSettings = entry.Value as DestinationSettings;
				CreateDestination(applicationContext, destinationSettings);
			}
		}

		protected virtual Destination NewDestination(DestinationSettings destinationSettings)
		{
			return new Destination(this, destinationSettings);
		}

		public ServiceSettings ServiceSettings{ get { return _serviceSettings; } }

		#region IService Members

		public string id
		{
			get
			{
				return _serviceSettings["id"] as string;
			}
			set
			{
				_serviceSettings["id"] = value;
			}
		}

		public MessageBroker GetMessageBroker()
		{
			return _messageBroker;
		}

		public Destination GetDestination(IMessage message)
		{
			lock(_objLock)
			{
				return _destinations[message.destination] as Destination;
			}
		}

		public Destination GetDestinationWithSource(string source)
		{
			lock(_objLock)
			{
				foreach(Destination destination in _destinations.Values)
				{
					string sourceTmp = destination.DestinationSettings.Properties["source"] as string;
					if( source == sourceTmp )
						return destination;
				}
				return null;
			}
		}

		public Destination DefaultDestination
		{
			get{ return _defaultDestination; }
		}

		public Destination GetDestination(string id)
		{
			lock(_objLock)
			{
				return _destinations[id] as Destination;
			}
		}

		public virtual object ServiceMessage(IApplicationContext applicationContext, IMessage message)
		{
			CommandMessage commandMessage = message as CommandMessage;
			if( commandMessage != null && commandMessage.operation == CommandMessage.ClientPingOperation )
				return true;
			throw new NotSupportedException();
		}

		public bool IsSupportedMessage(IApplicationContext applicationContext, IMessage message)
		{
			return IsSupportedMessageType(applicationContext, message.GetType().FullName);
		}

		public bool IsSupportedMessageType(IApplicationContext applicationContext, string messageClassName)
		{
			bool result = _serviceSettings.SupportedMessageTypes.Contains(messageClassName);
			if(!result)
			{
				//Check whether this type is mapped				
				string typeName = applicationContext.ClassMappings.GetCustomClass(messageClassName);
				return _serviceSettings.SupportedMessageTypes.Contains(typeName);
			}
			return result;
		}

		public void Start()
		{
		}

		public void Stop()
		{
		}

		#endregion

		public virtual Destination CreateDestination(IApplicationContext applicationContext, DestinationSettings destinationSettings)
		{
			lock(_objLock)
			{
				if( !_destinations.ContainsKey(destinationSettings.Id) )
				{
					Destination destination = NewDestination(destinationSettings);
					if( destinationSettings.AdapterSettings != null )
						destination.Init(applicationContext, destinationSettings.AdapterSettings);
					else
						destination.Init(applicationContext, _serviceSettings.DefaultAdapterSettings);
					_destinations[destination.Id] = destination;
					
					string source = destination.DestinationSettings.Properties["source"] as string;
					//TODO: warn if more then one "*" source occurs.
					if( source != null && source == "*" )
						_defaultDestination = destination;
					return destination;
				}
				else
					return _destinations[destinationSettings.Id] as Destination;
			}
		}

		public virtual void CheckSecurity(IApplicationContext applicationContext, IMessage message)
		{
			CheckSecurity(applicationContext, this.GetDestination(message));
		}

		public virtual void CheckSecurity(IApplicationContext applicationContext, Destination destination)
		{
			if( destination == null )
				throw new UnauthorizedAccessException("Invalid Destination");
			if( destination.DestinationSettings != null && destination.DestinationSettings.SecuritySettings != null )
			{
				SecuritySettings securitySettings = destination.DestinationSettings.SecuritySettings;
				string[] roles = securitySettings.GetRoles();
				if( roles != null && roles.Length > 0 )
				{
					bool authorized = DoAuthorization(applicationContext, roles);
					if( !authorized )
						throw new UnauthorizedAccessException("Requested access is not allowed.");
				}
			}
		}

		public bool DoAuthorization(IApplicationContext applicationContext, string[] roles)
		{
			if( applicationContext.User == null )
				throw new UnauthorizedAccessException("Not a valid Principal object.");
			if( _messageBroker == null )
				throw new NullReferenceException("RemotingAdapter: MessageBroker not found.");
			if( _messageBroker.LoginCommand != null )
			{
				bool authorized = _messageBroker.LoginCommand.DoAuthorization( applicationContext.User, roles);
				return authorized;
			}
			else
			{
				throw new UnauthorizedAccessException("LoginCommand not found. Security information for the current request was not available." );
			}		
		}
	}
}
