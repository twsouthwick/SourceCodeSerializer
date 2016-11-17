using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SourceCodeSerializer.Converters
{
    public sealed class DateTimeOffsetConverter : ExpressionConverter<DateTimeOffset>
    {
        public override ExpressionSyntax ConvertToExpression(Type type, DateTimeOffset obj, SourceCodeSerializer serializer)
        {
            return ParseExpression($"new DateTimeOffset({obj.Ticks}, new TimeSpan({obj.Offset.Ticks}))");
        }
    }
}
