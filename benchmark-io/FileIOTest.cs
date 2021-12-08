using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using System.IO;
using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using System.Buffers;
using System.Text;

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

        private byte[] binaryData = new byte[]
        {
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09
        };

        private string textData = "abcdefghij";

        private string textFilePath = Path.GetTempFileName();

        private string binaryFilePath = Path.GetTempFileName();

        [GlobalSetup]
        public void GlobalSetup()
        {
            Task.WaitAll(
                Task.Run(async () =>
                {
                    using (var fs = new FileStream(binaryFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 64, true))
                    {
                        for (int i = 0; i < 100_000_000 / 10; i++)
                        {
                            await fs.WriteAsync(binaryData);
                        }
                    }
                }),
                Task.Run(async () =>
                {
                    using (var fs = new FileStream(textFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 64, true))
                    using (var st = new StreamWriter(fs, Encoding.UTF8))
                    {
                        for (int i = 0; i < 100_000_000 / 10; i++)
                        {
                            await st.WriteAsync(textData);
                        }
                    }
                })
            );
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            if (File.Exists(textFilePath))
            {
                File.Delete(textFilePath);
            }

            if (File.Exists(binaryFilePath))
            {
                File.Delete(binaryFilePath);
            }
        }

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
        public async Task WriteAsync()
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
        public void WriteSync()
        {
            using (var fs = new FileStream(Path.Join(tempDir, "2.txt"), FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 1, FileOptions.None))
            {
                for (int i = 0; i < 100_000_000 / 8_000; i++)
                {
                    fs.Write(writeData);
                }
            }
        }

        [Benchmark]
        public async Task ReadBinaryAsync()
        {
            using (var fs = new FileStream(binaryFilePath, FileMode.Open, FileAccess.Read, FileShare.None, 1, FileOptions.Asynchronous))
            using (var buffer = MemoryPool<byte>.Shared.Rent())
            {
                await fs.ReadAsync(buffer.Memory);
            }
        }

        [Benchmark]
        public async Task ReadToEndAsync()
        {
            using (var fs = new FileStream(textFilePath, FileMode.Open, FileAccess.Read, FileShare.None, 1, FileOptions.Asynchronous))
            using (var reader = new StreamReader(fs, System.Text.Encoding.UTF8))
            {
                await reader.ReadToEndAsync();
            }
        }
    }
}
