using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
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

        protected virtual SeparatedSyntaxList<ExpressionStatementSyntax> GetBody(SourceCodeSerializer serializer, ExpressionSyntax node, Type type)
        {
            var obj = node as ObjectCreationExpressionSyntax;
            if (obj != null)
            {
                return SeparatedList(obj.Initializer.Expressions.Select(ExpressionStatement));
            }
            else
            {
                Debug.Fail($"Unknown kind: {node.Kind()}");

                return SeparatedList(new[] { ExpressionStatement(node) });
            }
        }

        protected override ImmutableArray<MemberDeclarationSyntax> GetMembers(SourceCodeSerializer serializer, ExpressionSyntax node, Type type)
        {
            var constructor = ConstructorDeclaration(ClassName)
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithBody(Block(GetBody(serializer, node, type)));

            return ImmutableArray.Create<MemberDeclarationSyntax>(constructor);
        }
    }
}
