using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace RegexSearch
{
    public class RegexIndexBuilder : IndexBuilder
    {
        private string path;
        private string filePattern;
        private Regex indexPattern;

        public RegexIndexBuilder(string path, string filePattern, Regex indexPattern)
        {
            this.path = path;
            this.filePattern = filePattern;
            this.indexPattern = indexPattern;
        }

        public Index BuildIndex()
        {
            var filePaths =
                from file in Directory.EnumerateFiles(path, filePattern, SearchOption.AllDirectories).AsParallel()
                select Path.GetFullPath(file);

            var fileWords = (
                from file in filePaths
                orderby file
                select new
                {
                    File = file,
                    Words = (
                        from Match m in indexPattern.Matches(File.ReadAllText(file))
                        orderby m.Value
                        select m.Value).Distinct()
                }).ToArray();

            var indexEntries =
                from word in fileWords.SelectMany(fw => fw.Words).Distinct()
                select new IndexEntry(word, from fw in fileWords where fw.Words.Contains(word) select fw.File);

            return new Index(indexEntries);
        }
    }
}
