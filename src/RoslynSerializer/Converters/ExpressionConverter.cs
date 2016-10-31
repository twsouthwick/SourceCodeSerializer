using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.Collections;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.IO;

namespace RoslynSerializer.Converters
{
    public abstract class ExpressionConverter
    {
        public abstract bool CanConvert(Type type);

        public abstract ExpressionSyntax ConvertSyntax(Type type, object obj, SourceCodeSerializer serializer);
    }

    public abstract class ExpressionConverter<T> : ExpressionConverter
    {
        public override bool CanConvert(Type type) => type == typeof(T);

        public sealed override ExpressionSyntax ConvertSyntax(Type type, object obj, SourceCodeSerializer serializer) => ConvertSyntax(type, (T)obj, serializer);

        public abstract ExpressionSyntax ConvertSyntax(Type type, T obj, SourceCodeSerializer serializer);
    }

    public sealed class PrimitiveConverter : ExpressionConverter
    {
        public override bool CanConvert(Type type) => type.GetTypeInfo().IsPrimitive;

        public override ExpressionSyntax ConvertSyntax(Type type, object obj, SourceCodeSerializer serializer)
        {
            return ParseExpression(obj.ToString());
        }
    }

    public sealed class StringConverter : ExpressionConverter<string>
    {
        public override ExpressionSyntax ConvertSyntax(Type type, string obj, SourceCodeSerializer serializer)
        {
            return ParseExpression($"\"{obj}\"");
        }
    }

    public sealed class EnumConverter : ExpressionConverter
    {
        public override bool CanConvert(Type type) => type.GetTypeInfo().IsEnum;

        public override ExpressionSyntax ConvertSyntax(Type type, object obj, SourceCodeSerializer serializer)
        {
            return ParseExpression($"{obj.GetType().FullName}.{obj}");
        }
    }

    public sealed class DateTimeConverter : ExpressionConverter<DateTime>
    {
        public override ExpressionSyntax ConvertSyntax(Type type, DateTime obj, SourceCodeSerializer serializer)
        {
            return ParseExpression($"new DateTime({obj.Ticks}))");
        }
    }

    public sealed class DateTimeOffsetConverter : ExpressionConverter<DateTimeOffset>
    {
        public override ExpressionSyntax ConvertSyntax(Type type, DateTimeOffset obj, SourceCodeSerializer serializer)
        {
            return ParseExpression($"new DateTimeOffset({obj.Ticks}, new TimeSpan({obj.Offset.Ticks}))");
        }
    }

    public sealed class DecimalConverter : ExpressionConverter<decimal>
    {
        public override ExpressionSyntax ConvertSyntax(Type type, decimal obj, SourceCodeSerializer serializer)
        {
            return ParseExpression(obj.ToString());
        }
    }

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

    public class EnumerableConverter : ExpressionConverter<IEnumerable>
    {
        public override bool CanConvert(Type type) => typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo());

        public override ExpressionSyntax ConvertSyntax(Type type, IEnumerable collection, SourceCodeSerializer serializer)
        {
            var generic = collection.Cast<object>();

            var items = generic.Select(item => serializer.WriteValue(item).WithLeadingTrivia(TriviaList(LineFeed)));

            return ArrayCreationExpression(
                ArrayType(ParseTypeName(serializer.GetTypeName(type)), SingletonList(ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression())))))
                .WithInitializer(InitializerExpression(SyntaxKind.ArrayInitializerExpression, SeparatedList(items)));
        }
    }

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

                    return AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName(property.Name), expression)
                        .WithLeadingTrivia(TriviaList(LineFeed));
                }
                catch (Exception)
                {
                    return null;
                }
            }).Where(prop => prop != null);

            var typeName = IdentifierName(serializer.GetTypeName(obj.GetType()));

            return ObjectCreationExpression(typeName)
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

    public sealed class UnknownTypeException : Exception
    {
        public UnknownTypeException(Type type)
            : base($"Cannot serializer unknown type {type.FullName}")
        {
            UnknownType = type;
        }

        public Type UnknownType { get; }
    }
}
