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
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Threading;
// Import log4net classes.
using log4net;
using log4net.Config;

using com.TheSilentGroup.Fluorine.Activation;
using com.TheSilentGroup.Fluorine.Invocation;
using com.TheSilentGroup.Fluorine.Messaging.Security;
using com.TheSilentGroup.Fluorine.Messaging.Services;
using com.TheSilentGroup.Fluorine.Messaging.Messages;
using com.TheSilentGroup.Fluorine.Messaging.Endpoints;
using com.TheSilentGroup.Fluorine.Messaging.Config;
using com.TheSilentGroup.Fluorine.Messaging;

namespace com.TheSilentGroup.Fluorine.Filter
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class LibraryAdapter : IAdapter
	{
		private ILog _log;
		EndpointBase _endpoint;

		/// <summary>
		/// Initializes a new instance of the LibraryAdapter class.
		/// </summary>
		public LibraryAdapter(EndpointBase endpoint)
		{
			_endpoint = endpoint;
			try
			{
				_log = LogManager.GetLogger(typeof(LibraryAdapter));
			}
			catch{}
		}

		#region IAdapter Members

		public ResponseBody Invoke(IApplicationContext applicationContext, AMFBody amfBody)
		{
			if( amfBody.IsWebService )
				return null;

			try
			{
				Type type = ObjectFactory.LocateInLac(applicationContext, amfBody.TypeName);

				if( type != null )
				{
					MethodInfo mi = MethodHandler.GetMethod(applicationContext, type, amfBody.Method, amfBody.GetParameterList());
					//MethodInfo mi = type.GetMethod(amfBody.Method, BindingFlags.Public|BindingFlags.Instance|BindingFlags.Static);
					if( mi != null )
					{

						PageSizeAttribute pageSizeAttribute = null;
						MethodInfo miCounter = null;

						object[] pageSizeAttributes = mi.GetCustomAttributes( typeof(PageSizeAttribute), true);
						if( pageSizeAttributes != null && pageSizeAttributes.Length == 1 )
						{
							pageSizeAttribute = pageSizeAttributes[0] as PageSizeAttribute;
							miCounter = type.GetMethod( amfBody.Method + "Count" );
							if( miCounter != null && miCounter.ReturnType != typeof(System.Int32) )
								miCounter = null; //check signature
						}

						object obj = ObjectFactory.CreateInstance(applicationContext, type);
						ParameterInfo[] parameterInfos = mi.GetParameters();
						//Try to handle missing/optional parameters.
						object[] args = new object[parameterInfos.Length];
						if( !amfBody.IsRecordsetDelivery )
						{
							IList parameterList = amfBody.GetParameterList();
							if( args.Length != parameterList.Count )
							{
								string msg = string.Format("At least one of the passed arguments does not meet the parameter specification of the called method. {0} parameters for method {1}, expecting {2}.", parameterList.Count, mi.Name, args.Length);
								if( _log != null && _log.IsErrorEnabled )
									_log.Error(msg);
								return new ErrorResponseBody(amfBody, new ArgumentException(msg) );
							}

							parameterList.CopyTo(args, 0);
							if( pageSizeAttribute != null )
							{
								args[ args.Length - 1 ] = pageSizeAttribute.Limit;
								args[ args.Length - 2 ] = pageSizeAttribute.Offset;
							}
						}
						else
						{
							IList list = amfBody.GetParameterList();
							string recordsetId = list[0] as string;
							//object[] argsStore = applicationContext.Session[recordsetId] as object[];
							string recordetDeliveryParameters = amfBody.GetRecordsetArgs();
							byte[] buffer = Convert.FromBase64String(recordetDeliveryParameters);
							recordetDeliveryParameters = System.Text.Encoding.UTF8.GetString(buffer);

							string[] stringParameters = recordetDeliveryParameters.Split(new char[]{','});
							object[] argsStore = new object[stringParameters.Length];
							for(int i = 0; i < stringParameters.Length; i++)
							{
								if( stringParameters[i] == string.Empty )
									argsStore[i] = null;
								else
									argsStore[i] = stringParameters[i];
							}
							TypeHelper.NarrowValues(applicationContext, argsStore, parameterInfos);

							Array.Copy( argsStore, 0, args, 0, argsStore.Length );
							args[args.Length-2] = System.Convert.ToInt32(list[1]);
							args[args.Length-1] = System.Convert.ToInt32(list[2]);
						}

						TypeHelper.NarrowValues( applicationContext, args, parameterInfos);

						try
						{
							//object result = mi.Invoke( obj, args );
							InvocationHandler invocationHandler = new InvocationHandler(mi);
							object result = invocationHandler.Invoke(applicationContext, obj, args);

							ResponseBody responseBody = new ResponseBody(amfBody, result);

							if( pageSizeAttribute != null )
							{
								int totalCount = 0;
								string recordsetId = null;

								IList list = amfBody.GetParameterList();
								string recordetDeliveryParameters = null;
								if( !amfBody.IsRecordsetDelivery )
								{
									//fist call paging
									object[] argsStore = new object[list.Count];
									list.CopyTo(argsStore, 0);

									recordsetId = System.Guid.NewGuid().ToString();
									//applicationContext.Session[recordsetId] = argsStore;

									if( miCounter != null )
									{
										object[] counterArgs = new object[0];
										totalCount = (int)miCounter.Invoke( obj, counterArgs );
									}
									//applicationContext.Session[recordsetId + "C"] = totalCount;
									/*
									XmlSerializer xmlSerializer = new XmlSerializer(typeof(object[]));
									StringWriter stringWriter = new StringWriter();
									xmlSerializer.Serialize(stringWriter, argsStore);
									stringWriter.Close();
									recordetDeliveryParameters = stringWriter.ToString();
									*/
									string[] stringParameters = new string[argsStore.Length];
									for(int i = 0; i < argsStore.Length; i++)
									{
										if( argsStore[i] != null )
											stringParameters[i] = argsStore[i].ToString();
										else
											stringParameters[i] = string.Empty;
									}
									recordetDeliveryParameters = string.Join(",", stringParameters);
									byte[] buffer = System.Text.Encoding.UTF8.GetBytes(recordetDeliveryParameters);
									recordetDeliveryParameters = Convert.ToBase64String(buffer);									
								}
								else
								{
									recordsetId = amfBody.GetParameterList()[0] as string;
									//totalCount = (int)applicationContext.Session[recordsetId + "C"];
								}
								if( result is DataTable )
								{
									DataTable dataTable = result as DataTable;
									dataTable.ExtendedProperties["TotalCount"] = totalCount;
									dataTable.ExtendedProperties["Service"] = recordetDeliveryParameters + "/" + amfBody.Target;
									dataTable.ExtendedProperties["RecordsetId"] = recordsetId;
									if( amfBody.IsRecordsetDelivery )
									{
										dataTable.ExtendedProperties["Cursor"] = Convert.ToInt32( list[list.Count - 2] );
										dataTable.ExtendedProperties["DynamicPage"] = true;
									}
								}
							}
							return responseBody;
						}
						catch(Exception exception)
						{
							if( exception is TargetInvocationException && exception.InnerException != null)
								return new ErrorResponseBody(amfBody, exception.InnerException);
							else
								return new ErrorResponseBody(amfBody, exception);
						}
					}
					else
						return new ErrorResponseBody(amfBody, new MissingMethodException(amfBody.TypeName, amfBody.Method) );
				}
				else
					return new ErrorResponseBody(amfBody, new TypeInitializationException(amfBody.TypeName, null) );
			}
			catch(Exception exception)
			{
				return new ErrorResponseBody(amfBody, exception);
			}
		}

		public bool SupportsService(IApplicationContext applicationContext, AMFBody amfBody)
		{
			if( amfBody.IsWebService )
				return false;

			Type type = ObjectFactory.LocateInLac(applicationContext, amfBody.TypeName);
			if( type == null )
				return false;

			return MethodHandler.SupportsMethod(applicationContext, type, amfBody.Method);
		}

		#endregion
	}
}
