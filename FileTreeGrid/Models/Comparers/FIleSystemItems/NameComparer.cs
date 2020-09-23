using FileTreeGrids.Models.FileSystemItems;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FileTreeGrid.Models.Comparers.FIleSystemItems
{
    public class NameComparer : IComparer<FileSystemItem>
    {
        //Methods
        public int Compare([AllowNull] FileSystemItem x, [AllowNull] FileSystemItem y)
        {
            if (x == null)
                return -1;
            if (y == null)
                return 1;

            if (x.IsDirectory && !y.IsDirectory)
                return -1;

            if (!x.IsDirectory && y.IsDirectory)
                return 1;

            return x.Name.CompareTo(y.Name);
        }
    }
}
