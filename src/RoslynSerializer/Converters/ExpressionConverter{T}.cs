using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace RoslynSerializer.Converters
{
    public abstract class ExpressionConverter<T> : ExpressionConverter
    {
        public override bool CanConvert(Type type) => type == typeof(T);

        public sealed override ExpressionSyntax ConvertSyntax(Type type, object obj, SourceCodeSerializer serializer) => ConvertSyntax(type, (T)obj, serializer);

        public abstract ExpressionSyntax ConvertSyntax(Type type, T obj, SourceCodeSerializer serializer);
    }
}
