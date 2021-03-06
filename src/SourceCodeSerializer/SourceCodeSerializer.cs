﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using SourceCodeSerializer.Converters;
using System;
using System.IO;

namespace SourceCodeSerializer
{
    public class SourceCodeSerializer
    {
        public SerializerSettings Settings { get; }

        public SourceCodeSerializer()
            : this(null)
        {
        }

        public SourceCodeSerializer(SerializerSettings settings)
        {
            Settings = settings ?? new SerializerSettings();
        }

        public static string Serialize<T>(T obj, SerializerSettings settings = null)
        {
            var serializer = new SourceCodeSerializer(settings);

            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, obj);

                return writer.ToString();
            }
        }

        public static void Serialize<T>(TextWriter writer, T obj, SerializerSettings settings = null)
        {
            var serializer = new SourceCodeSerializer(settings);

            serializer.Serialize(writer, obj);
        }

        public TypeSyntax GetTypeName(Type type)
        {
            return type.GetSyntaxNode(Settings.Usings);
        }

        public ExpressionSyntax GetExpression(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            var type = obj.GetType();

            if (Settings.IgnoreDefaultValues && type.GetDefault()?.Equals(obj) == true)
            {
                return null;
            }

            foreach (var converter in Settings.Converters)
            {
                if (converter.CanConvert(type))
                {
                    return converter.ConvertToExpression(type, obj, this);
                }
            }

            throw new UnknownTypeException(type);
        }

        private void Serialize<T>(TextWriter writer, T obj)
        {
            var expression = GetExpression(obj);

            var cu = Settings.Generator?.Generate(this, expression, typeof(T)) ?? expression;

            using (var ws = new AdhocWorkspace())
            {
                var formatted = Formatter.Format(cu, ws);

                formatted.WriteTo(writer);
            }
        }
    }
}
