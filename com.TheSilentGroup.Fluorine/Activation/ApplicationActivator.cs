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

namespace com.TheSilentGroup.Fluorine.Activation
{
	/// <summary>
	/// ApplicationActivator handles application level object activations.<br/>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	public class ApplicationActivator : IActivator
	{
		private Hashtable instances = new Hashtable();

		/// <summary>
		/// Initializes a new instance of the ApplicationActivator class.
		/// </summary>
		public ApplicationActivator()
		{
		}

		#region IActivator Members

		public object Activate(IApplicationContext applicationContext, Type type)
		{
			return Activate(applicationContext, type, null);
		}

		public object Activate(IApplicationContext applicationContext, Type type, object[] args)
		{
			// Check in the cache.
			object instance = instances[type];

			// Not found (first request).
			if(instance == null)
			{
				if (type.IsAbstract && type.IsSealed)
				{
					instance = type;
				}
				else
				{
					if( args == null )
						instance = Activator.CreateInstance(type, BindingFlags.CreateInstance|BindingFlags.Public|BindingFlags.Instance|BindingFlags.Static, null, new object[]{}, null);
					else
						instance = Activator.CreateInstance(type, BindingFlags.CreateInstance|BindingFlags.Public|BindingFlags.Instance|BindingFlags.Static, null, args, null);
				}
				instances[type] = instance;
			}
			return instance;
		}

		public string ActivationMode
		{
			get
			{
				return "application";
			}
		}

		#endregion
	}
}
