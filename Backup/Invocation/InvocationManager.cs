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
using System.Reflection;
using System.Collections;

namespace com.TheSilentGroup.Fluorine.Invocation
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	public interface IInvocationManager
	{
		/// <summary>
		/// Gets a stack-based, user-defined storage area that is useful for communication between callback handlers.
		/// </summary>
		Stack Context {get;}
		Hashtable Properties { get; }
		object Result {get; set;}
	}

	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class InvocationManager : IInvocationManager
	{
		Stack		_context;
		Hashtable	_properties;
		object		_result;

		public InvocationManager()
		{
			_context = new Stack();
			_properties = new Hashtable();
		}

		#region IInvocationManager Members

		public Stack Context
		{
			get
			{
				return _context;
			}
		}

		public Hashtable Properties
		{
			get
			{
				return _properties;
			}
		}

		public object Result
		{
			get
			{
				return _result;
			}
			set
			{
				_result = value;
			}
		}

		#endregion
	}
}
