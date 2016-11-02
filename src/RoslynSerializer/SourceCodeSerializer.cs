using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using RoslynSerializer.Converters;
using System;
using System.IO;

namespace RoslynSerializer
{
    public class SourceCodeSerializer
    {
        private static ExpressionConverter[] s_converters = new ExpressionConverter[]
        {
            new PrimitiveConverter(),
            new StringConverter(),
            new EnumConverter(),
            new DateTimeConverter(),
            new DateTimeOffsetConverter(),
            new DecimalConverter(),
            new UnknownValueTypeConverter(),
            new EnumerableConverter(),
            new ObjectConverter()
        };

        public SerializerSettings Settings { get; }

        public SourceCodeSerializer()
            : this(null)
        {
        }

        public SourceCodeSerializer(SerializerSettings settings)
        {
            Settings = settings ?? new SerializerSettings();
        }

        public static void Serialize<T>(TextWriter writer, T obj, SerializerSettings settings = null)
        {
            var serializer = new SourceCodeSerializer(settings);

            serializer.Serialize(writer, obj);
        }

        private void Serialize<T>(TextWriter writer, T obj)
        {
            var node = Format(WriteValue(obj), typeof(T));

            node.WriteTo(writer);
        }

        public TypeSyntax GetTypeName(Type type)
        {
            return type.GetSyntaxNode(Settings.Usings);
        }


        private SyntaxNode Format(ExpressionSyntax node, Type type)
        {
            var cu = Settings.Generator?.Generate(this, node, type) ?? node;

            using (var ws = new AdhocWorkspace())
            {
                return Formatter.Format(cu, ws);
            }
        }

        public ExpressionSyntax WriteValue(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            var type = obj.GetType();

            if (type.GetDefault()?.Equals(obj) == true)
            {
                return null;
            }

            foreach (var converter in s_converters)
            {
                if (converter.CanConvert(type))
                {
                    return converter.ConvertSyntax(type, obj, this);
                }
            }

            throw new UnknownTypeException(type);
        }
    }
}
