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

using com.TheSilentGroup.Fluorine.Messaging;
using com.TheSilentGroup.Fluorine.Messaging.Messages;

namespace com.TheSilentGroup.Fluorine
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	public class AMFSerializer : AMFWriter
	{
		private ILog _log;

		/// <summary>
		/// Initializes a new instance of the AMFSerializer class.
		/// </summary>
		/// <param name="stream"></param>
		public AMFSerializer(Stream stream) : base(stream)
		{
			try
			{
				_log = LogManager.GetLogger(typeof(AMFSerializer));
			}
			catch{}
		}

		public void WriteMessage(AMFMessage message)
		{
			WriteMessage(null, message);
		}

		public void WriteMessage(IApplicationContext applicationContext, AMFMessage amfMessage)
		{
			try
			{
				base.WriteShort(amfMessage.Version);
				int headerCount = amfMessage.HeaderCount;
				base.WriteShort(headerCount);
				for(int i = 0; i < headerCount; i++)
				{
					this.WriteHeader(applicationContext, amfMessage.GetHeaderAt(i), ObjectEncoding.AMF0);
				}
				int bodyCount = amfMessage.BodyCount;
				base.WriteShort(bodyCount);
				for(int i = 0; i < bodyCount; i++)
				{
					ResponseBody responseBody = amfMessage.GetBodyAt(i) as ResponseBody;
					if( responseBody != null && !responseBody.IgnoreResults )
					{
						//Try to catch serialization errors
						if( this.BaseStream.CanSeek )
						{
							long position = this.BaseStream.Position;

							try
							{
								responseBody.WriteBody(applicationContext, amfMessage.ObjectEncoding, this);
							}
							catch(Exception exception)
							{
								this.BaseStream.Seek(position, SeekOrigin.Begin);
								//this.BaseStream.Position = position;

								if( _log != null && _log.IsFatalEnabled )
								{
									_log.Fatal("Message serializer failed.", exception);
									_log.Fatal("Retry to send an error response.");
								}
								
								ErrorResponseBody errorResponseBody;
								if( responseBody.RequestBody.IsEmptyTarget )
								{
									object content = responseBody.RequestBody.Content;
									if( content is IList )
										content = (content as IList)[0];
									IMessage message = content as IMessage;
									MessageException messageException = new MessageException(exception);
									messageException.FaultCode = "AMF serializer failed";
									errorResponseBody = new ErrorResponseBody(responseBody.RequestBody, message, messageException);
								}
								else
									errorResponseBody = new ErrorResponseBody(responseBody.RequestBody, exception);

								try
								{
									errorResponseBody.WriteBody(applicationContext, amfMessage.ObjectEncoding, this);
								}
								catch(Exception exception2)
								{
									if( _log != null && _log.IsFatalEnabled )
									{
										_log.Fatal("Could not send an error response.", exception2);
									}
									throw;
								}
							}
						}
						else
							responseBody.WriteBody(applicationContext, amfMessage.ObjectEncoding, this);
					}
				}
			}
			catch(Exception exception)
			{
				if( _log != null && _log.IsFatalEnabled )
					_log.Fatal("Message serializer failed. ", exception);
				throw;
			}
		}

		private void WriteHeader(IApplicationContext applicationContext, AMFHeader header, ObjectEncoding objectEncoding)
		{
			base.WriteUTF(header.Name);
			base.WriteBoolean(header.MustUnderstand);
			base.WriteInt32(-1);
			base.WriteData(applicationContext, objectEncoding, header.Content);
		}
 
		/*
		private void WriteBody(IApplicationContext applicationContext, AMFBody body, ObjectEncoding objectEncoding)
		{
			if(body.Target == null)
				base.WriteUTF("null");
			else
				base.WriteUTF(body.Target);

			if(body.Response == null)
				base.WriteUTF("null");
			else
				base.WriteUTF(body.Response);

			base.WriteInt32(-1);
			this.Reset();
			base.WriteData(applicationContext, objectEncoding, body.Content);
		}
		*/
	}
}
