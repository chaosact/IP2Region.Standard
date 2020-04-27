using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using System;
using System.Diagnostics;

namespace IP2Region.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            //Stopwatch stopwatch = new Stopwatch();
            //uint ip = 874245465;
            //Test test = new Test();
            ////test.MemoryBinarySearch(ip);
            ////test.OldMemorySearch(ip);
            ////stopwatch.Start();
            ////test.MemoryBinarySearch(ip);
            ////test.FileBinarySearch(ip);
            ////stopwatch.Stop();
            ////Console.WriteLine(stopwatch.Elapsed);
            ////stopwatch.Start();
            //////test.OldMemorySearch(4060901334U);
            //////test.MemoryBtreeSeacher(4060901334U);
            //Console.WriteLine(test.OldMemorySearch(ip));
            //Console.WriteLine(test.MemoryBtreeSeacher(ip));
            //Console.WriteLine(test.MemoryBinarySearch(ip));
            //Console.WriteLine(test.FileBtreeSearch(ip));
            //Console.WriteLine(test.FileBinarySearch(ip));
            //////test.FileBtreeSearch(4060901334U);
            ////Console.WriteLine(test.FileBtreeSearch(ip));
            ////Console.WriteLine(test.FileBinarySearch(ip));
            //stopwatch.Stop();
            ////Console.WriteLine(stopwatch.Elapsed);

            BenchmarkRunner.Run<Test>();
        }
    }
}
