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
		private static void Main(string[] args)
		{
			GraphWriter.Write(args[0], args[1]);
		}
	}
}