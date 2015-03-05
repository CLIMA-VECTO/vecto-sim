using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUGraz.VectoCore.Models.Connector.Ports
{
    public interface ITnPort
    {
    }

    public interface ITnInPort : ITnPort
    {

    }

    public interface ITnOutPort : ITnPort
    {
        void Request(TimeSpan absTime, TimeSpan dt, double torque, double engineSpeed);
    }
}
