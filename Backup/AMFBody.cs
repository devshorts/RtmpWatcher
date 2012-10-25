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
using System.Text;
using System.IO;

using com.TheSilentGroup.Fluorine.Messaging;
using com.TheSilentGroup.Fluorine.Messaging.Messages;

namespace com.TheSilentGroup.Fluorine
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	public class AMFBody
	{
		public const string Recordset = "rs://";
		protected object	_content;
		protected string	_response;
		protected string	_target;
		/// <summary>
		/// IgnoreResults is a flag to tell the serializer to ignore the results of the body message.
		/// </summary>
		protected bool	_ignoreResults;
		protected bool	_isAuthenticationAction;
		protected bool	_isDebug;
		protected bool	_isDescribeService;

		/// <summary>
		/// Initializes a new instance of the AMFBody class.
		/// </summary>
		public AMFBody()
		{
		}
		/// <summary>
		/// Initializes a new instance of the AMFBody class.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="response"></param>
		/// <param name="content"></param>
		public AMFBody(string target, string response, object content)
		{
			this._target = target;
			this._response = response;
			this._content = content;
		}

		public string Target
		{
			get{ return _target; }
			set
			{ 
				_target = value;
			}
		}

		public bool IsEmptyTarget
		{
			get
			{
				return _target == null || _target == string.Empty || _target == "null";
			}
		}

		public string Response
		{
			get{ return _response; }
			set{ _response = value; }
		}

		public object Content
		{
			get{ return _content; }
			set{ _content = value; }
		}

		public bool IsAuthenticationAction
		{
			get{ return _isAuthenticationAction; }
			set{ _isAuthenticationAction = value; }
		}

		public bool IgnoreResults
		{
			get{ return _ignoreResults; }
			set{ _ignoreResults = value; }
		}

		public bool IsDebug
		{
			get{ return _isDebug; }
			set{ _isDebug = value; }
		}

		public bool IsDescribeService
		{
			get{ return _isDescribeService; }
			set{ _isDescribeService = value; }
		}

		public bool IsWebService
		{
			get
			{
				// Check for a http link which means web service or wsdl fragment.
				if( this.TypeName != null )
				{
					string targetLower = this.TypeName.ToLower();
					if(targetLower.StartsWith("http://") || targetLower.StartsWith("https://")  )
						return true;
					if(targetLower.StartsWith("<?xml version") )
						return true;
					if(targetLower.EndsWith(".wsdl") )
						return true;
					if(targetLower.EndsWith(".asmx?wsdl") )
						return true;
					if(targetLower.EndsWith(".asmx") )
						return true;
				}
				return false;
			}
		}

		public bool IsAspxPage(IApplicationContext applicationContext)
		{
			if( applicationContext == null )
				return false;
			if( this.Target.StartsWith( AMFBody.Recordset ) )
				return false;
			if( this.IsWebService )
				return false;
			try
			{
				//Type name in this case would be an ASPX page name
				string aspxPageVirtualFolder = "/" + this.TypeName.Replace(".", "/");
				string aspxPage = aspxPageVirtualFolder + "/" + this.Method + ".aspx";
				string aspxPagePhysicalPath = applicationContext.MapPath(aspxPage);
				if (File.Exists(aspxPagePhysicalPath))
					return true;
			}
			catch
			{
			}
			return false;
		}

		public bool IsRecordsetDelivery
		{
			get
			{
				if( _target.StartsWith(AMFBody.Recordset) )
					return true;
				else
					return false;
				
			}
		}

		public string GetRecordsetArgs()
		{
			if( _target != null )
			{
				if( this.IsRecordsetDelivery )
				{
					string args = _target.Substring( AMFBody.Recordset.Length );
					args = args.Substring( 0, args.IndexOf("/") );
					return args;
				}
			}
			return null;
		}

		public string TypeName
		{
			get
			{
				if( _target != "null" && _target != null && _target != string.Empty )
				{
					if( _target.LastIndexOf('.') != -1 )
					{
						string target = _target.Substring(0, _target.LastIndexOf('.'));
						if( this.IsRecordsetDelivery )
						{
							target = target.Substring( AMFBody.Recordset.Length );
							target = target.Substring( target.IndexOf("/") + 1 );
							target = target.Substring(0, target.LastIndexOf('.'));
						}
						return target;
					}
				}
				return null;
			}
		}

		public string Method
		{
			get
			{
				if( _target != "null" && _target != null && _target != string.Empty )
				{
					if( _target != null && _target.LastIndexOf('.') != -1 )
					{
						string target = _target;
						if( this.IsRecordsetDelivery )
						{
							target = target.Substring( AMFBody.Recordset.Length );
							target = target.Substring( target.IndexOf("/") + 1 );
						}

						if( this.IsRecordsetDelivery )
							target = target.Substring(0, target.LastIndexOf('.'));
						string method = target.Substring(target.LastIndexOf('.')+1);

						return method;
					}
				}
				return null;
			}
		}

		public string GetSignature()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append( this.Target );
			IList parameterList = GetParameterList();
			for(int i = 0; i < parameterList.Count; i++)
			{
				object parameter = parameterList[i];
				sb.Append( parameter.GetType().FullName );
			}
			return sb.ToString();
		}

		public virtual IList GetParameterList()
		{
			IList list = null;
			if( !this.IsEmptyTarget )//Flash RPC parameters
			{
				if(!(_content is IList))
				{
					list = new ArrayList();
					list.Add(_content );
				}
				else 
					list = _content as IList;
			}
			else
			{
				object content = this.Content;
				if( content is IList )
					content = (content as IList)[0];
				IMessage message = content as IMessage;
				if( message != null )
				{
					//for RemotingMessages only now
					if( message is RemotingMessage )
					{
						list = message.body as IList;
					}
				}
			}

			if( list == null )
				list = new ArrayList();
			return list;
		}

		/*
		public static AMFBody GetResponseForException(AMFBody amfBody, Exception exception)
		{
			if( !amfBody.IsEmptyTarget )
			{
				//Flash error
				ErrorAMFBody bodyOut = new ErrorAMFBody();
				bodyOut.IgnoreResults = amfBody.IgnoreResults;
				bodyOut.Target = amfBody.Response + "/onStatus";
				bodyOut.Response = null;
				bodyOut.Content = exception;
				bodyOut.RequestBody = amfBody;
				return bodyOut;
			}
			else
			{
				MessageException me = null;
				if( exception is MessageException )
					me = exception as MessageException;
				else
					me = new MessageException(exception);
				IList content = amfBody.GetParameterList();
				IMessage message = content[0] as IMessage;
				ErrorMessage errorMessage = me.GetErrorMessage();
				errorMessage.clientId = message.clientId;
				errorMessage.correlationId = message.messageId;
				errorMessage.destination = message.destination;
				return GetResponseForException(amfBody, message, errorMessage);
			}
		}

		public static AMFBody GetResponseForException(AMFBody amfBody, IMessage message, ErrorMessage errorMessage)
		{
			ErrorAMFBody bodyOut = new ErrorAMFBody();
			bodyOut.Content = errorMessage;
			bodyOut.Target = amfBody.Response + "/onStatus";
			bodyOut.IgnoreResults = amfBody.IgnoreResults;
			bodyOut.Response = "";
			bodyOut.RequestBody = amfBody;
			return bodyOut;
		}
		*/
	}
}
