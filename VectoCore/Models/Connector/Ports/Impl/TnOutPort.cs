using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUGraz.VectoCore.Models.Connector.Ports.Impl
{
    public class TnOutPort : OutPort, ITnOutPort
    {
        public void Request(TimeSpan absTime, TimeSpan dt, float torque, float engineSpeed)
        {
            throw new NotImplementedException();
        }
    }
}
