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
using System.Web;
using System.Web.Caching;
// Import log4net classes.
using log4net;
using log4net.Config;

namespace com.TheSilentGroup.Fluorine.Configuration
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	public sealed class CacheMap
	{
		class CacheDescriptor
		{
			string		_name;
			int			_timeout;
			bool		_slidingExpiration;

			public CacheDescriptor(string name, int timeout, bool slidingExpiration)
			{
				_name = name;
				_timeout =  timeout;
				_slidingExpiration = slidingExpiration;
			}

			public string Name
			{
				get{ return _name; }
			}

			public int Timeout
			{
				get{ return _timeout; }
			}

			public bool SlidingExpiration
			{
				get{ return _slidingExpiration; }
			}
		}

		private ILog _log;
		private Hashtable _cacheMap;

		public CacheMap()
		{
			try
			{
				_log = LogManager.GetLogger(typeof(CacheMap));
			}
			catch{}
			_cacheMap = Hashtable.Synchronized(new Hashtable());
		}

		public void AddCacheDescriptor(string name, int timeout, bool slidingExpiration)
		{
			CacheDescriptor cacheDescriptor = new CacheDescriptor(name, timeout, slidingExpiration);
			_cacheMap[name] = cacheDescriptor;
			if( _log != null && _log.IsDebugEnabled )
			{
				string msg = "Loading key: " + name + " to cache map, timeout: " + timeout + " sliding expiration: " + slidingExpiration;
				_log.Debug(msg);
			}
		}

		public bool ContainsCacheDescriptor(string name)
		{
			if( name != null )
				return _cacheMap.Contains(name);
			return false;
		}

		public int Count
		{
			get{ return _cacheMap.Count; }
		}

		public bool ContainsValue(string cacheKey)
		{
			return HttpContext.Current.Cache.Get(cacheKey) != null;
		}

		public object Get(string cacheKey)
		{
			object value = HttpContext.Current.Cache.Get(cacheKey);
			if( value != null )
			{
				if( _log != null && _log.IsDebugEnabled )
				{
					string msg = "Cache hit, name: " + cacheKey;
					_log.Debug(msg);
				}
			}
			else
			{
				if( _log != null && _log.IsDebugEnabled )
				{
					string msg = "Cache miss, name: " + cacheKey;
					_log.Debug(msg);
				}
			}
			return value;
		}

		public object Add(string source, string cacheKey, object value)
		{
			if( _cacheMap.Contains( source ) )
			{
				if( _log != null && _log.IsDebugEnabled )
				{
					string msg = "Add to ASP.NET cache name: " + source + " key: " + cacheKey;
					_log.Debug(msg);
				}

				CacheDescriptor cacheDescriptor = _cacheMap[source] as CacheDescriptor;
				if( !cacheDescriptor.SlidingExpiration )
					return HttpContext.Current.Cache.Add(cacheKey, value, null, DateTime.Now.AddMinutes(cacheDescriptor.Timeout), TimeSpan.Zero, CacheItemPriority.Default, null);
				else
					return HttpContext.Current.Cache.Add(cacheKey, value, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(cacheDescriptor.Timeout), CacheItemPriority.Default, null);
			}
			if( _log != null && _log.IsDebugEnabled )
			{
				string msg = "Cannot add to ASP.NET cache the name: " + source + " key: " + cacheKey + ". Check your web.config file.";
				_log.Debug(msg);
			}
			return null;
		}

		internal static string GenerateCacheKey(string source, IList arguments)
		{
			int argumentsHashCode = com.TheSilentGroup.Fluorine.Data.ListHashCodeProvider.GenerateHashCode(arguments);
			string key = source + "_" + argumentsHashCode.ToString();
			return key;
		}
	}
}
