using RoslynSerializer.Generators;
using System;

namespace RoslynSerializer
{
    public class SerializerSettings
    {
        public bool ObjectInitializationNewLine { get; set; } = true;

        public SourceGenerator Generator { get; set; }

        public string[] Usings { get; set; } = Array.Empty<string>();
    }
}
