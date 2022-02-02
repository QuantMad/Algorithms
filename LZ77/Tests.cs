using System.IO;
using System.Diagnostics;

namespace LZ77;

public class Tests
{
    public void run()
    {
        string[] sources = Directory.GetFiles("./source");
        Stopwatch watch = new();
        foreach (var file in sources)
        {
            Console.WriteLine("Processing... " + file);
            LZ77 coder = new(2048, 512);
            byte[] source = File.ReadAllBytes(file);
            watch.Start();
            (byte[] encoded, TimeSpan forMatching, TimeSpan forShifts) = coder.Encode(source);
            watch.Stop();
            string encodedPath = $"./compressed/{Path.GetFileName(file)}.lz77";
            File.WriteAllBytes(encodedPath, encoded);
            Console.WriteLine("Compressed to: " + encodedPath);
            byte[] decoded = coder.Decode(File.ReadAllBytes(encodedPath));
            string decodedPath = $"./decompressed/{Path.GetFileName(file)}";
            File.WriteAllBytes(decodedPath, decoded);
            Console.WriteLine("Decompressed to: " + decodedPath);
            var sourceSize = new FileInfo(file).Length;
            var encodedSize = new FileInfo(encodedPath).Length;
            var decodedSize = new FileInfo(decodedPath).Length;
            Console.WriteLine();
            Console.WriteLine($"source\tencoded\tdifference\tdecoded\tfor matching\tfor shifting\tencoding elapsed");
            Console.WriteLine(
                $"{sourceSize}\t{encodedSize}\t" +
                $"{sourceSize - encodedSize}\t\t{decodedSize}\t" +
                $"{TimeSpanToString(forMatching)}\t{TimeSpanToString(forShifts)}\t{TimeSpanToString(watch.Elapsed)}");
            Console.WriteLine("-------------------------------");
            watch.Reset();
        }
    }
    private string TimeSpanToString(TimeSpan ts)
    {
        return String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
    }
}
