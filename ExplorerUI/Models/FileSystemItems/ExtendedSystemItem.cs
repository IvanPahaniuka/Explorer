using FileTreeGrids.Models.FileSystemItems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ExplorerUI.Models.FileSystemItems
{
    public class ExtendedSystemItem : FileSystemItem
    {
        //Fields
        private DateTime lastWriteTime;

        //Properties
        public DateTime LastWriteTime
        {
            get => lastWriteTime;
            set
            {
                lastWriteTime = value;
                OnPropertyChanged();
            }
        }

        //Constructors
        public ExtendedSystemItem(FileSystemInfo info)
            :base(info)
        {

        }

        //Methods
        protected override void OnInfoChanged()
        {
            base.OnInfoChanged();

            LastWriteTime = Info.LastWriteTime;
        }
    }
}
