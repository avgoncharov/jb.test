using System.Linq.Expressions;
using System.Reflection;
using Jb.Test.DAL.Interfaces.Model;
using Jb.Test.ODataInfrastructura;

namespace Jb.Test.Web.ODataNugetODataInfrastuctura
{
	public class NormalizeVersionInterceptor : ExpressionVisitor
	{
		private static readonly MemberInfo _versionMember = typeof(Package).GetProperty("Version");
		private static readonly MemberInfo _normalizedVersionMember = typeof(Package).GetProperty("NormalizedVersion");

		protected override Expression VisitBinary(BinaryExpression node)
		{
			if(node.NodeType != ExpressionType.Equal)
			{
				return node;
			}

			var constSide = (node.Left as ConstantExpression) ?? (node.Right as ConstantExpression);

			if(constSide == null || constSide.Type != typeof(string))
			{
				return node;
			}

			var memberSide = (node.Right as MemberExpression) ?? (node.Left as MemberExpression);
			if(memberSide == null || memberSide.Member != _versionMember)
			{
				return node;
			}

			return SemanticVersion.TryParse((string) constSide.Value, out var semanticVersion)
				? Expression.MakeBinary(
					ExpressionType.Equal,
					left: Expression.Constant(semanticVersion.ToNormalizedString()),
					right: Expression.MakeMemberAccess(memberSide.Expression, _normalizedVersionMember))
				: node;
		}
	}
}