using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Reflection;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RoslynSerializer
{
    internal static class TypeExtensions
    {
        private static readonly Dictionary<Type, SyntaxKind> s_syntaxKinds = new Dictionary<Type, SyntaxKind>
        {
            { typeof(bool), SyntaxKind.BoolKeyword },
            { typeof(byte), SyntaxKind.ByteKeyword },
            { typeof(sbyte), SyntaxKind.SByteKeyword },
            { typeof(short), SyntaxKind.ShortKeyword },
            { typeof(ushort), SyntaxKind.UShortKeyword },
            { typeof(int), SyntaxKind.IntKeyword },
            { typeof(uint), SyntaxKind.UIntKeyword },
            { typeof(long), SyntaxKind.LongKeyword },
            { typeof(ulong), SyntaxKind.ULongKeyword },
            { typeof(double), SyntaxKind.DoubleKeyword },
            { typeof(float), SyntaxKind.FloatKeyword },
            { typeof(decimal), SyntaxKind.DecimalKeyword },
            { typeof(string), SyntaxKind.StringKeyword },
            { typeof(char), SyntaxKind.CharKeyword },
            { typeof(void), SyntaxKind.VoidKeyword },
            { typeof(object), SyntaxKind.ObjectKeyword }
        };

        public static TypeSyntax GetSyntaxNode(this Type type, IEnumerable<string> usings)
        {
            SyntaxKind kind;
            if (s_syntaxKinds.TryGetValue(type, out kind))
            {
                return PredefinedType(Token(kind));
            }

            var fullName = type.FullName;

            foreach (var u in usings)
            {
                var up = $"{u}.";
                if (fullName.StartsWith(up, StringComparison.Ordinal))
                {
                    return ParseTypeName(fullName.Substring(up.Length));
                }
            }

            return ParseTypeName(fullName);
        }

        public static object GetDefault(this Type targetType)
        {
            if (targetType == null || !targetType.GetTypeInfo().IsValueType)
            {
                return null;
            }

            return typeof(TypeExtensions).GetTypeInfo().GetDeclaredMethod(nameof(ToDefaultHelper))
                .MakeGenericMethod(targetType)
                .Invoke(null, null);
        }

        private static T ToDefaultHelper<T>()
        {
            return default(T);
        }
    }
}
