using NLog;

namespace TUGraz.VectoCore.Models
{
	public class LoggingObject
	{
		protected Logger Log { get; private set; }

		protected LoggingObject()
		{
			Log = LogManager.GetLogger(GetType().FullName);
		}

		protected static Logger Logger<T>()
		{
			return LogManager.GetLogger(typeof(T).ToString());
		}
	}
}