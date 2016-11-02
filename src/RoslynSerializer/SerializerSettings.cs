using System;

namespace RoslynSerializer
{
    public class SerializerSettings
    {
        public bool ObjectInitializationNewLine { get; set; } = true;

        public SourceGeneration Generator { get; set; }

        public string[] Usings { get; set; } = Array.Empty<string>();
    }

    public abstract class SourceGeneration
    {
        public SourceGeneration(string ns, string className)
        {
            Namespace = ns;
            ClassName = className;
        }

        public string Namespace { get; }
        public string ClassName { get; }
    }

    public class FactoryMethod : SourceGeneration
    {
        public FactoryMethod(string ns, string className, string method)
            : base(ns, className)
        {
            MethodName = method;
        }

        public string MethodName { get; }
    }

    public class Constructor : SourceGeneration
    {
        public Constructor(string ns, string className)
            : base(ns, className)
        {
        }
    }
}
