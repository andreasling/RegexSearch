﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace RegexSearchConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var path =
                //"..\\..";
                //@"C:\Users\andreas\documents\visual studio 2010\Projects\";
                @"C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\";
            var searchPattern = "*.cs";

            Console.WriteLine("Building Index...");

            Stopwatch sw = Stopwatch.StartNew();

            IDictionary<string, IEnumerable<string>> index = 
                BuildIndex(path, searchPattern);

            Console.WriteLine("Build Index: " + sw.Elapsed);
            sw.Reset();

            Console.WriteLine("Simple Searching...");

            //foreach (var i in index)
            //{
            //    Console.WriteLine(i.Key);

            //    foreach (var f in i.Value)
            //    {
            //        Console.WriteLine("\t{0:x8}", f);
            //    }
            //}

            sw.Restart();

            var simpleSearch =
                (from i in index.AsParallel()
                 where i.Key == "Program"
                 select i.Value).ToArray();

            Console.WriteLine("Simple Search: " + sw.Elapsed);
            
            //foreach (var file in simpleSearch)
            //{
            //    Console.WriteLine(file);
            //}
            Console.WriteLine("count: " + simpleSearch.Count());

            sw.Reset();

            Console.WriteLine("Regex Search...");
            sw.Start();

            var results =
                (from i in index.AsParallel()
                where Regex.IsMatch(i.Key, "About|App|Class1|Details|Edit|Form1|Global|Insert|List|Login|MainPage|Program|Register|Resources|Service|Settings|Site|SiteMaster|SomeType|TextField|UrlField")
                select i.Value).ToArray();

            Console.WriteLine("Regex Search: " + sw.Elapsed);

            //foreach (var r in results)
            //{
            //    Console.WriteLine(r);
            //}
            Console.WriteLine("count: " + results.Count());
        }

        private static IDictionary<string, IEnumerable<string>> BuildIndex(string path, string searchPattern)
        {
            var regex = new Regex(@"(?<=[\s{]class\s+?)[_a-zA-Z][_a-zA-Z\d]*?(?=[\s:{])", RegexOptions.Compiled | RegexOptions.Singleline);

            var filePaths =
                from file in Directory.EnumerateFiles(path, searchPattern, SearchOption.AllDirectories).AsParallel()
                select Path.GetFullPath(file);

            var fileTexts =
                from f in filePaths
                select new
                {
                    //Id = f.GetHashCode(),
                    Name = f,
                    Text = File.ReadAllText(f)
                };

            var fileMatches =
                from ft in fileTexts
                select new
                {
                    //ft.Id,
                    ft.Name,
                    Matches = regex.Matches(ft.Text)
                };

            //foreach (var fm in fileMatches)
            //{
            //    Console.WriteLine("{0:x8}: {1}", fm.Id, fm.Name);
            //}

            //foreach (var fm in fileMatches)
            //{
            //    foreach (Match m in fm.Matches)
            //    {
            //        Console.WriteLine("{0:x8}(@{1}+{2}): {3}", fm.Id, m.Index, m.Length, m.Value);
            //    }
            //}

            var fileWords =
                from fm in fileMatches
                select new
                {
                    //fm.Id,
                    fm.Name,
                    Words =
                        from Match m in fm.Matches
                        select m.Value
                };

            var indexStructure =
                from w in fileWords.SelectMany(fw => fw.Words).Distinct()
                select new
                {
                    Word = w,
                    Files = fileWords.Where(fw => fw.Words.Contains(w))
                };

            IDictionary<string, IEnumerable<string>> index = new Dictionary<string, IEnumerable<string>>();

            foreach (var i in indexStructure)
            {
                index.Add(i.Word, i.Files.Select(f => f.Name.ToString()).ToArray());
            }
            return index;
        }
    }
}
