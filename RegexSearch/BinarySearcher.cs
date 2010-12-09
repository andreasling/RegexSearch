using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RegexSearch
{
    public class BinarySearcher : IndexSearcher
    {
        private IndexEntry[] indexEntries = new IndexEntry[0];
        private static IndexEntryBinarySearchComparer indexEntryBinarySearchComparer = new IndexEntryBinarySearchComparer();

        public void SetIndexEntries(IEnumerable<IndexEntry> indexEntries)
        {
            this.indexEntries = indexEntries.OrderBy(ie => ie.Word).ToArray();
        }

        public IEnumerable<string> Search(string pattern)
        {
            int index = Array.BinarySearch(indexEntries, new IndexEntry(pattern, null), indexEntryBinarySearchComparer);
            
            if (index > 0)
            {
                return indexEntries[index].Files;
            }
            else
            {
                return Enumerable.Empty<string>();
            }
        }

        class IndexEntryBinarySearchComparer : IComparer<IndexEntry>
        {
            public int Compare(IndexEntry x, IndexEntry y)
            {
                return string.Compare(x.Word, y.Word);
            }
        }
    }
}
