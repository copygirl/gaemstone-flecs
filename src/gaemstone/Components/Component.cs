using System;

namespace gaemstone
{
	public readonly struct Component
	{
		public readonly Type Type;
		public Component(Type value) => Type = value;
		public static Component Of<T>() => new(typeof(T));
	}
}
