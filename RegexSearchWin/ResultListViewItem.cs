using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RegexSearchWin
{
    public class ResultListViewItem
    {
        public string FullPath { get; set; }
        public string FileName { get { return Path.GetFileName(FullPath); } }
        public string FolderName { get { return Path.GetDirectoryName(FullPath); } }
    }
}
