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


            var temp =
                (from x in Enumerable.Empty<object>()
                select new { File = string.Empty, Word = string.Empty }).ToList();

            
            foreach (var fileWord in fileWords)
            {
                foreach (var word in fileWord.Words)
                {
                    temp.Add(new { File = fileWord.File, Word = word});
                }
            }

            var indexEntries = new List<IndexEntry>();
            var lastWord = temp.First().Word;
            var files = new List<string>();

            foreach (var t in temp.OrderBy(t => t.Word).ToArray())
            {
                if (t.Word != lastWord)
                {
                    indexEntries.Add(new IndexEntry(lastWord, files.ToArray()));
                    lastWord = t.Word;
                    files.Clear();
                }

                files.Add(t.File);
            }

            indexEntries.Add(new IndexEntry(lastWord, files.ToArray()));

            return new Index(indexEntries.ToArray());
        }
    }
}
