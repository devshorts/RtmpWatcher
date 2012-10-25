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
using System.Reflection;
using System.Collections;
using com.TheSilentGroup.Fluorine.Exceptions;

namespace com.TheSilentGroup.Fluorine.AMF3
{
	class ClassDefinition
	{
		private string					_className;
		private string					_mappedClassName;
		private int						_memberCount;
		private ClassMemberDefinition[]	_classMemberDefinitions;
		private bool					_externalizable;
		private bool					_dynamic;

		public ClassDefinition(string className, string mappedClassName, int memberCount, ClassMemberDefinition[] classMemberDefinitions, bool externalizable, bool dynamic)
		{
			_className = className;
			_memberCount = memberCount;
			_classMemberDefinitions = classMemberDefinitions;
			_externalizable = externalizable;
			_mappedClassName = mappedClassName;
			_dynamic = dynamic;
		}

		public string ClassName{ get{ return _className; } }

		//public string MappedClassName{ get{ return _mappedClassName; } }

		public int MemberCount{ get{ return _memberCount; } }

		public ClassMemberDefinition[] ClassMemberDefinitions{ get{ return _classMemberDefinitions; } }

		public bool IsExternalizable{ get{ return _externalizable; } }

		public bool IsDynamic{ get{ return _dynamic; } }

		public bool IsTypedObject{ get{ return (_className != null && _className != string.Empty); } }

		public object GetValue(ClassMemberDefinition classMemberDefinition, object obj)
		{
			return GetValue(classMemberDefinition.ClassMember, obj);
		}

		public object GetValue(string member, object obj)
		{
			if( obj is IDictionary )
			{
				IDictionary dictionary = obj as IDictionary;
				if( dictionary.Contains(member) )
					return dictionary[member];
			}
			PropertyInfo propertyInfo = obj.GetType().GetProperty(member);
			if( propertyInfo != null )
			{
				object propertyValue = propertyInfo.GetValue(obj, null);
				return propertyValue;
			}
			else
			{
				FieldInfo fieldInfo = obj.GetType().GetField(member);
				if( fieldInfo != null )
				{
					object propertyValue = fieldInfo.GetValue(obj);
					return propertyValue;
				}
				else
					throw new FluorineException("Class member " + member + " not found.");
			}
		}
	}

	class ClassMemberDefinition
	{
		private string	_classMember;

		public ClassMemberDefinition(string classMember)
		{
			_classMember = classMember;
		}

		public string ClassMember{ get{ return _classMember; } }
	}
}
