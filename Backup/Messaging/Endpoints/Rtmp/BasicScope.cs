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

using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api;
using com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp.Api.Event;

namespace com.TheSilentGroup.Fluorine.Messaging.Endpoints.Rtmp
{
	/// <summary>
	/// Summary description for BasicScope.
	/// </summary>
	public class BasicScope : PersistableAttributeStore, IBasicScope, IEnumerable
	{
		public static int Global = 0x00;
		public static int Application = 0x01;
		public static int Room = 0x02;

		protected IScope	_parent;
		protected ArrayList _listeners;

		public BasicScope(IScope parent, string type, string name, bool persistent) : base(type, name, null, persistent)
		{
			_parent = parent;
			_listeners = new ArrayList();
		}

		#region IBasicScope Members

		public bool HasParent
		{
			get{ return _parent != null; }
		}

		public virtual IScope Parent
		{
			get{ return _parent; }
			set{ _parent = value; }
		}

		public int Depth
		{
			get
			{ 
				if( HasParent )
					return _parent.Depth + 1;
				else
					return 0;
			}
		}

		public override string Path
		{
			get
			{
				if( HasParent )
					return _parent.Path + "/" + _parent.Name;
				else
					return string.Empty;
			}
		}

		#endregion


		public void AddEventListener(IEventListener listener) 
		{
			_listeners.Add(listener);
		}

		public void RemoveEventListener(IEventListener listener) 
		{
			_listeners.Remove(listener);
			if(IsRoom(this) && IsPersistent && _listeners.Count == 0) 
			{
				// Delete empty rooms
				_parent.RemoveChildScope(this);
			}
		}

		public ICollection GetEventListeners()
		{
			return _listeners;
		}

		public bool HandleEvent(IEvent evt) 
		{
			return false;
		}

		public void NotifyEvent(IEvent evt) 
		{
		}

		public void DispatchEvent(IEvent evt) 
		{
			foreach(IEventListener listener in _listeners) 
			{
				if(evt.Source == null || evt.Source != listener) 
				{
					listener.NotifyEvent(evt);
				}
			}
		}

		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			return null;
		}

		#endregion


		#region Utils

		/// <summary>
		/// Check whether scope is an application scope (level 1 leaf in scope tree) or not
		/// </summary>
		/// <param name="scope"></param>
		/// <returns></returns>
		public static bool IsApplication(IBasicScope scope)
		{
			return scope.Depth == Application;
		}
		/// <summary>
		/// Check whether scope is a room scope (level 2 leaf in scope tree or lower, e.g. 3, 4, ...) or not
		/// </summary>
		/// <param name="scope"></param>
		/// <returns></returns>
		public static bool IsRoom(IBasicScope scope)
		{
			return scope.Depth >= Room;
		}

		#endregion Utils
	}
}
