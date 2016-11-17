using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SourceCodeSerializer.Converters
{
    public sealed class DecimalConverter : ExpressionConverter<decimal>
    {
        public override ExpressionSyntax ConvertToExpression(Type type, decimal obj, SourceCodeSerializer serializer)
        {
            return ParseExpression($"{obj}m");
        }
    }
}
