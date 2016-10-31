using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Reflection;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RoslynSerializer.Converters
{
    public class UnknownValueTypeConverter : ExpressionConverter
    {
        private readonly HashSet<Type> _unknownTypes = new HashSet<Type>();

        public override bool CanConvert(Type type) => type.GetTypeInfo().IsValueType;

        public override ExpressionSyntax ConvertSyntax(Type type, object obj, SourceCodeSerializer serializer)
        {
            _unknownTypes.Add(type);

            return ParseExpression("new object()");
        }
    }
}
