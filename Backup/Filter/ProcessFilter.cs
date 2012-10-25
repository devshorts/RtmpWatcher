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
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using System.Web.Configuration;
using System.Web.Caching;
using System.Threading;
using System.Reflection;
// Import log4net classes.
using log4net;
using log4net.Config;

using com.TheSilentGroup.Fluorine.Exceptions;
using com.TheSilentGroup.Fluorine.Gateway;
using com.TheSilentGroup.Fluorine.Diagnostic;
using com.TheSilentGroup.Fluorine.Activation;
using com.TheSilentGroup.Fluorine.Messaging.Security;
using com.TheSilentGroup.Fluorine.Messaging.Services;
using com.TheSilentGroup.Fluorine.Messaging.Messages;
using com.TheSilentGroup.Fluorine.Messaging.Endpoints;
using com.TheSilentGroup.Fluorine.Messaging;

namespace com.TheSilentGroup.Fluorine.Filter
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class ProcessFilter : AbstractFilter
	{
		private ILog _log;

		EndpointBase _endpoint;

		LibraryAdapter		_libraryAdapter;
		ASPXAdapter			_aspxAdapter;
		/// <summary>
		/// Initializes a new instance of the ProcessFilter class.
		/// </summary>
		public ProcessFilter(EndpointBase endpoint)
		{
			try
			{
				_log = LogManager.GetLogger(typeof(ProcessFilter));
			}
			catch{}
			_endpoint = endpoint;
			_libraryAdapter = new LibraryAdapter(endpoint);
			_aspxAdapter = new ASPXAdapter();
		}

		#region IFilter Members

		public override void Invoke(AMFContext context)
		{
			MessageOutput messageOutput = context.MessageOutput;
			for(int i = 0; i < context.AMFMessage.BodyCount; i++)
			{
				AMFBody amfBody = context.AMFMessage.GetBodyAt(i);
				ResponseBody responseBody = null;
				//Check for Flex2 messages and skip
				if( amfBody.IsEmptyTarget )
					continue;

				if( amfBody.IsDebug )
					continue;
				if( amfBody.IsDescribeService )
				{
					responseBody = new ResponseBody();
					responseBody.IgnoreResults = amfBody.IgnoreResults;
					responseBody.Target = amfBody.Response + "/onResult";
					responseBody.Response = null;
					DescribeService describeService = new DescribeService( amfBody );
					responseBody.Content = describeService.GetDescription();
					messageOutput.AddBody(responseBody);
					continue;
				}

				//Check if response exists.
				responseBody = messageOutput.GetResponse(amfBody);
				if( responseBody != null )
				{
					if( responseBody is CachedBody )
					{
						if( _log != null && _log.IsDebugEnabled )
							_log.Debug("Response to the request " + amfBody.Target + " retrieved.");
					}
					continue;
				}

				try
				{
					responseBody = DoAction(context, amfBody );
					if (responseBody.Content is Exception && _log != null && _log.IsErrorEnabled )
					{
						Exception ex = responseBody.Content as Exception;
						_log.Error(ex.Message, ex);
					}
				}
				catch(UnauthorizedAccessException ex)
				{
					responseBody = new ErrorResponseBody(amfBody, ex);
				}
				catch(Exception ex)
				{
					if( _log != null && _log.IsErrorEnabled )
						_log.Error(ex.Message, ex);
					responseBody = new ErrorResponseBody(amfBody, ex);
				}
				messageOutput.AddBody(responseBody);
			}
		}

		private ResponseBody DoAction(AMFContext context, AMFBody amfBody)
		{
			ResponseBody responseBody = null;
			//Give higher priority to the library adapter as the aspx adapter will throw exceptions
			if( _libraryAdapter.SupportsService( context.ApplicationContext, amfBody ) )
			{
				responseBody = _libraryAdapter.Invoke(context.ApplicationContext, amfBody);
			}
			else if( _aspxAdapter.SupportsService( context.ApplicationContext, amfBody ) )
			{
				responseBody = _aspxAdapter.Invoke(context.ApplicationContext, amfBody);
			}
			else
			{
				//At this point no handling was provided so something went really wrong
				string msg = "AMF message was not handled. Check your log file for further information. Failed target: " + amfBody.Target;
				if( _log != null && _log.IsFatalEnabled )
					_log.Fatal(msg);
				responseBody = new ErrorResponseBody(amfBody, new FluorineException(msg));
			}
			return responseBody;
		}

		#endregion

	}
}
