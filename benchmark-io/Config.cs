using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;

namespace Benchmark.IO
{
    public class Config : ManualConfig
    {
        public Config()
        {
            AddJob(
            new Job
            {
                Environment =
                {
                        Runtime = CoreRuntime.Core31
                },
                Meta =
                {
                        Baseline = true
                }
            },
            new Job
            {
                Environment =
                {
                        Runtime = CoreRuntime.Core50
                }
            },
            new Job
            {
                Environment =
                {
                        Runtime = CoreRuntime.Core60
                }
            });
            AddDiagnoser(MemoryDiagnoser.Default, ThreadingDiagnoser.Default);
            AddExporter(MarkdownExporter.GitHub);
        }
    }
}