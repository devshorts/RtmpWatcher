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
using System.IO;
// Import log4net classes.
using log4net;
using log4net.Config;

namespace com.TheSilentGroup.Fluorine.Filter
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class ASPXAdapter : IAdapter
	{
		private ILog _log;

		/// <summary>
		/// Initializes a new instance of the ASPXAdapter class.
		/// </summary>
		public ASPXAdapter()
		{
			try
			{
				_log = LogManager.GetLogger(typeof(ASPXAdapter));
			}
			catch{}
		}

		#region IAdapter Members

		public ResponseBody Invoke(IApplicationContext applicationContext, AMFBody amfBody)
		{
			string aspxPageVirtualFolder = "/" + amfBody.TypeName.Replace(".", "/");
			string aspxPage = aspxPageVirtualFolder + "/" + amfBody.Method + ".aspx";

			//Send in parameters to the ASPX page
			IList parameterList = amfBody.GetParameterList();
			applicationContext.Items["flash.parameters"] = parameterList;
			try
			{
				StringWriter sw = new StringWriter();
				applicationContext.Execute(aspxPage, sw);

				object result = applicationContext.Items["flash.result"];
				ResponseBody responseBody = new ResponseBody(amfBody, result);
				return responseBody;
			}
			catch(Exception ex)
			{
				if (ex.GetBaseException() != null)
					ex = ex.GetBaseException();
				if( _log != null && _log.IsErrorEnabled )
					_log.Error(ex.Message, ex);
				return new ErrorResponseBody(amfBody, ex);
			}
		}

		public bool SupportsService(IApplicationContext applicationContext, AMFBody amfBody)
		{
			return amfBody.IsAspxPage(applicationContext);
		}

		#endregion
	}
}
