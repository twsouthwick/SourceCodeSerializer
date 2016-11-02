using RoslynSerializer.Converters;
using RoslynSerializer.Generators;
using System;
using System.Collections.Immutable;

namespace RoslynSerializer
{
    public class SerializerSettings
    {
        private static readonly ImmutableArray<ExpressionConverter> s_converters = new ExpressionConverter[]
        {
            new PrimitiveConverter(),
            new StringConverter(),
            new EnumConverter(),
            new DateTimeConverter(),
            new DateTimeOffsetConverter(),
            new DecimalConverter(),
            new EnumerableConverter(),
            new ObjectConverter()
        }.ToImmutableArray();

        public ImmutableArray<ExpressionConverter> Converters { get; set; } = s_converters;

        public bool ObjectInitializationNewLine { get; set; } = true;

        public SourceGenerator Generator { get; set; }

        public ImmutableArray<string> Usings { get; set; } = ImmutableArray<string>.Empty;
    }
}
