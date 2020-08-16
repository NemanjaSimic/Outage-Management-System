using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace OMS.HistoryDBManagerImplementation.Reporting.Queries.Specifications
{
    public class AndSpecification<T> : Specification<T> where T : class
	{
		private IList<Specification<T>> _specifications;

		public AndSpecification(IList<Specification<T>> specifications)
			=> _specifications = specifications.Count >= 2 ? specifications : throw new ArgumentException($"{nameof(specifications)} length cannot be less than 2");


		/// <summary>
		/// Builds an expression tree for checking multiple ISpecifications
		/// </summary>
		/// <returns>Expression tree</returns>
		public override Expression<Func<T, bool>> IsSatisfiedBy
		{
			get
			{
				var param = Expression.Parameter(typeof(T));
				var body = Expression.And(Expression.Invoke(_specifications[0].IsSatisfiedBy, param),
										   Expression.Invoke(_specifications[1].IsSatisfiedBy, param));

				if (_specifications.Count > 2)
				{
					for (int i = 2; i < _specifications.Count; i++)
					{
						body = Expression.And(body, Expression.Invoke(_specifications[i].IsSatisfiedBy, param));
					}
				}

				return Expression.Lambda<Func<T, bool>>(body, param);
			}
		}
	}
}
