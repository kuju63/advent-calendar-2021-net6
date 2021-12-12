using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Benchmark.IO
{
    [Config(typeof(Config))]
    public class FileReadBenchmarks
    {
        private static readonly byte[] binaryData = new byte[]
        {
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09
        };

        private static readonly string textData = "abcdefghij";

        private string textFilePath = Path.GetTempFileName();

        private string binaryFilePath = Path.GetTempFileName();

        [Params(1, 4096)]
        public int ReadBuffer { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            Task.WaitAll(
                Task.Run(async () =>
                {
                    using (var fs = new FileStream(binaryFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 128, true))
                    {
                        for (int i = 0; i < 100_000_000 / 10; i++)
                        {
                            await fs.WriteAsync(binaryData);
                        }
                    }
                }),
                Task.Run(async () =>
                {
                    using (var fs = new FileStream(textFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 128, true))
                    {
                        for (int i = 0; i < 100_000_000 / 10; i++)
                        {
                            await fs.WriteAsync(Encoding.UTF8.GetBytes(textData));
                            await fs.WriteAsync(Encoding.UTF8.GetBytes(Environment.NewLine));
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

        [Benchmark]
        public async Task ReadBinaryAsync()
        {
            using (var fs = new FileStream(binaryFilePath, FileMode.Open, FileAccess.Read, FileShare.None, ReadBuffer, FileOptions.Asynchronous))
            using (var buffer = MemoryPool<byte>.Shared.Rent())
            {
                while (fs.Length > fs.Position)
                {
                    await fs.ReadAsync(buffer.Memory);
                }
            }
        }

        [Benchmark]
        public async Task ReadToEndAsync()
        {
            using (var fs = new FileStream(textFilePath, FileMode.Open, FileAccess.Read, FileShare.None, ReadBuffer, FileOptions.Asynchronous))
            using (var reader = new StreamReader(fs, System.Text.Encoding.UTF8))
            {
                await reader.ReadToEndAsync();
            }
        }

        [Benchmark]
        public async Task ReadLineAsync()
        {
            using (var fs = new FileStream(textFilePath, FileMode.Open, FileAccess.Read, FileShare.None, ReadBuffer, FileOptions.Asynchronous))
            using (var reader = new StreamReader(fs, System.Text.Encoding.UTF8))
            {
                while (!reader.EndOfStream)
                {
                    await reader.ReadLineAsync();
                }
            }
        }
    }
}