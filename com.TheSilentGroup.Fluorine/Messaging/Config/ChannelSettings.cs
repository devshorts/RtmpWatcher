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
using System.Xml;
using System.Collections;

using com.TheSilentGroup.Fluorine.Messaging.Util;

namespace com.TheSilentGroup.Fluorine.Messaging.Config
{
    /// <summary>
    /// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public class ChannelSettings : Hashtable
    {
        public static string ContextRoot = "{context.root}";

        UriBase _uri;
        string _id;

        public ChannelSettings()
        {
        }

        public ChannelSettings(XmlNode channelDefinitionNode)
        {
            _id = channelDefinitionNode.Attributes["id"].Value;

            XmlNode endPointNode = channelDefinitionNode.SelectSingleNode("endpoint");
            string endpointClass = endPointNode.Attributes["class"].Value;
            string endpointUri = endPointNode.Attributes["uri"].Value;
            _uri = new UriBase(endpointUri);

            XmlNode propertiesNode = channelDefinitionNode.SelectSingleNode("properties");
            if (propertiesNode != null)
            {
                foreach (XmlNode propertyNode in propertiesNode.ChildNodes)
                {
                    this[propertyNode.Name] = propertyNode.InnerXml;
                }
            }
        }

        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public string Uri
        {
            set
            {
                _uri = new UriBase(value);
            }
        }

        public UriBase GetUri()
        {
            return _uri;
        }

        public bool Bind(IApplicationContext applicationContext, string path, string contextPath)
        {
            // The context root maps requests to the Flex application.
            // For example, the context root in the following URL is /flex:
            // http://localhost:8700/flex/myApp.mxml
            //
            // In the Flex configuration files, the {context.root} token takes the place of 
            // the path to the Flex web application itself. If you are running your MXML apps 
            // inside http://localhost:8100/flex) then "/flex" is the {context.root}. 
            // The value of {context.root} includes the prefix "/". 
            // As a result, you are not required to add a forward slash before the {context.root} token.
            //
            // If {context.root} is used in a nonrelative path, it must not have a leading "/". 
            // For example, instead of this:
            // http://localhost/{context.root}
            // Do this:
            // http://localhost{context.root}

            if (_uri != null)
            {
                string endpointPath = _uri.Path;
                if (!endpointPath.StartsWith("/"))
                    endpointPath = "/" + endpointPath;
                if (contextPath == "/")
                    contextPath = string.Empty;
                if (endpointPath.IndexOf("/" + ChannelSettings.ContextRoot) != -1)
                {
                    //relative path
                    endpointPath = endpointPath.Replace("/" + ChannelSettings.ContextRoot, contextPath);
                }
                else
                {
                    //nonrelative path, but we do not handle these for now
                    endpointPath = endpointPath.Replace(ChannelSettings.ContextRoot, contextPath);
                }
                if (endpointPath.ToLower() == path.ToLower())
                    return true;
            }
            return false;
        }
    }
}