namespace TUGraz.VectoCore.Models.Connector.Ports
{
    /// <summary>
    /// Defines a method to acquire an Tn in port.
    /// </summary>
    public interface IInShaft
    {
        /// <summary>
        /// Returns the inport to connect it to another outport.
        /// </summary>
        /// <returns></returns>
        ITnInPort InShaft();
    }

    /// <summary>
    /// Defines a method to acquire an Tn out port.
    /// </summary>
    public interface IOutShaft
    {
        /// <summary>
        /// Returns the outport to send requests to.
        /// </summary>
        /// <returns></returns>
        ITnOutPort OutShaft();
    }
}