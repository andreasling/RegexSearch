using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace RegexSearch
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Building index...");
            var sw = Stopwatch.StartNew();

            var index = BuildIndex();

            sw.Stop();
            Console.WriteLine("... done! " + sw.Elapsed);

            Console.WriteLine("Searching...");
            sw.Restart();

            index.SetSearcher(new BinarySearcher());
            var result = index.Search("Program");

            sw.Stop();
            Console.WriteLine("... done! " + sw.Elapsed);

            foreach (var item in result)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine("count: " + result.Count());
        }

        static void MainTest(string[] args)
        {
            var path =
                //"..\\..";
                //@"C:\Treserva\Dev\Prestanda1\S3\Arende\Source\Process";
                //@"C:\Treserva\Dev\Prestanda1\S3\Arende\Source\";
                @"C:\Treserva\Dev\Prestanda1\S3\";
            var pattern = "*.cs";

            var regex = new Regex(
                    //@"(?:(?<=(?:class|namespace)\s+?)[^\s:{]+?(?=[\s:{]))|(?:())", 
                    //"class", 
                    @"(?:(?<=(?:class|namespace)\s+?)(?<identifier>[a-zA-Z_][\w_]*?)(?=[\s:{]))|(?:(?<=(?<before>[{};]\s*?)(?<type>(?:[a-zA-Z_][\w_]*?\.)*?(?:[a-zA-Z_][\w_]*?)(?:\<[^\>]+?\>)?)\s+?)(?<identifier>[a-zA-Z_][\w_]*?)(?=(?<after>\s*?[;=])))", 
                    RegexOptions.Compiled | RegexOptions.Singleline);

            var sw = Stopwatch.StartNew();

            var files =
                (from file in Directory.EnumerateFiles(path, pattern, SearchOption.AllDirectories).ToArray()
                select Path.GetFullPath(file)).ToArray();

            sw.Stop();
            Console.WriteLine("Directory.EnumerateFiles: " + sw.Elapsed);
            Console.WriteLine("count: " + files.Count());

            sw.Restart();

            var fileMatches =
                (from file in files.ToArray()//.Take(20)
                select new { File = file, FileHash = file.GetHashCode(), Matches = regex.Matches(File.ReadAllText(file)) }).ToArray();

            sw.Stop();
            Console.WriteLine("regex.Matches: " + sw.Elapsed);
            sw.Restart();

            //var sb = new StringBuilder();
            //foreach (var fileMatch in fileMatches)
            //{
            //    sb.AppendFormat("{0:x8}: {1}", fileMatch.FileHash, fileMatch.File).AppendLine();
            //    //Console.WriteLine("{0:x8}: {1}", fileMatch.FileHash, fileMatch.File);
            //}

            //foreach (var fileMatch in fileMatches)
            //{
            //    foreach (Match match in fileMatch.Matches)
            //    {
            //        sb.AppendFormat("{3:x8}(@{0}+{1}): {2}", match.Index, match.Length, match.Value, fileMatch.FileHash).AppendLine();
            //        //Console.WriteLine("{3:x8}(@{0}+{1}): {2}", match.Index, match.Length, match.Value, fileMatch.FileHash);
            //    }
            //}

            //sw.Stop();
            //Console.WriteLine("sb.AppendFormat: " + sw.Elapsed);
            //sw.Restart();

            //File.WriteAllText("index.txt", sb.ToString());

            //sw.Stop();
            //Console.WriteLine("File.WriteAllText: " + sw.Elapsed);
            //sw.Restart();

            var fileSearchWords =
                (from fileMatch in fileMatches.ToArray()
                select new 
                { 
                    fileMatch.File, 
                    fileMatch.FileHash, 
                    SearchWords = 
                        from Match match in fileMatch.Matches 
                        select match.Value
                }).ToArray();

            sw.Stop();
            Console.WriteLine("fileSearchWords: " + sw.Elapsed);


            //var index = 
            //    from fileSearchWord in fileSearchWords
            //    group 
                        

            /* TODO:
             * bygg om index -> sökord - filer...
             * söka i index -> lista på filer -> söka i listade filer
             * hashade filer/ord
             */
        }

        static Index BuildIndex()
        {
            var path =
                //"..\\..";
                //@"C:\Treserva\Dev\Prestanda1\S3\Arende\Source\Process";
                //@"C:\Treserva\Dev\Prestanda1\S3\Arende\Source\";
                @"C:\Treserva\Dev\Prestanda1\S3\";
            var pattern = "*.cs";

            var regex = new Regex(
                    //@"(?:(?<=(?:class|namespace)\s+?)[^\s:{]+?(?=[\s:{]))|(?:())", 
                    //"class", 
                    @"(?:(?<=(?:class|namespace)\s+?)(?<identifier>[a-zA-Z_][\w_]*?)(?=[\s:{]))|(?:(?<=(?<before>[{};]\s*?)(?<type>(?:[a-zA-Z_][\w_]*?\.)*?(?:[a-zA-Z_][\w_]*?)(?:\<[^\>]+?\>)?)\s+?)(?<identifier>[a-zA-Z_][\w_]*?)(?=(?<after>\s*?[;=])))", 
                    RegexOptions.Compiled | RegexOptions.Singleline);


            var indexBuilder = new RegexIndexBuilder(path, pattern, regex);

            var index = indexBuilder.BuildIndex(null);

            return index;
        }
    }
}
