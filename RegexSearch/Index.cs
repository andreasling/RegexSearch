using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RegexSearch
{
    public class Index
    {
        private IndexEntry[] searchWords;
        private string[] searchWordIndex;

        public Index(IEnumerable<IndexEntry> indexEntries)
	    {
            this.searchWords = indexEntries.OrderBy(s => s.Word).ToArray();
            this.searchWordIndex = this.searchWords.Select(s => s.Word).ToArray();
	    }
        
        public IEnumerable<string> BinarySearch(string searchWord)
        {
            int index = Array.BinarySearch(searchWordIndex, searchWord);

            if (index > 0)
            {
                return searchWords[index].Files;
            }
            else
            {
                return Enumerable.Empty<string>();
            }
        }

        public IEnumerable<string> RegexSearch(Regex pattern)
        {
            return
                (from w in searchWords
                where pattern.IsMatch(w.Word)
                select w) //.ToArray()
                .SelectMany(r => r.Files)//.Distinct().ToArray()
                ;
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
