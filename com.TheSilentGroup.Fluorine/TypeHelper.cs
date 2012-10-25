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
using System.ComponentModel;
using System.Collections;
using System.Reflection;
using System.IO;
using System.Linq;
// Import log4net classes.
using log4net;
using log4net.Config;

namespace com.TheSilentGroup.Fluorine
{
	/// <summary>
	/// This type supports the Fluorine infrastructure and is not intended to be used directly from your code.
	/// </summary>
	public sealed class TypeHelper
	{
		private static ILog _log;

		static TypeHelper()
		{
			try
			{
				_log = LogManager.GetLogger(typeof(TypeHelper));
			}
			catch{}
		}

		static public Type Locate(string typeName)
		{
			if( typeName == null || typeName == string.Empty )
				return null;
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (int i = 0; i < assemblies.Length; i++)
			{
				Assembly assembly = assemblies[i];
				Type type = assembly.GetType(typeName, false);
				if (type != null)
					return type;
			}
			return null;
		}

		static public Type Locate(IApplicationContext applicationContext, string typeName, string lac)
		{
			if( lac == null  )
				return null;
			if( typeName == null || typeName == string.Empty )
				return null;
			foreach (string file in Directory.GetFiles(lac, "*.dll"))
			{
				try
				{
					Assembly assembly = Assembly.LoadFrom(file);
					Type type = assembly.GetType(typeName, false);
					if (type != null)
						return type;
				}
                catch(BadImageFormatException ex)
                {
                    // only suppress native loading of dll's since they won't have serializable types
                    // so if the exception doesn't contain this error then log it
                    if(!ex.Message.Contains("The module was expected to contain an assembly manifest"))
                    {
                        LogDllLoadError(ex.Message);
                    }
                }
				catch(Exception ex)
				{
					LogDllLoadError(ex.Message);
				}
			}
			return null;
		}

        private static void LogDllLoadError(string message)
        {
            if (_log != null && _log.IsWarnEnabled)
            {
                _log.Warn("Failed to load a dll from LAC.");
                _log.Warn(message);
            }
        }

		static public Type[] SearchAllTypes(IApplicationContext applicationContext, string lac, Hashtable excludedBaseTypes)
		{
			ArrayList result = new ArrayList();
			foreach (string file in Directory.GetFiles(lac, "*.dll"))
			{
				try
				{
					Assembly assembly = Assembly.LoadFrom(file);
					if (assembly == Assembly.GetExecutingAssembly())
						continue;
					foreach (Type type in assembly.GetTypes())
					{
						if (excludedBaseTypes != null)
						{
							if (excludedBaseTypes.ContainsKey(type))
								continue;
							if (type.BaseType != null && excludedBaseTypes.ContainsKey(type.BaseType))
								continue;
						}
						result.Add(type);
					}
				}
				catch(Exception ex)
				{
					if( _log != null && _log.IsWarnEnabled )
					{
						_log.Warn("Failed to load a dll from LAC.");
						_log.Warn(ex.Message);
					}
				}			
			}
			return (Type[])result.ToArray(typeof(Type));
		}

		static public object ChangeType(IApplicationContext applicationContext, object value, Type targetType)
		{
			if (value == null)
			{
				return GetNullValue(applicationContext, targetType);
				//return value;
			}

			Type valueType = value.GetType();

			if( valueType == targetType )//Skip further adapting
				return value;

			if (targetType.IsEnum)
			{
				if( value is string )
					return Enum.Parse(targetType, (string)value, true);
				if (value is int)
					return Enum.ToObject(targetType, (int)value);
				// SAM: support converting double do enum
				if (value is double)
					return Enum.ToObject(targetType, Convert.ToInt32(value));				
			}

			if (value is string && targetType == typeof(Guid))
			{
				try
				{
					return new Guid((string) value);
				} 
				catch(FormatException ex)
				{
					throw new FormatException(String.Format("Can not convert string value '{0}' to Guid.", value), ex);
				}
			}

			if (value is IConvertible)
			{
				try
				{
					if (!IsNullable(targetType))
						return Convert.ChangeType(value, targetType);
					else
					{
						if (value == null)
							return null;
						Type[] arguments = GetGenericArguments(targetType);
						if (arguments != null && arguments.Length > 0)
							return ChangeType(applicationContext, value, arguments[0]);
					}
				}
				catch (Exception ex)
				{
					if(_log != null && _log.IsErrorEnabled)
					{
						_log.Error(string.Format("Could not change type {0} to generic type {1}.", valueType.FullName, targetType.FullName), ex);
					}
				}
			}

