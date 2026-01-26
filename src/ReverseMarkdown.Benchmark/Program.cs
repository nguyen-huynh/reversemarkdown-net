using System;
using BenchmarkDotNet.Running;
using ReverseMarkdown.Benchmark;

namespace ReverseMarkdown.Benchmark.Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<CompareBenchmark>();
            Console.ReadLine();
        }
    }
}
