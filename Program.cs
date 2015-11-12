using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SymbolFinder
{
    class Program
    {
        private const string StoreDestination = @"H:\Symbols";

        private static readonly string[] Roots =
        {
            @"S:\",
            //@"T:\",
        };

        private static readonly string[] SearchPatterns = {"*.exe", "*.pdb", "*.dll", "*.ax"};


        static void Main(string[] args)
        {
            var date = new DateTime(2015, 6, 1);
            var hash = new HashSet<string>(GetIndexedFiles());
            var allPaths = GetAllPaths(date).ToArray();
            var files = allPaths.Where(x => !hash.Contains(x)).AsParallel().ToArray();

            foreach (var file in files)
            {
                StoreSymbol(file, StoreDestination);
            }
        }

        private static IEnumerable<string> GetIndexedFiles()
        {
            return 
                from admin in new[] { Path.Combine(StoreDestination, "000Admin")}
                where Directory.Exists(admin)
                from file in Directory.EnumerateFiles(admin)
                where !file.EndsWith(".txt")
                let contents = File.ReadAllText(file)
                let items = contents.Split(',')
                let path = items[1].TrimEnd().Trim('"')
                select path;
        }

        private static IEnumerable<string> GetAllPaths(DateTime date)
        {
            return
                from root in Roots
                from searchPattern in SearchPatterns
                from folder in Directory.EnumerateDirectories(root, searchPattern)
                let pattern = "*" + Path.GetExtension(folder)
                from file in Directory.EnumerateFiles(folder, pattern, SearchOption.AllDirectories)
                where File.GetLastWriteTime(file) > date
                select file;
        }


        static void StoreSymbol(string filename, string symstore)
        {
            Console.WriteLine("Indexing: " + filename);
            var arguments = string.Format("add /r /f \"{0}\" /t \"DS\" /s \"{1}\"", filename, symstore);
            var startInfo = new ProcessStartInfo("symstore.exe", arguments)
            {
                CreateNoWindow = true, 
                WindowStyle = ProcessWindowStyle.Hidden
            };
            var process = Process.Start(startInfo);
            using (process)
            {
                process.WaitForExit();
            }
        }
    }
}
