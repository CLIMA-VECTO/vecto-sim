using System;
using System.Runtime.CompilerServices;

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
		public static Switcher<T> Switch<T>(this T self)
		{
			return new Switcher<T>(self);
		}

		public class Switcher<T>
		{
			private readonly T _value;
			private bool _handled;

			internal Switcher(T value)
			{
				_value = value;
				_handled = false;
			}

			public Switcher<T> Case<TTarget>(Action action) where TTarget : T
			{
				return Case<TTarget>(_ => action());
			}

			public Switcher<T> Case<TTarget>(Action<TTarget> action) where TTarget : T
			{
				if (!_handled && _value is TTarget) {
					action((TTarget)_value);
					_handled = true;
				}
				return this;
			}

			public void Default(Action action)
			{
				Default(_ => action());
			}

			public void Default(Action<T> action)
			{
				if (!_handled) {
					action(_value);
				}
			}
		}
	}
}