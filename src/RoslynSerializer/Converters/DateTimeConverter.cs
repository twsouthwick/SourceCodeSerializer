using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RoslynSerializer.Converters
{
    public sealed class DateTimeConverter : ExpressionConverter<DateTime>
    {
        public override ExpressionSyntax ConvertSyntax(Type type, DateTime obj, SourceCodeSerializer serializer)
        {
            return ParseExpression($"new DateTime({obj.Ticks}))");
        }
    }
}
