using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace RoslynSerializer.Converters
{
    public abstract class ExpressionConverter
    {
        public abstract bool CanConvert(Type type);

        public abstract ExpressionSyntax ConvertSyntax(Type type, object obj, SourceCodeSerializer serializer);
    }
}
