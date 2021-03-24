using System;

namespace gaemstone
{
	public interface IQuery
	{

	}

	public class Query<T> : IQuery
		where T : Delegate
	{

	}
}
