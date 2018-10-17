using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuromoji.NET.Compile
{
    public static class ProgressLog
    {
        const string DateFormat = "HH:mm:ss";

        static int Indent { get; set; }

        static bool AtEOL { get; set; }

        static Dictionary<int, long> StartTimes { get; } = new Dictionary<int, long>();

        public static void Begin(string message)
        {
            NewLine();
            Console.Write(Leader() + message + "... ");
            AtEOL = true;
            Indent++;
            StartTimes[Indent] = DateTime.Now.Ticks;
        }

        public static void End()
        {
            NewLine();
            var start = StartTimes[Indent];
            Indent = Math.Max(0, Indent - 1);
            Console.WriteLine(Leader() + "done" + (start != 0 ? new TimeSpan(DateTime.Now.Ticks - start).ToString(@"\[hh\:mm\:ss\]") : ""));
        }

        public static void Println(string message)
        {
            NewLine();
            Console.WriteLine(Leader() + message);
        }

        static void NewLine()
        {
            if (AtEOL)
            {
                Console.WriteLine();
            }
            AtEOL = false;
        }

        static string Leader()
        {
            return "[KUROMOJI] " + DateTime.Now.ToString(DateFormat) + ": " + string.Join("", Enumerable.Repeat("    ", Indent));
        }
    }
}
