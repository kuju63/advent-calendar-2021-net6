using BenchmarkDotNet.Attributes;
using System.IO;
using System;
using System.Threading.Tasks;

namespace Benchmark.IO
{
    [Config(typeof(Config))]
    public class FileWriteBenchmarks
    {
        private string tempDir = Path.Join(Path.GetTempPath(), new Random().Next().ToString());

        private byte[] writeData = new byte[8_000];

        [Params(1, 4096)]
        public int WriteBuffer { get; set; }

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
            using (var fs = new FileStream(Path.Join(tempDir, "1.txt"), FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, WriteBuffer, FileOptions.Asynchronous))
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
            using (var fs = new FileStream(Path.Join(tempDir, "2.txt"), FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, WriteBuffer))
            {
                for (int i = 0; i < 100_000_000 / 8_000; i++)
                {
                    fs.Write(writeData);
                }
            }
        }
    }
}
