using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace SourceCodeSerializer.Converters
{
    public abstract class ExpressionConverter
    {
        public abstract bool CanConvert(Type type);

        public abstract ExpressionSyntax ConvertToExpression(Type type, object obj, SourceCodeSerializer serializer);
    }
}
