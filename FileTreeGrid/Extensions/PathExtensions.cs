using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
