using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RoslynSerializer.Converters
{
    public sealed class EnumConverter : ExpressionConverter
    {
        public override bool CanConvert(Type type) => type.GetTypeInfo().IsEnum;

        public override ExpressionSyntax ConvertToExpression(Type type, object obj, SourceCodeSerializer serializer)
        {
            var entries = GetFlags(type, (Enum)obj)
                .Select(e => $"{serializer.GetTypeName(type)}.{e}");

            return ParseExpression(string.Join(" | ", entries));
        }

        private IEnumerable<Enum> GetFlags(Type type, Enum obj)
        {
            var zero = Enum.ToObject(type, 0);

            if (!obj.Equals(zero) && type.GetTypeInfo().IsDefined(typeof(FlagsAttribute)))
            {
                foreach (Enum entry in Enum.GetValues(type))
                {
                    if (!entry.Equals(zero) && obj.HasFlag(entry))
                    {
                        yield return entry;
                    }
                }
            }
            else
            {
                yield return obj;
            }
        }
    }
}
