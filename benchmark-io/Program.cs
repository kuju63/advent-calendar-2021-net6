﻿using BenchmarkDotNet.Running;

namespace Benchmark.IO
{
    public class Program
    {
        public static void Main()
        {
            BenchmarkRunner.Run<FileIOTest>();
        }
    }
}
