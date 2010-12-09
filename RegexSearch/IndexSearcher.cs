using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RegexSearch
{
    public interface IndexSearcher
    {
        void SetIndexEntries(IEnumerable<IndexEntry> indexEntries);
        IEnumerable<string> Search(string pattern);
    }
}
