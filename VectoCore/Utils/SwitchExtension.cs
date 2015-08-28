using System;
using System.Diagnostics;

namespace TUGraz.VectoCore.Utils
{
	/// <summary>
	/// Extension Methods for Creating a Switch-Case Construct on Types.
	/// </summary>
	/// <remarks>
	/// Adapted for VECTO. Created by Virtlink. Original source code on GitHub: <see href="https://gist.github.com/Virtlink/8722649"/>.
	/// </remarks>
	public static class SwitchExtension
	{
		[DebuggerHidden]
		public static Switch<T> Switch<T>(this T self)
		{
			return new Switch<T>(self);
		}
	}

	public class Switch<T>
	{
		private readonly T _value;
		private bool _handled;

		[DebuggerHidden]
		internal Switch(T value)
		{
			_value = value;
			_handled = false;
		}

		[DebuggerHidden]
		public Switch<T> Case<TFilter>(Action action) where TFilter : T
		{
			return Case<TFilter>(_ => action());
		}

		[DebuggerHidden]
		public Switch<T> Case<TFilter>(Action<TFilter> action) where TFilter : T
		{
			if (!_handled && _value is TFilter) {
				action((TFilter)_value);
				_handled = true;
			}
			return this;
		}

		[DebuggerHidden]
		public void Default(Action action)
		{
			Default(_ => action());
		}

		[DebuggerHidden]
		public void Default(Action<T> action)
		{
			if (!_handled) {
				action(_value);
			}
		}
	}
}