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
                });

            var flatFileWords = 
                fileWords.AsParallel().
                SelectMany(fileWord => fileWord.Words, (fileWord, word) => new { fileWord.File, Word = word });

            var indexEntries =
                from flatFileWord in flatFileWords
                group flatFileWord by flatFileWord.Word into wordFiles
                select new IndexEntry(wordFiles.Key, wordFiles.Select(wordFile => wordFile.File).ToArray());

            return new Index(indexEntries.ToArray());
        }
    }
}
