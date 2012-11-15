using System.Collections.Generic;

namespace RtmpWatcherNet.Common
{
	public static class CollectionUtil
	{
        public static bool IsNullOrEmpty<T>(ICollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }
	}
}
