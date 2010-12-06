using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RegexSearch
{
    public class IndexEntry
    {
        public string Word { get; private set; }
        public IEnumerable<string> Files { get; private set; }

        public IndexEntry(string word, IEnumerable<string> files)
        {
            Word = word;
            Files = files;
        }
    }
}