			if (value is ArrayList && targetType.IsArray)
			{
				return ((ArrayList)value).ToArray(targetType.GetElementType());
			}
            if (targetType == typeof(ArrayList))
            {
                if (valueType.IsArray)
                    return ArrayList.Adapter(value as IList);
            }

			// SAM: Convert arrays
			if (valueType.IsArray && targetType.IsArray)
			{
				Type elementType = targetType.GetElementType();
				Array source = (Array) value;
				Array result = Array.CreateInstance(elementType, source.Length);
				for(int i=0; i<source.Length; i++)
				{
					try
					{
						object item = ChangeType(applicationContext, source.GetValue(i), elementType);
						result.SetValue(item, i);
					}
					catch(Exception ex)
					{
						if(_log != null && _log.IsErrorEnabled)
						{
							_log.Error(
								string.Format(
									"Could not convert array of type {0} with {1} items to array of type {2} since value {3} was not converted properly.",
									valueType.FullName,
									source.Length,
									targetType.FullName,
									source.GetValue(i)),
								ex);
							result = null;
							break;
						}
					}
				}
				if (result != null)
				{
					return result;
				}
			}

            // RYAN: Convert Hashtable to array (due to AMF0 representations of arrays)
            if(valueType == typeof(Hashtable) && targetType.IsArray)
            {
                var result = TryHashtableToArray(applicationContext, value, targetType);
                if(result != null)
                {
                    return result;
                }
            }

			if (IsGenericType(targetType))
			{
				object obj = CreateInstance(applicationContext, targetType);
				if (obj != null)
				{
					if (value is IList)
					{
						Type[] typeParameters = GetGenericArguments(targetType);
						if (typeParameters != null && typeParameters.Length == 1)
						{
							//For generic interfaces, the name parameter is the mangled name, ending with a grave accent (`) and the number of type parameters
							Type typeGenericICollection = targetType.GetInterface("System.Collections.Generic.ICollection`1");
							if (typeGenericICollection != null)
							{
								MethodInfo miAddCollection = targetType.GetMethod("Add");
								if (miAddCollection != null)
								{
									for (int i = 0; i < (value as IList).Count; i++)
									{
										miAddCollection.Invoke(obj, new object[] { ChangeType(applicationContext, (value as IList)[i], typeParameters[0]) });
									}
								}
							}
							else
							{
								if(_log != null && _log.IsErrorEnabled)
									_log.Error(string.Format("Generic type {0} does not implement System.Collections.Generic.ICollection interface.", targetType.FullName));
							}
						}
						else
						{
							if(_log != null && _log.IsErrorEnabled)
								_log.Error(string.Format("{0} type arguments of the generic type {1} expecting 1.", typeParameters.Length, targetType.FullName));
						}
						return obj;
					}
					if (value is IDictionary)
					{
						Type[] typeParameters = GetGenericArguments(targetType);
						if (typeParameters != null && typeParameters.Length == 2)
						{
							//For generic interfaces, the name parameter is the mangled name, ending with a grave accent (`) and the number of type parameters
							Type typeGenericIDictionary = targetType.GetInterface("System.Collections.Generic.IDictionary`2");
							if (typeGenericIDictionary != null)
							{
								MethodInfo miAddCollection = targetType.GetMethod("Add");
								if (miAddCollection != null)
								{
									IDictionary dictionary = value as IDictionary;
									foreach (DictionaryEntry entry in dictionary)
									{
										miAddCollection.Invoke(obj, new object[] { 
																					 ChangeType(applicationContext, entry.Key, typeParameters[0]),
																					 ChangeType(applicationContext, entry.Value, typeParameters[1]) 
																				 });
									}
								}
							}
							else
							{
								if(_log != null && _log.IsErrorEnabled)
									_log.Error(string.Format("Generic type {0} does not implement System.Collections.Generic.ICollection interface.", targetType.FullName));
							}
							return obj;
						}
						else
						{
							if(_log != null && _log.IsErrorEnabled)
								_log.Error(string.Format("{0} type arguments of the generic type {1} expecting 1.", typeParameters.Length, targetType.FullName));
						}
						return obj;
					}
					if (_log != null && _log.IsErrorEnabled)
						_log.Error(string.Format("Could not change type {0} to generic type {1}.", valueType.FullName, targetType.FullName));
				}
			}

            if (value is IDictionary && targetType.GetInterface("IDictionary") != null )
            {
                object obj = CreateInstance(applicationContext, targetType);
				IDictionary dictionary = value as IDictionary;
                foreach (DictionaryEntry entry in dictionary)
                {
                    (obj as IDictionary).Add(entry.Key, entry.Value);
                }
                return obj;
            }

