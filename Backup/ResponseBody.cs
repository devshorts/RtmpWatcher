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

using com.TheSilentGroup.Fluorine.Messaging;
using com.TheSilentGroup.Fluorine.Messaging.Messages;

namespace com.TheSilentGroup.Fluorine
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	public class ResponseBody : AMFBody
	{
		AMFBody	_requestBody;

		/// <summary>
		/// Initializes a new instance of the ResponseBody class.
		/// </summary>
		internal ResponseBody()
		{
		}
		/// <summary>
		/// Initializes a new instance of the ResponseBody class.
		/// </summary>
		/// <param name="requestBody"></param>
		public ResponseBody(AMFBody	requestBody)
		{
			_requestBody = requestBody;
		}
		/// <summary>
		/// Initializes a new instance of the ResponseBody class.
		/// </summary>
		/// <param name="requestBody"></param>
		/// <param name="content"></param>
		public ResponseBody(AMFBody	requestBody, object content)
		{
			_requestBody = requestBody;
			_target = requestBody.Response + "/onResult";
			_content = content;
			_response = "null";
		}

		public AMFBody RequestBody
		{
			get{ return _requestBody; }
			set{ _requestBody = value; }
		}

		public string GetSource()
		{
			if( _requestBody == null )
				return null;

			if( !_requestBody.IsEmptyTarget )
				return _requestBody.Target;

			object content = _requestBody.Content;
			if( content is IList )
				content = (content as IList)[0];
			IMessage message = content as IMessage;
			if( message != null )
			{
				return message.GetMessageSource();
			}
			return null;
		}

		public override IList GetParameterList()
		{
			if( _requestBody == null )
				return null;

			return _requestBody.GetParameterList ();
		}


		public void WriteBody(IApplicationContext applicationContext, ObjectEncoding objectEncoding, AMFWriter writer)
		{
			writer.Reset();
			if(this.Target == null)
				writer.WriteUTF("null");
			else
				writer.WriteUTF(this.Target);

			if(this.Response == null)
				writer.WriteUTF("null");
			else
				writer.WriteUTF(this.Response);
			writer.WriteInt32(-1);

			WriteBodyData(applicationContext, objectEncoding, writer);
		}

		protected virtual void WriteBodyData(IApplicationContext applicationContext, ObjectEncoding objectEncoding, AMFWriter writer)
		{
			string source = this.GetSource();
			if( applicationContext != null && source != null
				&& applicationContext.CacheMap != null 
				&& applicationContext.CacheMap.ContainsCacheDescriptor(source)
				)
			{
				MemoryStream ms = new MemoryStream();
				AMFSerializer tmpAMFSerializer = new AMFSerializer(ms);
				object content = this.Content;

				if( content is IMessage )
				{
					IMessage message = content as IMessage;
					content = message.body;
					if( objectEncoding == ObjectEncoding.AMF0 )
						tmpAMFSerializer.WriteData(applicationContext, objectEncoding, content);
					else
						tmpAMFSerializer.WriteAMF3Data(applicationContext, content);
					tmpAMFSerializer.Flush();
					byte[] cachedContent = ms.ToArray();
					CacheResult cacheResult = new CacheResult(cachedContent);
					IList arguments = this.GetParameterList();
					string key = com.TheSilentGroup.Fluorine.Configuration.CacheMap.GenerateCacheKey(source, arguments);
					applicationContext.CacheMap.Add(source, key, cacheResult);
					message.body = cacheResult;
				}
				else
				{
					tmpAMFSerializer.WriteData(applicationContext, objectEncoding, content);
					tmpAMFSerializer.Flush();
					byte[] cachedContent = ms.ToArray();
					CacheResult cacheResult = new CacheResult(cachedContent);
					IList arguments = this.GetParameterList();
					string key = com.TheSilentGroup.Fluorine.Configuration.CacheMap.GenerateCacheKey(source, arguments);
					applicationContext.CacheMap.Add(source, key, cacheResult);
					this.Content = cacheResult;
				}
			}

			writer.WriteData(applicationContext, objectEncoding, this.Content);
		}
	}
}
