using System.Collections.Generic;
using System.Linq;

namespace apimtemplate.Common.Helpers
{
    internal static class CollectionHelpers
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
        {
            if (collection is null) return true;
            return !collection.Any();
        }
    }
}
