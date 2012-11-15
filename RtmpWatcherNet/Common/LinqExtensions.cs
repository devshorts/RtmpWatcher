using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace RtmpWatcherNet.Common
{
    public static class LinqExtensions
    {
        public static string FoldToDelimitedList<TSource>(this IEnumerable<TSource> source, Func<TSource, string> formatItemFunc, string delimiter)
        {
            var s = source
                .Aggregate(new StringBuilder(), (acc, item) => acc.AppendFormat("{0}{1}", formatItemFunc(item), delimiter))
                .ToString();
            return s.EndsWith(delimiter)
                ? s.Substring(0, s.Length - delimiter.Length)
                : s;
        }
    }
}