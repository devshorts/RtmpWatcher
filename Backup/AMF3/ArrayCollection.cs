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
using System.ComponentModel;
using System.Reflection;

using com.TheSilentGroup.Fluorine;

namespace com.TheSilentGroup.Fluorine.AMF3
{
	/// <summary>
	/// Provides a type converter to convert ArrayCollection objects to and from various other representations.
	/// </summary>
	public class ArrayCollectionConverter : ArrayConverter
	{
		/// <summary>
		/// Overloaded. Returns whether this converter can convert the object to the specified type.
		/// </summary>
		/// <param name="context">An ITypeDescriptorContext that provides a format context.</param>
		/// <param name="destinationType">A Type that represents the type you want to convert to.</param>
		/// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == null)
				throw new ArgumentNullException("destinationType");

			if( destinationType == typeof(ArrayCollection) )
				return true;
			if( destinationType.IsArray )
				return true;
			if( destinationType == typeof(ArrayList) )
				return true;
			if( destinationType == typeof(IList) )
				return true;
			Type typeIList = destinationType.GetInterface("System.Collections.IList");
			if(typeIList != null)
				return true;
			//generic interface
			Type typeGenericICollection = destinationType.GetInterface("System.Collections.Generic.ICollection`1");
			if (typeGenericICollection != null)
				return true;

			return base.CanConvertTo(context, destinationType);
		}
		/// <summary>
		/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
		/// </summary>
		/// <param name="context">An ITypeDescriptorContext that provides a format context.</param>
		/// <param name="culture">A CultureInfo object. If a null reference (Nothing in Visual Basic) is passed, the current culture is assumed.</param>
		/// <param name="value">The Object to convert.</param>
		/// <param name="destinationType">The Type to convert the value parameter to.</param>
		/// <returns>An Object that represents the converted value.</returns>
		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == null)
				throw new ArgumentNullException("destinationType");

			if( destinationType == typeof(ArrayCollection) )
				return value;
			if( destinationType.IsArray )
			{
				return (value as ArrayCollection).ToArray();
			}
			if( destinationType == typeof(ArrayList) )
			{
				return (value as ArrayCollection).List;
			}
			if( destinationType == typeof(IList) )
			{
				return (value as ArrayCollection).List;
			}
			//generic interface
			Type typeGenericICollection = destinationType.GetInterface("System.Collections.Generic.ICollection`1");
			if (typeGenericICollection != null)
			{
				object obj = TypeHelper.CreateInstance(null, destinationType);
				MethodInfo miAddCollection = destinationType.GetMethod("Add");
				if (miAddCollection != null)
				{
					ParameterInfo[] parameters = miAddCollection.GetParameters();
					if(parameters != null && parameters.Length == 1)
					{
						Type parameterType = parameters[0].ParameterType;
						if (parameterType != typeof(object))
						{
							IList list = (IList) value;
							for (int i = 0; i < (value as IList).Count; i++)
							{
								miAddCollection.Invoke(obj, new object[] { TypeHelper.ChangeType(null, list[i], parameterType) });
							}
							return obj;
						}
					}
				}
			}
			Type typeIList = destinationType.GetInterface("System.Collections.IList");
			if(typeIList != null)
			{
				object obj = TypeHelper.CreateInstance(null, destinationType);
				IList list = obj as IList;
				for(int i = 0; i < (value as ArrayCollection).List.Count; i++)
				{
					list.Add( (value as ArrayCollection).List[i] );
				}
				return obj;
			}
			return base.ConvertTo (context, culture, value, destinationType);
		}
	}

	/// <summary>
	/// Flex ArrayCollection class. The ArrayCollection class is a wrapper class that exposes an Array as a collection.
	/// </summary>
	[TypeConverter(typeof(ArrayCollectionConverter))]
	public class ArrayCollection : IExternalizable, IList
	{
		private IList _list;

		/// <summary>
		/// Initializes a new instance of the ArrayCollection class.
		/// </summary>
		public ArrayCollection()
		{
			_list = null;
		}
		/// <summary>
		/// Initializes a new instance of the ArrayCollection class.
		/// </summary>
		/// <param name="list"></param>
		public ArrayCollection(IList list)
		{
			_list = list;
		}

		public int Count
		{
			get{ return _list == null ? 0 : _list.Count; }
		}

		public IList List
		{
			get{ return _list ; }
		}

		public object[] ToArray()
		{
			if( _list != null )
			{
				if( _list is ArrayList )
				{
					return ((ArrayList)_list).ToArray();
				}
				else
				{
					object[] objArray = new object[_list.Count];
					for(int i = 0; i < _list.Count; i++ )
						objArray[i] = _list[i];
					return objArray;
				}
			}
			return null;
		}

		#region IExternalizable Members

		public void ReadExternal(IDataInput input)
		{
			//_list = input.ReadObject() as IList;
			_list = new ArrayList(input.ReadObject() as IList);
		}

		public void WriteExternal(IDataOutput output)
		{
			//output.WriteObject(_list);
			output.WriteObject(ToArray());
		}

		#endregion

		#region IList Members

		public bool IsReadOnly
		{
			get
			{
				return _list.IsReadOnly;
			}
		}

		public object this[int index]
		{
			get
			{
				return _list[index];
			}
			set
			{
				_list[index] = value;
			}
		}

		public void RemoveAt(int index)
		{
			_list.RemoveAt(index);
		}

		public void Insert(int index, object value)
		{
			_list.Insert(index, value);
		}

		public void Remove(object value)
		{
			_list.Remove(value);
		}

		public bool Contains(object value)
		{
			return _list.Contains(value);
		}

		public void Clear()
		{
			_list.Clear();
		}

		public int IndexOf(object value)
		{
			return _list.IndexOf(value);
		}

		public int Add(object value)
		{
			return _list.Add(value);
		}

		public bool IsFixedSize
		{
			get
			{
				return _list.IsFixedSize;
			}
		}

		#endregion

		#region ICollection Members

		public bool IsSynchronized
		{
			get
			{
				return _list.IsSynchronized;
			}
		}

		public void CopyTo(Array array, int index)
		{
			_list.CopyTo(array, index);
		}

		public object SyncRoot
		{
			get
			{
				return _list.SyncRoot;
			}
		}

		#endregion

		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		#endregion
	}
}
