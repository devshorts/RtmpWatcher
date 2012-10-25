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
using System.Configuration;
using System.Web;
// Import log4net classes.
using log4net;
using log4net.Config;

using com.TheSilentGroup.Fluorine.Gateway;
using com.TheSilentGroup.Fluorine.Messaging;
using com.TheSilentGroup.Fluorine.Messaging.Messages;

namespace com.TheSilentGroup.Fluorine.Filter
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class CacheFilter : AbstractFilter
	{
		private ILog _log;

		public CacheFilter()
		{
			try
			{
				_log = LogManager.GetLogger(typeof(CacheFilter));
			}
			catch{}
		}

		#region IFilter Members

		public override void Invoke(AMFContext context)
		{
			MessageOutput messageOutput = context.MessageOutput;
			if( context.ApplicationContext != null && context.ApplicationContext.CacheMap != null && context.ApplicationContext.CacheMap.Count > 0 )
			{
				for(int i = 0; i < context.AMFMessage.BodyCount; i++)
				{
					AMFBody amfBody = context.AMFMessage.GetBodyAt(i);
					//Check if response exists.
					ResponseBody responseBody = messageOutput.GetResponse(amfBody);
					if( responseBody != null )
					{
						//AuthenticationFilter may insert response.
						continue;
					}

					if( !amfBody.IsEmptyTarget )
					{
						string source = amfBody.Target;
						IList arguments = amfBody.GetParameterList();
						string key = com.TheSilentGroup.Fluorine.Configuration.CacheMap.GenerateCacheKey(source, arguments);
						//Flash message
						if( context.ApplicationContext.CacheMap.ContainsValue(key) )
						{
							object cachedContent = context.ApplicationContext.CacheMap.Get(key);

							if( _log != null && _log.IsDebugEnabled )
								_log.Debug("Response to the request " + amfBody.Target + " retrieved from cache with key = " + key);
							
							CachedBody cachedBody = new CachedBody(amfBody, cachedContent);
							messageOutput.AddBody(cachedBody);
						}
					}
					else
					{
						//Flex message
						object content = amfBody.Content;
						if( content is IList )
							content = (content as IList)[0];
						IMessage message = content as IMessage;
							
						if( message != null )
						{
							//Only RemotingMessages for now
							string source = message.GetMessageSource();

							if( source != null )
							{
								IList arguments = amfBody.GetParameterList();
								string key = com.TheSilentGroup.Fluorine.Configuration.CacheMap.GenerateCacheKey(source, arguments);
								if( context.ApplicationContext.CacheMap.ContainsValue(key) )
								{
									object cachedContent = context.ApplicationContext.CacheMap.Get(key);

									if( _log != null && _log.IsDebugEnabled )
										_log.Debug("Response to the request " + source + " retrieved from cache with key = " + key);
							
									CachedBody cachedBody = new CachedBody(amfBody, message, cachedContent);
									messageOutput.AddBody(cachedBody);
								}
							}
						}
					}
				}
			}
		}

		#endregion

	}
}
