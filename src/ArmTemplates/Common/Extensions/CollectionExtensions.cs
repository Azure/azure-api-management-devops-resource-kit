using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Extensions
{
    internal static class CollectionExtensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
        {
            if (collection is null) return true;
            return !collection.Any();
        }
    }
}
