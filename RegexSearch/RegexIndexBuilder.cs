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

        public Index BuildIndex(Action<int> reportProgress)
        {
            var files = Directory.GetFiles(path, filePattern, SearchOption.AllDirectories);

            var filePaths =
                from file in files.WithProgressReporting(files.Count(), progress => reportProgress(progress)).AsParallel()
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

    public static class Extensions
    {
        public static IEnumerable<T> WithProgressReporting<T>(this IEnumerable<T> sequence, long itemCount, Action<int> reportProgress)
        {
            if (sequence == null) { throw new ArgumentNullException("sequence"); }

            int completed = 0;
            foreach (var item in sequence)
            {
                yield return item;

                completed++;
                reportProgress((int)(((double)completed / itemCount) * 100));
            }
        }
    }
}
