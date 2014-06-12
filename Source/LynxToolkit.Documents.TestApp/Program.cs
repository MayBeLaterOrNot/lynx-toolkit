
namespace LynxToolkit.Documents.TestApp
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Threading;

    class Program
    {
        private static void Main(string[] args)
        {
            var thisProc = Process.GetCurrentProcess();
            thisProc.PriorityClass = ProcessPriorityClass.RealTime;

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            Test(@"Input\Headers.wiki");
            Test(@"Input\Example.wiki");
            Console.ReadKey();
        }

        private static void Test(string fileName)
        {

            var input = File.ReadAllText(fileName);
            input = input + input;

            var wikiParser = new WikiParser(File.OpenRead) { CurrentDirectory = Path.GetDirectoryName(fileName) };
            using (var stream = File.OpenRead(fileName))
            {
                wikiParser.Parse(stream);
            }

            Console.WriteLine(fileName);
            for (int j = 0; j < 5; j++)
            {
                var w = Stopwatch.StartNew();
                int n = 1000;
                for (int i = 0; i < n; i++)
                {
                    wikiParser.ParseText(input);
                }

                var bps = input.Length * n / (w.ElapsedMilliseconds * 0.001) / 1024 / 1024;
                Console.WriteLine("  {0:0.000} Mbyte/sec", bps);
            }

            Console.WriteLine();
        }
    }
}
