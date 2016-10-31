using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Reflection;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RoslynSerializer.Converters
{
    public sealed class EnumConverter : ExpressionConverter
    {
        public override bool CanConvert(Type type) => type.GetTypeInfo().IsEnum;

        public override ExpressionSyntax ConvertSyntax(Type type, object obj, SourceCodeSerializer serializer)
        {
            return ParseExpression($"{serializer.GetTypeName(obj.GetType())}.{obj}");
        }
    }
}
