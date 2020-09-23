using FileTreeGrids.Extensions;
using System;
using System.IO;

namespace FileTreeGrids.Models.FileSystemItems
{
    public partial class FileSystemItem
    {
        //Static methods
        public static T Create<T>(string path) where T: FileSystemItem
        {
            var info = FileSystemInfoExtensions.GetInfo(path);
            return Create<T>(info);
        }
        public static FileSystemItem Create(Type itemType, string path)
        {
            var info = FileSystemInfoExtensions.GetInfo(path);
            return Create(itemType, info);
        }
        public static T Create<T>(FileSystemInfo info) where T : FileSystemItem
        {
            return Create(typeof(T), info) as T;
        }
        public static FileSystemItem Create(Type itemType, FileSystemInfo info)
        {
            if (!itemType.IsCompatible<FileSystemItem>())
                throw new ArgumentException();

            var item = Activator.CreateInstance(itemType, info) as FileSystemItem;
            return item;
        }
        
        public static void FixItemState(FileSystemItem item, FileSystemItem parent)
        {
            if (item == null || parent == null)
                return;

            item.IsHidden = !parent.IsActive || parent.IsHidden;
            item.Level = parent.Level + 1;
        }
        private static void FixItemState(FileSystemItem item, FileSystemItemMemento parent)
        {
            if (item == null)
                return;

            item.IsHidden = !parent.isActive || parent.isHidden;
            item.Level = parent.level + 1;
        }
    }
}
