using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUGraz.VectoCore.Models.SimulationComponent;

namespace TUGraz.VectoCore.Models.Connector.Ports.Impl
{
    public class TnOutPort : OutPort, ITnOutPort
    {
        private VectoSimulationComponent _component;

        public TnOutPort(VectoSimulationComponent component)
        {
            _component = component;
        }

        public void Request(TimeSpan absTime, TimeSpan dt, double torque, double engineSpeed)
        {
            throw new NotImplementedException();
        }
    }
}
