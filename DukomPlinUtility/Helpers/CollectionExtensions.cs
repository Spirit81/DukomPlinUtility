using System.Collections.Generic;

namespace DukomPlinUtility.Helpers;

public static class CollectionExtensions
{
    public static void AddIfNotExists(this ICollection<string> collection, string item)
    {
        if (collection == null) return;
        if (item == null) return;
        if (!collection.Contains(item))
        {
            collection.Add(item);
        }
    }
}
