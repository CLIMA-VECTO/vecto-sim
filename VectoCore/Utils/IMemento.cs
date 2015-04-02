namespace TUGraz.VectoCore.Utils
{
	public interface IMemento
	{
		string Serialize();
		void Deserialize(string data);
	}
}