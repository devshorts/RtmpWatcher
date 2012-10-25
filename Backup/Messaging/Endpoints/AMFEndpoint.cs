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
using System.Web;

using com.TheSilentGroup.Fluorine;
using com.TheSilentGroup.Fluorine.Gateway;
using com.TheSilentGroup.Fluorine.Filter;
using com.TheSilentGroup.Fluorine.Configuration;

using com.TheSilentGroup.Fluorine.Messaging;
using com.TheSilentGroup.Fluorine.Messaging.Config;
using com.TheSilentGroup.Fluorine.Messaging.Messages;
using com.TheSilentGroup.Fluorine.Messaging.Endpoints;

//namespace com.TheSilentGroup.Fluorine.Messaging.Endpoints
namespace flex.messaging.endpoints
{
	/// <summary>
	/// Summary description for AMFEndpoint.
	/// </summary>
	internal class AMFEndpoint : EndpointBase
	{
		FilterChain _filterChain;
		const string LegacyCollectionKey = "legacy-collection";

		public AMFEndpoint(MessageBroker messageBroker, ChannelSettings channelSettings):base(messageBroker, channelSettings)
		{
		}

		public override void Start()
		{
			DeserializationFilter deserializationFilter = new DeserializationFilter();
			deserializationFilter.UseLegacyCollection = GetIsLegacyCollection();
			ServiceMapFilter serviceMapFilter = new ServiceMapFilter(this);
			WsdlFilter wsdlFilter = new WsdlFilter();
			AccessFilter accessFilter = new AccessFilter();
			DescribeServiceFilter describeServiceFilter = new DescribeServiceFilter();
			AuthenticationFilter authenticationFilter = new AuthenticationFilter(this);
			CacheFilter cacheFilter = new CacheFilter();
			ProcessFilter processFilter = new ProcessFilter(this);
			MessageFilter messageFilter = new MessageFilter(this);
			DebugFilter debugFilter = new DebugFilter();
			SerializationFilter serializationFilter = new SerializationFilter();
			serializationFilter.UseLegacyCollection = GetIsLegacyCollection();
			
			deserializationFilter.Next = serviceMapFilter;
			serviceMapFilter.Next = wsdlFilter;
			wsdlFilter.Next = accessFilter;
			accessFilter.Next = describeServiceFilter;
			describeServiceFilter.Next = authenticationFilter;
			authenticationFilter.Next = cacheFilter;
			cacheFilter.Next = processFilter;
			processFilter.Next = debugFilter;
			debugFilter.Next = messageFilter;
			messageFilter.Next = serializationFilter;

			_filterChain = new FilterChain(deserializationFilter);
		}

		public bool GetIsLegacyCollection()
		{
			if( !_channelSettings.Contains(LegacyCollectionKey) )
				return false;
			string value = _channelSettings[LegacyCollectionKey] as string;
			bool isLegacy = Convert.ToBoolean(value);
			return isLegacy;
		}

		public override void Stop()
		{
			_filterChain = null;
			base.Stop();
		}

		public override void Service(IApplicationContext applicationContext)
		{
			AMFContext amfContext = new AMFContext( applicationContext );
			_filterChain.InvokeFilters( amfContext );
		}
	}
}
