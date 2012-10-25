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
using com.TheSilentGroup.Fluorine.Filter;

namespace com.TheSilentGroup.Fluorine.Gateway
{
	/// <summary>
	/// Summary description for FilterChain.
	/// </summary>
	internal class FilterChain
	{
		IFilter _filter;

		/// <summary>
		/// Initializes a new instance of the FilterChain class.
		/// </summary>
		/// <param name="filter"></param>
		public FilterChain(IFilter filter)
		{
			_filter = filter;
		}

		public void InvokeFilters(AMFContext context)
		{
			IFilter filter = _filter;
			while(filter != null)
			{
				filter.Invoke(context);
				filter = filter.Next;
			}
		}
	}
}
