using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RegexSearch
{
    public class RegexSearcher : IndexSearcher
    {
        private IEnumerable<IndexEntry> indexEntries = Enumerable.Empty<IndexEntry>();

        public void SetIndexEntries(IEnumerable<IndexEntry> indexEntries)
        {
            this.indexEntries = indexEntries;
        }

        public IEnumerable<string> Search(string pattern)
        {
            var regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.Singleline);

            return
                (from ie in indexEntries
                where regex.IsMatch(ie.Word)
                select ie) //.ToArray()
                .SelectMany(r => r.Files)
                .Distinct()
                //.ToArray()
                ;
        }
    }
}
