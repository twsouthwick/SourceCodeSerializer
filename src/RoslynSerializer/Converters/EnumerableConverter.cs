using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RoslynSerializer.Converters
{
    public class EnumerableConverter : ExpressionConverter<IEnumerable>
    {
        public override bool CanConvert(Type type) => typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo());

        public override ExpressionSyntax ConvertSyntax(Type type, IEnumerable collection, SourceCodeSerializer serializer)
        {
            var arrayType = GetGenericParameter(type);
            var generic = collection.Cast<object>();

            var items = generic.Select(item => serializer.WriteValue(item).WithLeadingTrivia(TriviaList(LineFeed)));

            return ArrayCreationExpression(
                ArrayType(serializer.GetTypeName(arrayType), SingletonList(ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression())))))
                .WithInitializer(InitializerExpression(SyntaxKind.ArrayInitializerExpression, SeparatedList(items)));
        }

        private Type GetGenericParameter(Type type)
        {
            if (!type.GetTypeInfo().IsGenericType)
            {
                return typeof(object);
            }

            var genericArgs = type.GetTypeInfo().GenericTypeArguments;

            return genericArgs[0];
        }
    }
}
