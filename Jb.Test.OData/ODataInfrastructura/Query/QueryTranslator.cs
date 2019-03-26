using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Jb.Test.ODataODataInfrastructura.Query
{
	internal class QueryTranslator<T> : IOrderedQueryable<T>
	{
		private readonly QueryTranslatorProvider<T> _provider;


		public QueryTranslator(IQueryable source, IEnumerable<ExpressionVisitor> visitors)
		{
			if(source == null)
			{
				throw new ArgumentNullException("source");
			}

			if(visitors == null)
			{
				throw new ArgumentNullException("visitors");
			}

			Expression = Expression.Constant(this);
			_provider = new QueryTranslatorProvider<T>(source, visitors);
		}

		public QueryTranslator(IQueryable source, Expression expression, IEnumerable<ExpressionVisitor> visitors)
		{
			Expression = expression ?? throw new ArgumentNullException("expression");
			_provider = new QueryTranslatorProvider<T>(source, visitors);
		}


		public IEnumerator<T> GetEnumerator() =>
			((IEnumerable<T>) _provider
			.ExecuteEnumerable(Expression))
			.GetEnumerator();


		IEnumerator IEnumerable.GetEnumerator() =>
			_provider
			.ExecuteEnumerable(Expression)
			.GetEnumerator();


		public Type ElementType => typeof(T);
		public Expression Expression { get; }
		public IQueryProvider Provider => _provider;
	}
}