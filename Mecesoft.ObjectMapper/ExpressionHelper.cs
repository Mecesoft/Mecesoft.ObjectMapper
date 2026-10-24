using System.Linq.Expressions;

namespace Mecesoft.ObjectMapper;

internal static class ExpressionHelper
{
    public static string GetMemberName(LambdaExpression expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        if (StripConversions(expression.Body) is not MemberExpression member)
        {
            throw new ArgumentException("Expression must target a member.", nameof(expression));
        }

        if (!ReferenceEquals(StripConversions(member.Expression), expression.Parameters[0]))
        {
            throw new MappingConfigurationException(
                $"Nested destination member paths are not supported: '{GetMemberPath(member)}'. " +
                "Configure a map for the nested destination type instead.");
        }

        return member.Member.Name;
    }

    private static Expression? StripConversions(Expression? expression)
    {
        while (expression is UnaryExpression
               {
                   NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked
               } unary)
        {
            expression = unary.Operand;
        }

        return expression;
    }

    private static string GetMemberPath(MemberExpression member)
    {
        var memberNames = new List<string>();
        Expression? current = member;

        while (StripConversions(current) is MemberExpression currentMember)
        {
            memberNames.Add(currentMember.Member.Name);
            current = currentMember.Expression;
        }

        memberNames.Reverse();
        return string.Join('.', memberNames);
    }
}
