namespace Willow.AdminApp;

using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

/// <summary>
/// Strips off any directories until it finds a match
/// This allows an SPA to work even after page reload with some SPA path still on the URL
/// </summary>
public class IndexFallbackFileProvider : IFileProvider
{
    private readonly PhysicalFileProvider innerProvider;

    public IndexFallbackFileProvider(PhysicalFileProvider physicalFileProvider)
    {
        innerProvider = physicalFileProvider;
    }

    // For an SPA we may be called with a path like /foo/bar left over from some earlier SPA navigation
    // We need to remove these prefix directories until we find a match

    public IDirectoryContents GetDirectoryContents(string path)
    {
        string subpath = path.TrimStart(System.IO.Path.DirectorySeparatorChar);  // remove leading /
        while (true)
        {
            var result = innerProvider.GetDirectoryContents(subpath);
            if (result.Exists) return result;

            string? directory = System.IO.Path.GetDirectoryName(subpath);
            if (directory is null) break;

            int index = directory.IndexOf(System.IO.Path.DirectorySeparatorChar);
            if (index < 0) break;

            subpath = subpath.Substring(index + 1);
        }
        return innerProvider.GetDirectoryContents("");
    }

    public IFileInfo GetFileInfo(string path)
    {
        string subpath = path.TrimStart(System.IO.Path.DirectorySeparatorChar);  // remove leading /
        while (true)
        {
            var result = innerProvider.GetFileInfo(subpath);
            if (result.Exists) return result;

            string? directory = System.IO.Path.GetDirectoryName(subpath);
            if (directory is null) break;

            int index = directory.IndexOf(System.IO.Path.DirectorySeparatorChar);
            if (index < 0) break;

            // First loop might just take the leading slash

            subpath = subpath.Substring(index + 1);
        }
        return innerProvider.GetFileInfo("index.html");
    }

    public IChangeToken Watch(string filter)
    {
        return innerProvider.Watch(filter);
    }
}
