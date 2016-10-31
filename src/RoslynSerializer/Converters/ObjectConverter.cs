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
                try
                {
                    var value = property.GetValue(obj);
                    var expression = serializer.WriteValue(value);
                    var assignment = AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName(property.Name), expression);

                    if (serializer.Settings.ObjectInitializationNewLine)
                    {
                        return assignment.WithLeadingTrivia(TriviaList(LineFeed));
                    }
                    else
                    {
                        return assignment;
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }).Where(prop => prop != null);

            return ObjectCreationExpression(serializer.GetTypeName(obj.GetType()))
                     .WithInitializer(
                       InitializerExpression(SyntaxKind.ObjectInitializerExpression, SeparatedList<ExpressionSyntax>(propertyNodes)));
        }

        protected virtual bool IncludeProperty(PropertyInfo prop)
        {
            return prop.CustomAttributes.Any(attr => string.Equals("IncludeAttribute", attr.AttributeType.Name, StringComparison.Ordinal));
        }

        private IEnumerable<PropertyInfo> GetProperties(Type type)
        {
            return new HashSet<PropertyInfo>(type.GetTypeInfo().DeclaredProperties).Where(prop =>
            {
                if (!prop.CanRead || !prop.CanWrite)
                {
                    return false;
                }

                if (prop.SetMethod.IsPublic && prop.GetMethod.IsPublic)
                {
                    return true;
                }

                // Skip index parameters for now
                if (prop.GetIndexParameters().Length == 0)
                {
                    return false;
                }

                return IncludeProperty(prop);
            }).OrderBy(prop => prop.Name);
        }
    }
}
