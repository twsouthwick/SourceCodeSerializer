using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Reflection;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RoslynSerializer.Converters
{
    public sealed class PrimitiveConverter : ExpressionConverter
    {
        private static readonly Dictionary<Type, string> s_suffixMap = new Dictionary<Type, string>
        {
            { typeof(double), "d" },
            { typeof(float), "f" },
        };

        public override bool CanConvert(Type type) => type.GetTypeInfo().IsPrimitive;

        public override ExpressionSyntax ConvertSyntax(Type type, object obj, SourceCodeSerializer serializer)
        {
            return ParseExpression($"{obj}{GetSuffix(type)}");
        }

        private string GetSuffix(Type type)
        {
            string result;
            if(s_suffixMap.TryGetValue(type, out result))
            {
                return result;
            }

            return string.Empty;
        }
    }
}
