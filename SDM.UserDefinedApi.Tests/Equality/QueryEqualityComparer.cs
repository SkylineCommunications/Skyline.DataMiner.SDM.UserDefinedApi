namespace Skyline.DataMiner.SDM.UserDefinedApi.Tests.Equality
{
	using System;
	using System.Collections.Generic;

	using SLDataGateway.API.Types.Querying;

	internal class QueryEqualityComparer<T> : IEqualityComparer<IQuery<T>>
	{
		public bool Equals(IQuery<T> x, IQuery<T> y)
		{
			if (ReferenceEquals(x, y))
			{
				return true;
			}

			if (x is null || y is null)
			{
				return false;
			}

			if (!x.Filter.Equals(y.Filter))
			{
				return false;
			}

			if (!x.Order.Equals(y.Order))
			{
				return false;
			}

			if (!x.Limit.Equals(y.Limit))
			{
				return false;
			}

			return true;
		}

		public int GetHashCode(IQuery<T> obj)
		{
			if (obj is null)
			{
				return 0;
			}

			int hash = 17;
			hash = hash * 23 + (obj.Filter?.GetHashCode() ?? 0);
			hash = hash * 23 + (obj.Order?.GetHashCode() ?? 0);
			hash = hash * 23 + (obj.Limit?.GetHashCode() ?? 0);
			return hash;
		}
	}
}
