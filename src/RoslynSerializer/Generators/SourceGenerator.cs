﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace RoslynSerializer.Generators
{
    public abstract class SourceGenerator
    {
        public abstract SyntaxNode Generate(SourceCodeSerializer serializer, ExpressionSyntax node, Type type);
    }
}
