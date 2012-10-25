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
using System.Collections.Specialized;

using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api;
using com.TheSilentGroup.Fluorine.SystemHelpers;

namespace com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp
{
	/// <summary>
	/// Summary description for AttributeStore.
	/// </summary>
	public class AttributeStore : DisposableBase, IAttributeStore
	{
		/// <summary>
		/// Prefix for attribute names that should not be made persistent. 
		/// </summary>
		public static string TransientPrefix = "_transient";

		Hashtable _attributes;

		public AttributeStore()
		{
			_attributes = new Hashtable();
		}

		#region IAttributeStore Members

		public ICollection GetAttributeNames()
		{
			return _attributes.Keys;
		}

		public virtual bool SetAttribute(string name, object value)
		{
			if(name == null )
				return false;
			_attributes[name] = value;
			return true;
		}

		public virtual void SetAttributes(StringDictionary values)
		{
			foreach(DictionaryEntry entry in values)
			{
				SetAttribute(entry.Key as string, entry.Value);
			}
		}

		public virtual void SetAttributes(IAttributeStore values)
		{
			foreach(string name in values.GetAttributeNames())
			{
				object value = values.GetAttribute(name);
				SetAttribute(name, value);
			}
		}

		public object GetAttribute(string name)
		{
			return _attributes[name];
		}

		public object GetAttribute(string name, object defaultValue)
		{
			if( _attributes.ContainsKey(name) )
				return _attributes[name];
			else
				return defaultValue;
		}

		public bool HasAttribute(string name)
		{
			return _attributes.ContainsKey(name);
		}

		public virtual bool RemoveAttribute(string name)
		{
			if( HasAttribute(name) )
			{
				_attributes.Remove(name);
				return true;
			}
			return false;
		}

		public virtual void RemoveAttributes()
		{
			_attributes.Clear();
		}

		#endregion
	}
}
