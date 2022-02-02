using System;
using System.Text;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace LZ77
{
    class Program
    {
        static void Main(string[] args)
        {
            new Tests().run();
        }

        public void run()
        {
            LZ77 coder = new(2048, 512);
            byte[] source = File.ReadAllBytes("/home/quantmad/test.txt");

            Stopwatch stopWatch = new ();
            stopWatch.Start();

            byte[] encoded = coder.Encode(source).Item1;

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10); 
            Console.WriteLine("Всего затрачено: " + elapsedTime);

            File.WriteAllBytes("/home/quantmad/test.lz77", encoded);
            byte[] decoded = coder.Decode(File.ReadAllBytes("/home/quantmad/test.lz77"));
            //Console.WriteLine(Encoding.Default.GetString(decoded));
            File.WriteAllBytes("/home/quantmad/test_decoded.txt", decoded);
        }
    }
}
