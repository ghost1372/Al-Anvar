using System.Collections.Concurrent;
using Microsoft.Windows.ApplicationModel.Resources;

namespace AlAnvar.Common;

public static partial class Extensions
{
    private static readonly ConcurrentDictionary<string, string> cachedResources = new();
    private static readonly ResourceMap resourcesTree = new ResourceManager().MainResourceMap.TryGetSubtree("Resources");

    public static string GetLocalizedResource(this string resourceKey)
    {
        if (cachedResources.TryGetValue(resourceKey, out var value))
        {
            return value;
        }

        value = resourcesTree?.TryGetValue(resourceKey)?.ValueAsString;

        return cachedResources[resourceKey] = value ?? string.Empty;
    }
}
