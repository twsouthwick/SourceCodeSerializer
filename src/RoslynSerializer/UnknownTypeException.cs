using System;

namespace RoslynSerializer
{
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
