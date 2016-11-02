using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics;
using System.Linq;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RoslynSerializer.Generators
{
    public class ConstructorGenerator : ClassMemberGenerator
    {
        public ConstructorGenerator(string ns, string className)
            : base(ns, className)
        { }

        protected override SyntaxNode GetMember(SourceCodeSerializer serializer, ExpressionSyntax node, Type type)
        {
            var obj = node as ObjectCreationExpressionSyntax;
            if (obj != null)
            {
                return ConstructorDeclaration(ClassName)
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithBody(Block(SeparatedList(obj.Initializer.Expressions.Select(ExpressionStatement))));
            }
            else
            {
                Debug.Fail($"Unknown kind: {node.Kind()}");

                return node;
            }
        }
    }
}
