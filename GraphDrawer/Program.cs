using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUGraz.VectoCore.Tests.Utils;

namespace GraphDrawer
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			var imgV3 = @"..\..\..\VectoCoreTest\bin\Debug\Coach_DriverStrategy_Drive_50_slope_dec-inc.vmod";
			var imgv22 =
				@"..\..\..\VectoCoreTest\TestData\Integration\DriverStrategy\Vecto2.2\Coach\24t Coach_Cycle_Drive_30_Dec_Increasing_Slope.vmod";

			GraphWriter.Write(imgV3, imgv22);
			//GraphWriter.Write(args[0], args[1]);
		}
	}
}