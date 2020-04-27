using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using IP2Region.Core;
using IP2Region.Core.Models;
using System;
using System.Collections.Generic;

namespace IP2Region.Tests
{
    [SimpleJob(RuntimeMoniker.NetCoreApp30)]
    [RPlotExporter]
    public class Test
    {
        private readonly MemorySearcher _MemorySearcher;

        private readonly FileSearcher _FileSearcher;

        private readonly DbSearcher _DbSearcher;

        private readonly string _DbPath;

        private uint[] _IPArray;

        private const int IPCount = 20000;

        public Test()
        {
            _DbPath = @"D:\Project\IP2Region.Core\IP2Region.Core\ip2region.db";
            _MemorySearcher = new MemorySearcher(_DbPath);
            _FileSearcher = new FileSearcher(_DbPath);
            _DbSearcher = new DbSearcher(_DbPath);
            _IPArray = new uint[IPCount];
            Random random = new Random();
            for(int i = 0; i < _IPArray.Length; i++)
            {
                _IPArray[i] = (uint)random.Next(int.MinValue, int.MaxValue);
            }
        }

        public IEnumerable<uint> GetIp()
        {
            yield return 2312435453;
            //yield return 3443534534;
            //yield return 788899232;
        }

        [Benchmark(Baseline = true)]
        [ArgumentsSource(nameof(GetIp))]
        public DataBlock OldMemorySearch(uint ip)
        {
            try
            {
                return _DbSearcher.MemorySearch(ip);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BtreeSearch:{ip} {ex}");
                throw ex;
            }
        }


        [Benchmark]
        [ArgumentsSource(nameof(GetIp))]
        public DataBlock MemoryBtreeSeacher(uint ip)
        {

            try
            {
                return _MemorySearcher.BtreeSearch(ip);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MemoryBtreeSeacher:{ip} {ex}");
                throw ex;
            }
        }

        [Benchmark]
        [ArgumentsSource(nameof(GetIp))]
        public DataBlock MemoryBinarySearch(uint ip)
        {
            try
            {
               return _MemorySearcher.BinarySearch(ip);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MemoryBinarySearch:{ip} {ex}");
                throw ex;
            }
        }

        [Benchmark]
        [ArgumentsSource(nameof(GetIp))]
        public DataBlock FileBtreeSearch(uint ip)
        {
            try
            {
               return _FileSearcher.BtreeSearch(ip);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FileBtreeSearch:{ip} {ex}");
                throw ex;
            }
        }

        [Benchmark]
        [ArgumentsSource(nameof(GetIp))]
        public DataBlock FileBinarySearch(uint ip)
        {
            try
            {
                return _FileSearcher.BinarySearch(ip);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FileBinarySearch:{ip} {ex}");
                throw ex;
            }
        }
    }
}
