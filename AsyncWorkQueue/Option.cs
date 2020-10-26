using System;

namespace AsyncWorkQueue
{
	public struct Option<T>
	{
		private readonly T value;

		public bool HasValue { get; private set; }

		public T Value
		{
			get
			{
				if(!this.HasValue)
				{
					throw new InvalidOperationException("Option has no value.");
				}

				return this.value;
			}
		}

		private Option(T value)
		{
			this.value = value;
			this.HasValue = true;
		}

		public static Option<T> None => new Option<T> {HasValue = false};

		public static Option<T> Some(T value) => new Option<T>(value);
	}
}