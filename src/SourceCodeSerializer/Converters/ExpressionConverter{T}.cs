using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace SourceCodeSerializer.Converters
{
    public abstract class ExpressionConverter<T> : ExpressionConverter
    {
        public override bool CanConvert(Type type) => type == typeof(T);

        public sealed override ExpressionSyntax ConvertToExpression(Type type, object obj, SourceCodeSerializer serializer) => ConvertToExpression(type, (T)obj, serializer);

        public abstract ExpressionSyntax ConvertToExpression(Type type, T obj, SourceCodeSerializer serializer);
    }
}
