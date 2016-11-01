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
    public class ObjectConverter : ExpressionConverter
    {
        public override bool CanConvert(Type type) => !type.GetTypeInfo().IsPrimitive;

        public override ExpressionSyntax ConvertSyntax(Type type, object obj, SourceCodeSerializer serializer)
        {
            var properties = GetProperties(type);

            var propertyNodes = properties.Select(property =>
            {
                var value = property.GetValue(obj);
                var expression = serializer.WriteValue(value);

                if (expression == null)
                {
                    return null;
                }

                var assignment = AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName(property.Name), expression);

                if (serializer.Settings.ObjectInitializationNewLine)
                {
                    return assignment.WithLeadingTrivia(TriviaList(LineFeed));
                }
                else
                {
                    return assignment;
                }
            }).Where(prop => prop != null);

            return ObjectCreationExpression(serializer.GetTypeName(obj.GetType()))
                     .WithInitializer(
                       InitializerExpression(SyntaxKind.ObjectInitializerExpression, SeparatedList<ExpressionSyntax>(propertyNodes)));
        }

        protected virtual bool IncludeProperty(PropertyInfo prop)
        {
            // Skip index parameters for now
            if (prop.GetIndexParameters().Length != 0)
            {
                return false;
            }

            if (prop.CustomAttributes.Any(attr => string.Equals("IncludeAttribute", attr.AttributeType.Name, StringComparison.Ordinal)))
            {
                return true;
            }

            if (prop.CustomAttributes.Any(attr => string.Equals("IgnoreAttribute", attr.AttributeType.Name, StringComparison.Ordinal)))
            {
                return false;
            }

            if (!prop.CanRead || !prop.CanWrite)
            {
                return false;
            }

            if (prop.SetMethod.IsPublic && prop.GetMethod.IsPublic)
            {
                return true;
            }

            return false;
        }

        private IEnumerable<PropertyInfo> GetProperties(Type type)
        {
            var result = new SortedSet<PropertyInfo>(PropertyInfoComparer.Instance);

            FillProperties(type, result);

            return result;
        }

        private void FillProperties(Type type, SortedSet<PropertyInfo> set)
        {
            if (type == typeof(object))
            {
                return;
            }

            foreach (var property in type.GetTypeInfo().DeclaredProperties.Where(IncludeProperty))
            {
                set.Add(property);
            }

            FillProperties(type.GetTypeInfo().BaseType, set);
        }

        private class PropertyInfoComparer : IComparer<PropertyInfo>
        {
            public static IComparer<PropertyInfo> Instance { get; } = new PropertyInfoComparer();

            private PropertyInfoComparer()
            {
            }

            public int Compare(PropertyInfo x, PropertyInfo y)
            {
                return StringComparer.Ordinal.Compare(x?.Name, y?.Name);
            }
        }
    }
}
