using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data.Engine
{
	public class FullLoadCurve
	{
	    public static FullLoadCurve ReadFromFile(string fileName)
	    {
	        return ReadFromJson(File.ReadAllText(fileName));
	    }

	    public static FullLoadCurve ReadFromJson(string json)
	    {
            //todo: implement ReadFromJson
            throw new NotImplementedException();
	        return new FullLoadCurve();
	    }

	}
}