			TypeConverter typeConverter = TypeDescriptor.GetConverter(targetType);
			if( typeConverter != null && typeConverter.CanConvertFrom(valueType) )
				return typeConverter.ConvertFrom(value);
			typeConverter = TypeDescriptor.GetConverter(value);
			if( typeConverter != null && typeConverter.CanConvertTo(targetType) )
				return typeConverter.ConvertTo(value, targetType);

			return value;
		}

		static public bool IsNullable(Type type)
		{
			PropertyInfo piIsGenericType = type.GetType().GetProperty("IsGenericType");
			if (piIsGenericType != null)
			{
				//.NET 2 here
				bool isGenericType = (bool)piIsGenericType.GetValue(type, null);
				return isGenericType && type.Name.StartsWith("Nullable");
				//return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
			}
			return false;
		}

        static public Array TryHashtableToArray(IApplicationContext applicationContext, object value, Type targetType)
        {
            try
            {
                var source = (Hashtable) value;
                var valueType = value.GetType();

                // Get the largest index from the keys and make an array of that size. AMF0 arrays can be saprse
                Type elementType = targetType.GetElementType();

                Array newArray = Array.CreateInstance(elementType, source.Count);

                // Loop through the values of the Hashtable, converting them to elementType and putting
                // them in the new array
                foreach(string stringKey in source.Keys)
                {
                    var intKey = int.Parse(stringKey);

                    try
                    {
                        object item = ChangeType(applicationContext, source[stringKey], elementType);

                        if (intKey > newArray.Length)
                        {
                            // Turns out the array length assumption wasn't long enough, expand the array to compensate
                            var newNewArray = Array.CreateInstance(elementType, intKey);
                            Array.Copy(newArray, newNewArray, newArray.Length);
                            newArray = newNewArray;
                        }

                        newArray.SetValue(item, intKey);
                    }
                    catch (Exception ex)
                    {
                        if (_log != null && _log.IsErrorEnabled)
                        {
                            _log.Error(
                                string.Format(
                                    "Could not convert array of type {0} with {1} items to array of type {2} since value {3} was not converted properly.",
                                    valueType.FullName,
                                    source.Count,
                                    targetType.FullName,
                                    source[stringKey]),
                                ex);
                        }
                        newArray = null;
                        break;
                    }
                }
                if (newArray != null)
                {
                    return newArray;
                }
            }
            catch (Exception e)
            {
                // An assumption or conversion failed.
                return null;
            }
            return null;
        }


        static public bool IsInt(string s)
        {
            int discard;
            return int.TryParse(s, out discard);
        }

		static public bool IsGenericTypeDefinition(Type type)
		{
			PropertyInfo piIsGenericTypeDefinition = type.GetType().GetProperty("IsGenericTypeDefinition");
			if (piIsGenericTypeDefinition != null)
			{
				//.NET 2 here
				bool isGenericTypeDefinition = (bool)piIsGenericTypeDefinition.GetValue(type, null);
				return isGenericTypeDefinition;
			}
			return false;
		}

		static public Type GetGenericTypeDefinition(Type type)
		{
			MethodInfo miGetGenericTypeDefinition = type.GetType().GetMethod("GetGenericTypeDefinition");
			if (miGetGenericTypeDefinition != null)
			{
				//.NET 2 here
				Type genericTypeDefinition = miGetGenericTypeDefinition.Invoke(type, new object[] { }) as Type;
				return genericTypeDefinition;
			}
			return null;
		}

		static public bool IsGenericType(Type type)
		{
			PropertyInfo piIsGenericType = type.GetType().GetProperty("IsGenericType");
			if (piIsGenericType != null)
			{
				//.NET 2 here
				bool isGenericType = (bool)piIsGenericType.GetValue(type, null);
				return isGenericType;
			}
			return false;
		}

		static public Type[] GetGenericArguments(Type type)
		{
			MethodInfo miGetGenericArguments = type.GetType().GetMethod("GetGenericArguments");
			if (miGetGenericArguments != null)
			{
				//.NET 2 here
				Type[] genericArguments = miGetGenericArguments.Invoke(type, new object[] { }) as Type[];
				return genericArguments;
			}
			return null;
		}

		internal static Type MakeGenericType(Type genericTypeDefinition, Type[] typeParameters)
		{
			MethodInfo miMakeGenericType = genericTypeDefinition.GetType().GetMethod("MakeGenericType");
			if (miMakeGenericType != null)
			{
				//.NET 2 here
				Type constructed = miMakeGenericType.Invoke(genericTypeDefinition, new object[] { typeParameters }) as Type;
				return constructed;
			}
			return null;
		}

