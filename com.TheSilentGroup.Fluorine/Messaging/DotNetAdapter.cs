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

using com.TheSilentGroup.Fluorine.Activation;
using com.TheSilentGroup.Fluorine.AMF3;

using com.TheSilentGroup.Fluorine.Data;
using com.TheSilentGroup.Fluorine.Data.Messages;
using com.TheSilentGroup.Fluorine.Data.Assemblers;
using com.TheSilentGroup.Fluorine.Messaging;
using com.TheSilentGroup.Fluorine.Messaging.Messages;
using com.TheSilentGroup.Fluorine.Messaging.Services;

namespace com.TheSilentGroup.Fluorine
{
	/// <summary>
	/// Summary description for DotNetAdapter.
	/// </summary>
	internal class DotNetAdapter : ServiceAdapter
	{
		public DotNetAdapter()
		{
		}

		public override object Invoke(IApplicationContext applicationContext, IMessage message)
		{
			object result = null;
			DataMessage dataMessage = message as DataMessage;
			switch(dataMessage.operation)
			{
				case DataMessage.FillOperation:
					result = Fill(applicationContext, dataMessage);
					break;
				case DataMessage.UpdateOperation:
					Update(applicationContext, dataMessage);
					result = dataMessage;//send back
					break;
				case DataMessage.CreateOperation:
					Create(applicationContext, dataMessage);
					result = dataMessage;
					break;
				case DataMessage.DeleteOperation:
					Delete(applicationContext, dataMessage);
					result = dataMessage;
					break;
			}

			return result;
		}

		private IList Fill(IApplicationContext applicationContext, DataMessage dataMessage)
		{
			IList result = null;
			Hashtable properties = this.DestinationSettings.Properties;
			string assemblerTypeName = properties["source"] as string;
			IAssembler assembler = ObjectFactory.CreateInstance(applicationContext, assemblerTypeName) as IAssembler;
			if( assembler != null )
			{
				IList parameters = dataMessage.body as IList;
				result = assembler.Fill(parameters);
				return result;
			}
			return null;
		}

		private void Update(IApplicationContext applicationContext, DataMessage dataMessage)
		{
			Hashtable properties = this.DestinationSettings.Properties;
			string assemblerTypeName = properties["source"] as string;
			IAssembler assembler = ObjectFactory.CreateInstance(applicationContext, assemblerTypeName) as IAssembler;
			if( assembler != null )
			{
				IList parameters = dataMessage.body as IList;
				assembler.UpdateItem(parameters[2], parameters[1], parameters[0] as IList);
			}
		}

		private void Create(IApplicationContext applicationContext, DataMessage dataMessage)
		{
			Hashtable properties = this.DestinationSettings.Properties;
			string assemblerTypeName = properties["source"] as string;
			IAssembler assembler = ObjectFactory.CreateInstance(applicationContext, assemblerTypeName) as IAssembler;
			if( assembler != null )
			{
				assembler.CreateItem(dataMessage.body);
				Identity identity = Identity.GetIdentity(dataMessage.body, this.Destination as DataDestination);
				dataMessage.identity = identity;
			}
		}

		private void Delete(IApplicationContext applicationContext, DataMessage dataMessage)
		{
			Hashtable properties = this.DestinationSettings.Properties;
			string assemblerTypeName = properties["source"] as string;
			IAssembler assembler = ObjectFactory.CreateInstance(applicationContext, assemblerTypeName) as IAssembler;
			if( assembler != null )
			{
				assembler.DeleteItem(dataMessage.body);
			}
		}
	}
}
