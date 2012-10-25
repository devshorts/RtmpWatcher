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
using System.Data;
using System.Collections;
using System.Reflection;
using com.TheSilentGroup.Fluorine.Invocation;

namespace com.TheSilentGroup.Fluorine
{
	/// <summary>
	/// The DataSetTypeAttribute specifies the types of data in a DataSet.
	/// </summary>
	public class DataSetTypeAttribute : System.Attribute, IInvocationResultHandler
	{
		string	_remoteClass;

		public DataSetTypeAttribute(string remoteClass)
		{
			_remoteClass = remoteClass;
		}

		#region IInvocationResultHandler Members

		public void HandleResult(IInvocationManager invocationManager, MethodInfo methodInfo, object obj, object[] arguments, object result)
		{
			if( result is DataSet )
			{
				DataSet dataSet = result as DataSet;
				ASObject asoResult = new ASObject(_remoteClass);

				foreach(DictionaryEntry entry in invocationManager.Properties)
				{
					if( entry.Key is DataTable )
					{
						DataTable dataTable = entry.Key as DataTable;
						if( dataSet.Tables.IndexOf(dataTable) != -1 )
						{
							if( !dataTable.ExtendedProperties.ContainsKey("alias") )
								asoResult[dataTable.TableName] = entry.Value;
							else
								asoResult[ dataTable.ExtendedProperties["alias"] as string ] = entry.Value;
						}
					}
				}
				invocationManager.Result = asoResult;
			}
		}

		#endregion
	}
}
