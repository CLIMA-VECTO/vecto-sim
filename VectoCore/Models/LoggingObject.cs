using System;
using NLog;

namespace TUGraz.VectoCore.Models
{
	public class LoggingObject
	{
		[NonSerialized] protected static readonly Logger Log = LogManager.GetCurrentClassLogger();
	}
}