		public static bool SkipMethod(MethodInfo methodInfo)
		{
			if (methodInfo.ReturnType == typeof(System.IAsyncResult))
				return true;
			foreach (ParameterInfo parameterInfo in methodInfo.GetParameters())
			{
				if (parameterInfo.ParameterType == typeof(System.IAsyncResult))
					return true;
			}
			return false;
		}

		public static string GetDescription(Type type)
		{
			AttributeCollection attributes = new AttributeCollection((Attribute[])type.GetCustomAttributes(typeof(DescriptionAttribute), false));
			if (attributes != null && attributes.Count > 0)
				return (attributes[0] as DescriptionAttribute).Description;
			return null;
		}

		public static string GetDescription(MethodInfo methodInfo)
		{
			AttributeCollection attributes = new AttributeCollection((Attribute[])methodInfo.GetCustomAttributes(typeof(DescriptionAttribute), false));
			if (attributes != null && attributes.Count > 0)
				return (attributes[0] as DescriptionAttribute).Description;
			return null;
		}

		internal static void NarrowValues(IApplicationContext applicationContext, object[] values, ParameterInfo[] parameterInfos)
		{
			//Narrow down convertibe types (double for example)
			for (int i = 0; values != null && i < values.Length; i++)
			{
				object value = values[i];
				values[i] = TypeHelper.ChangeType(applicationContext, value, parameterInfos[i].ParameterType);
			}
		}

		internal static object GetNullValue(IApplicationContext applicationContext, Type targetType)
		{
			if( applicationContext != null && applicationContext.NullableValues != null )
			{
				if( applicationContext.NullableValues.ContainsKey(targetType) )
					return applicationContext.NullableValues[targetType];
			}
			return null;
		}

		internal static object CreateInstance(IApplicationContext applicationContext, Type type)
		{
			//Is this a generic type definition?
			if (IsGenericType(type))
			{
				Type genericTypeDefinition = GetGenericTypeDefinition(type);
				// Get the generic type parameters or type arguments.
				Type[] typeParameters = GetGenericArguments(type);

				// Construct an array of type arguments to substitute for 
				// the type parameters of the generic class.
				// The array must contain the correct number of types, in 
				// the same order that they appear in the type parameter 
				// list.
				// Construct the type Dictionary<String, Example>.
				Type constructed = MakeGenericType(genericTypeDefinition, typeParameters);
				object obj = Activator.CreateInstance(constructed);
				if (obj == null)
				{
					if(_log != null && _log.IsErrorEnabled)
					{
						string msg = string.Format("Could not instantiate the generic type {0}.", type.FullName);
						_log.Error(msg);
					}
				}
				return obj;
			}
			else
				return Activator.CreateInstance(type);
		}

		public static bool DetectMono()
		{
			return (typeof(object).Assembly.GetType("System.MonoType") != null);
		}

		public static bool IsAssignable(IApplicationContext applicationContext, object obj, Type targetType)
		{
			if( obj != null && targetType.IsAssignableFrom( obj.GetType() ) )
				return true;//targetType can be assigned from an instance of the obj's Type
			
			if( obj != null )
			{
				TypeConverter typeConverter = TypeDescriptor.GetConverter(obj);
				if( typeConverter != null && typeConverter.CanConvertTo(targetType) )
					return true;
				typeConverter = TypeDescriptor.GetConverter(targetType);
				if( typeConverter != null && typeConverter.CanConvertFrom(obj.GetType()) )
					return true;
				if (IsGenericType(targetType))//Skip checking for generics now.
					return true;

				if (obj is ArrayList && targetType.IsArray)
					return true;
                if (obj.GetType().IsArray && (targetType == typeof(ArrayList)))
                    return true;

                if (obj is IDictionary && targetType.IsSubclassOf(typeof(IDictionary)) )
                    return true;

				if (targetType.IsEnum && obj is int)
					return true;

                if (obj is IConvertible)
                {
                    try
                    {
                        object conv = (obj as IConvertible).ToType(targetType, null);
                        return true;
                    }
                    catch (InvalidCastException)
                    {
                        return false;
                    }
                }

				return false;
			}
			else
			{
				if(IsNullable(targetType))
					return true;//null is assignable to a nullable type
				if( targetType is System.Data.SqlTypes.INullable )
					return true;
				if( targetType.IsValueType )
				{
					if( applicationContext != null && applicationContext.AcceptNullValueTypes )
					{
						// Any value-type that is not explicitly initialized with a value will 
						// contain the default value for that object type.
						return true;
					}
					return false;
				}
				if (IsGenericType(targetType))//Skip checking for generics now.
					return true;
				return true;
			}
		}
	}
}
