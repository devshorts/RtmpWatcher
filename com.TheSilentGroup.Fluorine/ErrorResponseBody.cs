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

using com.TheSilentGroup.Fluorine.Messaging;
using com.TheSilentGroup.Fluorine.Messaging.Messages;

namespace com.TheSilentGroup.Fluorine
{
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public class ErrorResponseBody : ResponseBody
    {
        /// <summary>
        /// Initializes a new instance of the ErrorResponseBody class.
        /// </summary>
        private ErrorResponseBody()
        {
        }
        /// <summary>
        /// Initializes a new instance of the ErrorResponseBody class.
        /// </summary>
        /// <param name="requestBody"></param>
        /// <param name="error"></param>
        public ErrorResponseBody(AMFBody requestBody, string error)
            : base(requestBody)
        {
            this.IgnoreResults = requestBody.IgnoreResults;
            this.Target = requestBody.Response + "/onStatus";
            this.Response = null;
            this.Content = error;
        }
        /// <summary>
        /// Initializes a new instance of the ErrorResponseBody class.
        /// </summary>
        /// <param name="requestBody"></param>
        /// <param name="exception"></param>
        public ErrorResponseBody(AMFBody requestBody, Exception exception)
            : base(requestBody)
        {
            this.Content = exception;
            if (requestBody.IsEmptyTarget)
            {
                object content = requestBody.Content;
                if (content is IList)
                    content = (content as IList)[0];
                IMessage message = content as IMessage;
                //Check for Flex2 messages and handle
                if (message != null)
                {
                    ErrorMessage errorMessage = GetErrorMessage(message, exception);
                    this.Content = errorMessage;
                }
            }
            this.IgnoreResults = requestBody.IgnoreResults;
            this.Target = requestBody.Response + "/onStatus";
            this.Response = null;
        }
        /// <summary>
        /// Initializes a new instance of the ErrorResponseBody class.
        /// </summary>
        /// <param name="requestBody"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public ErrorResponseBody(AMFBody requestBody, IMessage message, Exception exception)
            : base(requestBody)
        {
            ErrorMessage errorMessage = GetErrorMessage(message, exception);
            errorMessage.clientId = message.clientId;
            errorMessage.correlationId = message.messageId;
            errorMessage.destination = message.destination;
            this.Content = errorMessage;
            this.Target = requestBody.Response + "/onStatus";
            this.IgnoreResults = requestBody.IgnoreResults;
            this.Response = "";
        }
        /// <summary>
        /// Initializes a new instance of the ErrorResponseBody class.
        /// </summary>
        /// <param name="requestBody"></param>
        /// <param name="message"></param>
        /// <param name="errorMessage"></param>
        public ErrorResponseBody(AMFBody requestBody, IMessage message, ErrorMessage errorMessage)
            : base(requestBody)
        {
            errorMessage.clientId = message.clientId;
            errorMessage.correlationId = message.messageId;
            errorMessage.destination = message.destination;
            this.Content = errorMessage;
            this.Target = requestBody.Response + "/onStatus";
            this.IgnoreResults = requestBody.IgnoreResults;
            this.Response = "";
        }

        private ErrorMessage GetErrorMessage(IMessage message, Exception exception)
        {
            MessageException me = null;
            if (exception is MessageException)
                me = exception as MessageException;
            else
                me = new MessageException(exception);
            ErrorMessage errorMessage = me.GetErrorMessage();
            errorMessage.clientId = message.clientId;
            errorMessage.correlationId = message.messageId;
            errorMessage.destination = message.destination;
            return errorMessage;
        }

        protected override void WriteBodyData(IApplicationContext applicationContext, ObjectEncoding objectEncoding, AMFWriter writer)
        {
            writer.WriteData(applicationContext, objectEncoding, this.Content);
        }

    }
}
