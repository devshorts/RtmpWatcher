using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Web.UI.WebControls;

namespace RtmpWatcherNet.Common
{
	public static class CollectionUtil
	{
		/// <summary>
		/// Returns true if the contents of two collections are the same.
		/// </summary>
		/// <typeparam name="T">The type of the items in the collections.</typeparam>
		/// <param name="collection1">First collection to compare</param>
		/// <param name="collection2">Second collection to compare</param>
		/// <returns>True if the collections are the same length and each element is the same.  False otherwise.</returns>
		public static bool AreEqual<T>(IList<T> collection1, IList<T> collection2)
		{
			if (collection1 == null && collection2 == null)
			{
				return true;
			}
			if (collection1 == null || collection2 == null)
			{
				return false;
			}
			int max = collection1.Count;
			if (max != collection2.Count)
			{
				return false;
			}
			for (int i=0; i<max; i++)
			{
				if (!Equals(collection1[i], collection2[i]))
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Creates an int array of non-null integer parameters
		/// </summary>
		public static int[] Clean(params int[] ints)
		{
			if (ints == null || ints.Length == 0)
			{
				return null;
			}

			var cleaned = new List<int>(ints.Length);
			for (int i = 0; i < ints.Length; i++)
			{
				if (ints[i] != int.MinValue)
				{
					cleaned.Add(ints[i]);
				}
			}

			return cleaned.Count == ints.Length
					?
						ints
					:
						cleaned.Count == 0
							?
								null
							:
								cleaned.ToArray();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IDictionary Clone(IDictionary source)
		{
			if (source == null)
			{
				return null;
			}

			if (source is ICloneable)
			{
				return (IDictionary)((ICloneable)source).Clone();
			}

			IDictionary target;

			try
			{
				target = (IDictionary)Activator.CreateInstance(source.GetType());
			}
			catch
			{
				target = CreateDictionary(source.Count);
			}

			foreach (DictionaryEntry entry in source)
			{
				target[entry.Key] = entry.Value;
			}

			return target;
		}

		/// <summary>
		/// Combines several arrays into a single new array.  Null arrays are ignored.
		/// </summary>
		/// <param name="arrays">Each array is combined into a single larger array.</param>
		/// <returns>An array with all elements from the passed in arrays.  If all passed in arrays are null, null is returned.</returns>
		public static T[] Combine<T>(params T[][] arrays)
		{
			int length = 0;

			foreach (T[] array in arrays)
			{
				if (array != null && array.Length > 0)
				{
					length += array.Length;
				}
			}

			var combined = new T[length];

			int index = 0;
			if (length > 0)
			{
				foreach (Array array in arrays)
				{
					if (array != null && array.Length > 0)
					{
						array.CopyTo(combined, index);
						index += array.Length;
					}
				}
			}

			return combined;
		}
		
		public static int Count<T>(IList<T> list)
		{
			if (list == null)
			{
				return -1;
			}
			return list.Count;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IDictionary CreateDictionary(int capacity)
		{
			return capacity > 8 ? (IDictionary)new Hashtable(capacity) : new ListDictionary();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IDictionary CreateLookupDictionary(ICollection keys)
		{
			IDictionary d = CreateDictionary(keys.Count);
			SetMany(d, keys, null);
			return d;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static bool Equals(IList listX, IList listY)
		{
			int i = listX.Count;

			if (listY.Count != i)
			{
				return false;
			}

			while (i-- > 0)
			{
				object x = listX[i];
				object y = listY[i];

				if (x != null)
				{
					if (!x.Equals(y))
					{
						return false;
					}
				}
				else
				{
					// x is null, so the lists are only still equal if y is also null
					if (y != null)
					{
						return false;
					}
				}
			}
			return true;
		}

		public static List<T> FindANotInB<T>(IEnumerable<T> a, IEnumerable<T> b)
		{
			if (a == null)
			{
				throw new ArgumentNullException("a");
			}
			if (b == null)
			{
				throw new ArgumentNullException("b");
			}

			var bset = new HashSet<T>(b);

			var aNotInB = new List<T>();

			foreach (T i in a)
			{
				if (!bset.Contains(i))
				{
					aNotInB.Add(i);
				}
			}
			return aNotInB;
		}

		public static T First<T>(IEnumerable<T> enumerable)
		{
			if (enumerable == null)
			{
				throw new ArgumentNullException("enumerable");
			}
			IEnumerator<T> enumerator = enumerable.GetEnumerator();
			if (enumerator.MoveNext())
			{
				return enumerator.Current;
			}
			return default(T);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static object FirstNonNull(IEnumerable enumerable)
		{
			if (enumerable == null)
			{
				throw new ArgumentNullException("enumerable");
			}
			foreach(object item in enumerable)
			{
				if (item != null)
				{
					return item;
				}
			}
			return null;
		}

		public static Array GetColumnAsArray(DataTable table, string fieldName)
		{
			if (table == null || string.IsNullOrEmpty(fieldName))
			{
				throw new ArgumentNullException(table == null ? "table" : "fieldName");
			}

			DataColumn col = table.Columns[fieldName];
			if (col == null)
			{
				var names = new string[table.Columns.Count];
				for (int i = 0; i < names.Length; i++)
				{
					names[i] = table.Columns[0].ColumnName;
				}
				throw new ArgumentException(
					String.Format(
						"Column {0} does not exist in table.  Available columns: {1}",
						fieldName,
						String.Join(", ", names)));
			}

			int max = table.Rows.Count;
			Array array = Array.CreateInstance(col.DataType, max);
			for (int i = 0; i < max; i++)
			{
				array.SetValue(table.Rows[i][col], i);
			}
			return array;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IDictionary GetDuplicates(IDictionary x, IDictionary y)
		{
			if (x == null || y == null)
			{
				return null;
			}

			IDictionary result;

			if (x.GetType() == y.GetType())
			{
				result = (IDictionary)Activator.CreateInstance(x.GetType());
			}
			else
			{
				result = CreateDictionary(Math.Min(x.Count, y.Count));
			}

			foreach (DictionaryEntry de in x)
			{
				if (y.Contains(de.Key))
				{
					result.Add(de.Key, de.Value);
				}
			}

			return result;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static Array GetDuplicateKeys(IDictionary x, IDictionary y)
		{
			IDictionary duplicates = GetDuplicates(x, y);
			if (duplicates == null || duplicates.Count == 0)
			{
				return null;
			}

			return GetKeysAsArray(duplicates);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static Array GetKeysAsArray(IDictionary dict)
		{
			if (dict == null || dict.Count == 0)
			{
				return null;
			}

			Type elementType = null;

			foreach (object key in dict.Keys)
			{
				elementType = key.GetType();
				break;
			}

			// elementType really can't be null (sam)
			// ReSharper disable AssignNullToNotNullAttribute
			Array array = Array.CreateInstance(elementType, dict.Count);
			// ReSharper restore AssignNullToNotNullAttribute
			dict.Keys.CopyTo(array, 0);

			return array;
		}

		/// <summary>
		/// Returns keys from a dictionary as an integer array.  Assumes all keys are ints.  Predates GetKeysAsArray.
		/// </summary>
		/// <param name="dict">Dictionary whose keys to retrieve.</param>
		/// <returns>Array of key values</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static int[] GetKeysAsIntArray(IDictionary dict)
		{

			var ids = new ArrayList(dict.Keys);
			return (int[])ids.ToArray(typeof(int));
		}

		/// <summary>
		/// Returns the value associated with the specified key in the dictionary, or the default value if the key does not exist.
		/// </summary>
		/// <typeparam name="TKey">Type of Key</typeparam>
		/// <typeparam name="TValue">Type of Value</typeparam>
		/// <param name="dictionary">Dictionary</param>
		/// <param name="key">Key</param>
		/// <returns>Value or default(TValue)</returns>
		/// <remarks>
		/// This method is designed to help work around KeyNotFoundException and simplify access to TryGetValue.
		/// </remarks>
		public static TValue GetValue<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key)
		{
			TValue value;

			if (dictionary == null)
			{
				throw new ArgumentNullException("dictionary");
			}

			if (dictionary.TryGetValue(key, out value))
			{
				return value;
			}
			return default(TValue);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static Array GetValuesAsArray(IDictionary dict, Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			Array values = Array.CreateInstance(type, dict.Count);
			dict.Values.CopyTo(values, 0);
			return values;
		}

		public static int Increment<TKey>(IDictionary<TKey, int> dictionary, TKey key)
		{
		    int value;

		    value = dictionary.TryGetValue(key, out value)
		                          ? value + 1
		                          : 1;

		    dictionary[key] = value;

		    return value;
		}

	    public static bool IsNullOrEmpty<T>(ICollection<T> collection)
		{
			return collection == null || collection.Count == 0;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static string Join(ICollection items, string delimiter)
		{
			bool first = true;

			var sb = new StringBuilder();
			foreach (object item in items)
			{
				if (item == null)
					continue;

				if (!first)
				{
					sb.Append(delimiter);
				}
				else
				{
					first = false;
				}
				sb.Append(item);
			}
			return sb.ToString();
		}

		public static T Last<T>(IList<T> list)
		{
			if (list == null)
			{
				throw new ArgumentNullException("list");
			}
			if (list.Count == 0)
			{
				return default(T);
			}
			return list[list.Count - 1];
	}

		public static void RemoveDuplicates<T>(IList<T> list)
		{			
			var found = new HashSet<T>();
			for(int i=list.Count-1; i > -1; i--)
			{
				T t = list[i];
				if (found.Contains(t))
				{
					list.RemoveAt(i);
				}
				else
				{
					found.Add(t);
				}
			}
		}

		public static void RemoveNulls<T>(IList<T> list) where T : class
		{
			if (list == null)
			{
				return;
			}
			int i = list.Count;
			while(i-- != 0)
			{
				if (list[i] == null)
				{
					list.Remove(list[i]);
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetMany(IDictionary dictionary, IEnumerable keys, object value)
		{
			if (dictionary == null)
			{
				throw new ArgumentNullException("dictionary", "dictionary is required.");
			}

			if (keys == null)
			{
				return;
			}

			foreach (object key in keys)
			{
				dictionary[key] = value;
			}
		}

		public static IEnumerable<T> SkipFirst<T>(IEnumerable<T> enumerable)
		{
			if (enumerable == null)
			{
				throw new ArgumentNullException("enumerable");
			}

			using(var enumerator = enumerable.GetEnumerator())
			{
				if (!enumerator.MoveNext())
				{
					yield break;
				}

				while(enumerator.MoveNext())
				{
					yield return enumerator.Current;
				}
			}
		}

		public static T[] ToArray<T>(ICollection<T> collection)
		{
			if (collection == null)
			{
				return null;
			}

			var array = new T[collection.Count];
			collection.CopyTo(array, 0);
			return array;
		}
		
		public static Dictionary<TKey, List<TValue>> ToGroupedDictionary<TKey, TValue>(IEnumerable<TValue> collection, Converter<TValue, TKey> converter)
		{
			var d = new Dictionary<TKey, List<TValue>>();

			if (collection == null)
			{
				return d;
			}

			foreach (TValue value in collection)
			{
				TKey key = converter(value);

				List<TValue> list;

				if (!d.TryGetValue(key, out list))
				{
					list = new List<TValue>();
					d[key] = list;
				}
				list.Add(value);
			}
			return d;
		}

		public static Dictionary<TKey, object> ToDictionary<TKey>(TKey[] keys)
		{
			return ToDictionary<TKey, object>(keys, null);
		}

		public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(TKey[] keys, TValue value)
		{
			if (keys == null)
			{
				return new Dictionary<TKey, TValue>(1);
			}
			var d = new Dictionary<TKey, TValue>(keys.Length);
			foreach (TKey key in keys)
			{
				d[key] = value;
			}
			return d;
		}
		public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(ICollection<TValue> collection, Converter<TValue, TKey> converter)
		{
			return ToDictionary(collection, null, converter);	
		}

		public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(ICollection<TValue> collection, IEqualityComparer<TKey> comparer, Converter<TValue, TKey> converter)
		{
			if (collection == null)
			{
				return new Dictionary<TKey, TValue>(comparer);
			}

			var d = new Dictionary<TKey, TValue>(collection.Count, comparer);
			foreach(TValue value in collection)
			{
				TKey key = converter(value);
				d[key] = value;
			}
			return d;
		}

		public static Dictionary<TKey, object> ToDictionary<TKey>(ICollection<TKey> keys)
		{
			return ToDictionary<TKey, object>(keys, null);
		}

		public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(ICollection<TKey> keys, TValue value)
		{
			if (keys == null)
			{
				return new Dictionary<TKey, TValue>(1);				
			}
			var d = new Dictionary<TKey, TValue>(keys.Count);
			foreach(TKey key in keys)
			{
				d[key] = value;
			}
			return d;
		}

		public static List<T> ToList<T>(DataTable table, string fieldName)
		{
			if (table == null || table.Rows.Count == 0)
			{
				return new List<T>(1);
			}

			var list = new List<T>(table.Rows.Count);
			DataColumn column = table.Columns[fieldName];
			var t = typeof(T);

			if (column == null)
			{
				throw new ArgumentException(String.Format("Field {0} does not exist in provided data table.", fieldName));
			}
			foreach (DataRow row in table.Rows)
			{
				object value = row[column];
				if (value == null || value == DBNull.Value)
				{
					continue;
				}

				try
				{
					list.Add((T)Convert.ChangeType(value, t));
				}
				catch (InvalidCastException ex)
				{
					throw new InvalidCastException(String.Format("Cannot convert {0} ({1}) to {2}.",
																 value,
																 value.GetType().FullName,
																 t.FullName),
												   ex);
				}
			}
			return list;
		}

		public static List<T> ToList<T>(ListItemCollection items)
		{
			if (items == null || items.Count == 0)
			{
				return new List<T>(1);
			}

			var t = typeof(T);
			var list = new List<T>(items.Count);
			foreach (ListItem item in items)
			{
				if (item == null || string.IsNullOrEmpty(item.Value))
				{
					continue;
				}

				try
				{
					list.Add((T)Convert.ChangeType(item.Value, t));
				}
				catch (InvalidCastException ex)
				{
					throw new InvalidCastException(String.Format("Cannot convert '{0}' to {1}.",
																 item.Value,
																 t.FullName),
												   ex);
				}
			}
			return list;
		}

		public static T[] Unique<T>(T[] original)
		{
			return Unique(new List<T>(original)).ToArray();
		}

		public static List<T> Unique<T>(ICollection<T> original)
		{
			var copy = new List<T>(original);
			var unique = new List<T>(copy.Count);

			bool first = true;
			T last = default(T);

			copy.Sort();

			foreach (T t in copy)
			{
				if (first)
				{
					last = t;
					first = false;
					unique.Add(t);
				}
				else if (!t.Equals(last))
				{
					unique.Add(t);
					last = t;
				}
			}
			return unique;
		}

        /// <summary>
        /// Takes a source list who's converter key is the key in the dictionary and finds the associated item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="Y"></typeparam>
        /// <param name="source"></param>
        /// <param name="dict"></param>
        /// <param name="converter"></param>
        /// <param name="associate"></param>
        public static void Associate<T, Y>(List<T> source, Dictionary<int, Y> dict, Converter<T, int> converter, Action<T, Y> associate)
        {
            foreach (var item in source)
            {
                Y associator;
                if (dict.TryGetValue(converter(item), out associator))
                {
                    associate(item, associator);
                }
            }
        }

        /// <summary>
        /// Takes a source list who's converter key is the key in the dictionary and finds the associated item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="Y"></typeparam>
        /// <param name="source"></param>
        /// <param name="dict"></param>
        /// <param name="converter"></param>
        /// <param name="associate"></param>
        public static void AssociateList<T, Y>(List<T> source, Dictionary<int, List<Y>> dict, Converter<T, int> converter, Action<T, List<Y>> associate)
        {
            foreach (var item in source)
            {
                List<Y> associator;
                if (dict.TryGetValue(converter(item), out associator))
                {
                    associate(item, associator);
                }
            }
        }
	}
}
