using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Jb.Test.ODataODataInfrastructura.Query
{
	/// <summary>
	/// Класс транслятора запросов.
	/// </summary>
	internal abstract class QueryTranslatorProvider : ExpressionVisitor
	{
		protected QueryTranslatorProvider(IQueryable source)
		{
			Source = source ?? throw new ArgumentNullException("source");
		}


		internal IQueryable Source { get; }
	}


	internal class QueryTranslatorProvider<T> : QueryTranslatorProvider, IQueryProvider
	{
		private readonly IEnumerable<ExpressionVisitor> _visitors;


		public QueryTranslatorProvider(IQueryable source, IEnumerable<ExpressionVisitor> visitors)
		    : base(source)
		{
			_visitors = visitors ?? throw new ArgumentNullException("visitors");
		}


		public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
		{
			if(expression == null)
			{
				throw new ArgumentNullException("expression");
			}

			return new QueryTranslator<TElement>(Source, expression, _visitors);
		}

		public IQueryable CreateQuery(Expression expression)
		{
			if(expression == null)
			{
				throw new ArgumentNullException("expression");
			}

			var elementType = expression.Type.GetGenericArguments().First();
			var result = (IQueryable) Activator.CreateInstance(typeof(QueryTranslator<>).MakeGenericType(elementType), Source, expression, _visitors);
			return result;
		}


		public TResult Execute<TResult>(Expression expression)
		{
			if(expression == null)
			{
				throw new ArgumentNullException("expression");
			}
			var result = (this as IQueryProvider).Execute(expression);

			return (TResult) result;
		}


		public object Execute(Expression expression)
		{
			if(expression == null)
			{
				throw new ArgumentNullException("expression");
			}

			var translated = VisitAll(expression);
			return Source.Provider.Execute(translated);
		}


		internal IEnumerable ExecuteEnumerable(Expression expression)
		{
			if(expression == null)
			{
				throw new ArgumentNullException("expression");
			}

			var translated = VisitAll(expression);
			return Source.Provider.CreateQuery(translated);
		}


		private Expression VisitAll(Expression expression)
		{
			var visitors = new ExpressionVisitor[] { this }.Concat(_visitors);

			return visitors.Aggregate(expression, (expr, visitor) => visitor.Visit(expr));
		}

		protected override Expression VisitConstant(ConstantExpression node)
		{
			if(node.Type.IsGenericType &&
			    node.Type.GetGenericTypeDefinition() == typeof(QueryTranslator<>))
			{

				var provider = ((IQueryable) node.Value).Provider as QueryTranslatorProvider;

				return provider != null
					? provider.Source.Expression
					: Source.Expression;
			}

			return base.VisitConstant(node);
		}
	}
}