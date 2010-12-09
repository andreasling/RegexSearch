using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RegexSearch
{
    public class Index
    {
        private IndexEntry[] indexEntries;
        private IndexSearcher searcher = null;

        public Index(IEnumerable<IndexEntry> indexEntries)
        {
            this.indexEntries = indexEntries.OrderBy(s => s.Word).ToArray();
        }

        public void SetSearcher(IndexSearcher searcher)
        {
            this.searcher = searcher;
            this.searcher.SetIndexEntries(indexEntries);
        }

        public IEnumerable<string> Search(string pattern)
        {
            return searcher.Search(pattern);
        }

        private static Index empty = new Index(Enumerable.Empty<IndexEntry>());
        public static Index Empty 
        {
            get
            {
                return empty;
            } 
        }
    }
}
