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

namespace com.TheSilentGroup.Fluorine.Activation
{
	/// <summary>
	/// Activator interface.
	/// </summary>
	public interface IActivator
	{
		/// <summary>
		/// Activate instance of requested type.
		/// </summary>
		/// <param name="applicationContext">The application context.</param>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		object Activate(IApplicationContext applicationContext, Type type);
		/// <summary>
		/// Activate instance of requested type.
		/// </summary>
		/// <param name="applicationContext">The application context.</param>
		/// <param name="type">The type.</param>
		/// <param name="args"></param>
		/// <returns></returns>
		object Activate(IApplicationContext applicationContext, Type type, object[] args);
		
		/// <summary>
		/// Gets the activation mode.
		/// </summary>
		string ActivationMode{ get; }
	}
}
