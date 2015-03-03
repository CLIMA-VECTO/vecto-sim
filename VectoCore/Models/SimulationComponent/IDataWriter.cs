using TUGraz.VectoCore.Models.SimulationComponent.Data;
namespace TUGraz.VectoCore.Models.SimulationComponent
{
    public interface IDataWriter
    {
        object this[ModalResultFields key] { get; set; }
    }
}
