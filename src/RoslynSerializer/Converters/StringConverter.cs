using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RoslynSerializer.Converters
{
    public sealed class StringConverter : ExpressionConverter<string>
    {
        public override ExpressionSyntax ConvertToExpression(Type type, string obj, SourceCodeSerializer serializer)
        {
            return ParseExpression($"@\"{obj}\"");
        }
    }
}
