using System.IO;

namespace FileTreeGrids.Extensions
{
    public static class PathExtensions
    {
        public static bool IsDirectory(string path)
        {
            var attr = File.GetAttributes(path);
            return attr.HasFlag(FileAttributes.Directory);
        }
    }
}
