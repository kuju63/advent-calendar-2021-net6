using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using System.IO;
using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;

namespace Benchmark.IO
{
    [Config(typeof(Config))]
    public class FileIOTest
    {
        private class Config : ManualConfig
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

        private string tempDir = Path.Join(Path.GetTempPath(), new Random().Next().ToString());

        private byte[] writeData = new byte[8_000];

        [IterationSetup]
        public void IterationSetup()
        {
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }
        }

        [IterationCleanup]
        public void IterationCleanup()
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }

        [Benchmark]
        public async Task WriteAsyncOnceAsync()
        {
            using (var fs = new FileStream(Path.Join(tempDir, "1.txt"), FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 1, FileOptions.Asynchronous))
            {
                for (int i = 0; i < 100_000_000 / 8_000; i++)
                {
                    await fs.WriteAsync(writeData);
                }
            }
        }

        [Benchmark]
        public void WriteOnce()
        {
            using (var fs = new FileStream(Path.Join(tempDir, "2.txt"), FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 1, FileOptions.None))
            {
                for (int i = 0; i < 100_000_000 / 8_000; i++)
                {
                    fs.Write(writeData);
                }
            }
        }
    }
}
