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
using System.IO;
using System.Collections.Specialized;

using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api;
using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api.Persistence;

namespace com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp
{
	/// <summary>
	/// Summary description for PersistableAttributeStore.
	/// </summary>
	public class PersistableAttributeStore : AttributeStore, IPersistable
	{
		protected bool		_persistent = true;
		protected string	_name;
		protected string	_type;
		protected string	_path;
		protected long		_lastModified = -1;
		protected IPersistenceStore _store = null;

		public PersistableAttributeStore(string type, string name, string path, bool persistent)
		{
			_type = type;
			_name = name;
			_path = path;
			_persistent = persistent;
		}

		#region IPersistable Members

		public bool IsPersistent
		{
			get{ return _persistent; }
			set{ _persistent = value; }
		}

		public virtual string Name
		{
			get{ return _name; }
			set{ _name = value; }
		}

		public string Type
		{
			get{ return _type; }
		}

		public virtual string Path
		{
			get{ return _path; }
			set{ _path = value; }
		}

		public long LastModified
		{
			get{ return _lastModified; }
		}

		public IPersistenceStore Store
		{
			get{ return _store; }
			set
			{
				_store = value;
				if( _store != null )
					_store.Load(this);
			}
		}

		public void Serialize(Stream stream)
		{
			// TODO:  Add PersistableAttributeStore.Serialize implementation
		}

		public void Deserialize(Stream stream)
		{
			// TODO:  Add PersistableAttributeStore.Deserialize implementation
		}

		#endregion

		protected void OnModified()
		{
			_lastModified = System.Environment.TickCount;
			if(_store != null) 
				_store.Save(this);
		}

		public override bool RemoveAttribute(string name)
		{
			bool result = base.RemoveAttribute (name);
			if(result && !name.StartsWith(AttributeStore.TransientPrefix))
				OnModified();
			return result;
		}

		public override void RemoveAttributes()
		{
			base.RemoveAttributes();
			OnModified();
		}

		public override bool SetAttribute(string name, object value)
		{
			bool result = base.SetAttribute (name, value);
			if(result && !name.StartsWith(AttributeStore.TransientPrefix))
				OnModified();
			return result;
		}

		public override void SetAttributes(IAttributeStore values)
		{
			base.SetAttributes (values);
			OnModified();
		}

		public override void SetAttributes(StringDictionary values)
		{
			base.SetAttributes (values);
			OnModified();
		}
	}
}
