using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUGraz.VectoCore.Tests.Utils;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;

namespace GraphDrawer
{
    internal class Program
    {
        private const string HELP = @"
Tool for plotting graphs comparing Vecto 2.2 and Vecto 3

--split <leng> ... split input into parts of length <len> (in m), only distance output

";

        private static void Main(string[] args)
        {
            if (args.Contains("--split")) {
                Console.WriteLine("plotting graphs splitted by distance");
                var idx = Array.FindIndex(args, x => x == "--split");
                var lenght = int.Parse(args[idx + 1]);
                var success = true;
                var start = 0;
                do {
                    Console.WriteLine("plotting {0} - {1}", start / 1000, (start + lenght) / 1000);
                    success = GraphWriter.WriteDistanceSlice(args[0], args[1], start, start + lenght);
                    start += lenght;
                } while (success);
                Console.WriteLine("plotting full cycle");
                GraphWriter.Write(args[0], args[1]);
                Console.WriteLine("done");
                return;
            }
            Console.WriteLine("plotting graphs...");
            GraphWriter.Write(args[0], args[1]);
            Console.WriteLine("done");
        }
    }
}