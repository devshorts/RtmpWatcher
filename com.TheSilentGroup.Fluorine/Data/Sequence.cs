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

namespace com.TheSilentGroup.Fluorine.Data
{
	/// <summary>
	/// Summary description for Sequence.
	/// </summary>
	internal class Sequence : CollectionBase
	{
		int			_id;
		object[]	_parameters;
		Hashtable	_subcribers;

		public Sequence()
		{
			_subcribers = new Hashtable();
		}

		public int Id
		{
			get{ return _id; }
			set{ _id = value; }
		}

		public int Size
		{
			get{ return this.Count; }
		}

		public int Add(Identity identity)
		{
			return this.InnerList.Add(identity);
		}

		public Identity this[int index]
		{
			get{ return this.InnerList[index] as Identity; }
		}

		public bool Contains(Identity identity)
		{
			return this.InnerList.Contains(identity);
		}

		public void Remove(Identity identity)
		{
			this.InnerList.Remove(identity);
		}

		public object[] Parameters
		{
			get{ return _parameters; }
			set{ _parameters = value; }
		}

		public void AddSubscriber(string clientId)
		{
			lock(_subcribers)
			{
				_subcribers[clientId] = clientId;
			}
		}

		public void RemoveSubscriber(string clientId)
		{
			lock(_subcribers)
			{
				_subcribers.Remove(clientId);
			}
		}

		public int SubscriberCount
		{
			get
			{ 
				lock(_subcribers)
				{
					return _subcribers.Count; 
				}
			}
		}
	}
}
