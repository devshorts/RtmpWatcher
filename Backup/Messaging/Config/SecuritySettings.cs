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

namespace com.TheSilentGroup.Fluorine.Messaging.Config
{
	class SecurityConstraint
	{
		string		_id;
		string		_authMethod;
		string[]	_roles;

		public SecurityConstraint(string id, string authMethod, string[] roles)
		{
			_id = id;
			_authMethod = authMethod;
			_roles = roles;
		}

		public string Id{ get{ return _id; } }
		public string AuthMethod{ get{ return _authMethod; } }
		public string[] Roles{ get{ return _roles; } }
	}

	class SecurityConstraintRef
	{
		string		_reference;

		public SecurityConstraintRef(string reference)
		{
			_reference = reference;
		}

		public string Reference{ get{ return _reference; } }
	}

	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	class SecuritySettings : Hashtable
	{
		Hashtable _loginCommand;
		SecurityConstraintRef _securityConstraintRef;
		Hashtable _securityConstraints;
		/// <summary>
		/// Null for global security settings.
		/// </summary>
		DestinationSettings _destinationSettings;
		object _objLock = new object();

		public SecuritySettings(DestinationSettings destinationSettings)
		{
			_destinationSettings = destinationSettings;
			_securityConstraints = new Hashtable();
		}

		public SecuritySettings(DestinationSettings destinationSettings, XmlNode securityNode)
		{
			_destinationSettings = destinationSettings;
			_securityConstraints = new Hashtable();
			foreach(XmlNode propertyNode in securityNode.ChildNodes)
			{
				if( propertyNode.Name == "security-constraint" )
				{
					if( propertyNode.Attributes["ref"] != null )
					{
						_securityConstraintRef = new SecurityConstraintRef( propertyNode.Attributes["ref"].Value as string );
						continue;
					}
					string id = null;
					if( propertyNode.Attributes["id"] != null )
						id = propertyNode.Attributes["id"].Value as string;
					else
						id = Guid.NewGuid().ToString("N");
					string authMethod = "Custom";
					string[] roles = null;
					foreach(XmlNode node in propertyNode.ChildNodes)
					{
						if( node.Name == "auth-method" )
						{
							authMethod = node.InnerXml;
						}
						if( node.Name == "roles" )
						{
							ArrayList rolesTmp = new ArrayList();
							foreach(XmlNode roleNode in node.ChildNodes)
							{
								if( roleNode.Name == "role" )
								{
									rolesTmp.Add(roleNode.InnerXml);
								}
							}
							roles = rolesTmp.ToArray(typeof(string)) as string[];
						}
					}
					CreateSecurityConstraint(id, authMethod, roles);
					SecurityConstraint securityConstraint = new SecurityConstraint( id, authMethod, roles );
				}
				if( propertyNode.Name == "login-command" )
				{
					_loginCommand = new Hashtable(2);
					foreach(XmlAttribute attribute in propertyNode.Attributes)
					{
						_loginCommand[attribute.Name] = attribute.Value;
					}
				}
			}
		}

		public Hashtable LoginCommand{ get { return _loginCommand; } }
		public Hashtable SecurityConstraints{ get{ return _securityConstraints; } }
		public SecurityConstraintRef SecurityConstraintRef{ get{ return _securityConstraintRef; } }

		public SecurityConstraint CreateSecurityConstraint(string id, string authMethod, string[] roles)
		{
			lock(_objLock)
			{
				if( !_securityConstraints.ContainsKey(id) )
				{
					SecurityConstraint securityConstraint = new SecurityConstraint( id, authMethod, roles );
					_securityConstraints[id] = securityConstraint;
					return securityConstraint;
				}
				else
					return _securityConstraints[id] as SecurityConstraint;
			}
		}

		public string[] GetRoles()
		{
			lock(_objLock)
			{
				if( this.SecurityConstraintRef != null && _destinationSettings != null )
				{
					if( _destinationSettings.ServiceSettings.ServerSettings.SecuritySettings != null &&
						_destinationSettings.ServiceSettings.ServerSettings.SecuritySettings.SecurityConstraints != null )
					{
						SecurityConstraint securityConstraint = _destinationSettings.ServiceSettings.ServerSettings.SecuritySettings.SecurityConstraints[this.SecurityConstraintRef.Reference] as SecurityConstraint;
						if( securityConstraint != null )
							return securityConstraint.Roles;
						else
						{
							string error = string.Format("SecurityConstraint reference \"{0}\" not found. Check services-config.xml. Security information for the current request was not available.", this.SecurityConstraintRef.Reference );
							throw new UnauthorizedAccessException(error);
						}
					}
					else
						throw new UnauthorizedAccessException("<security> or <security-constraint> section not found. Check services-config.xml. Security information for the current request was not available." );
				}
				else
				{
					foreach(DictionaryEntry entry in this.SecurityConstraints)
					{
						SecurityConstraint securityConstraint = entry.Value as SecurityConstraint;
						return securityConstraint.Roles;
					}
				}
				return null;
			}
		}
	}
}
