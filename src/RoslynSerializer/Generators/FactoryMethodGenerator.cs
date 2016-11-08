using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RoslynSerializer.Generators
{
    public class FactoryMethodGenerator : ClassMemberGenerator
    {
        public FactoryMethodGenerator(string ns, string className, string method)
            : base(ns, className)
        {
            MethodName = method;
        }

        public string MethodName { get; }

        protected override ImmutableArray<MemberDeclarationSyntax> GetMembers(SourceCodeSerializer serializer, ExpressionSyntax node, Type type)
        {
            var method = MethodDeclaration(serializer.GetTypeName(type), Identifier(MethodName))
                  .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                  .WithBody(Block(SingletonList<StatementSyntax>(ReturnStatement(node))));

            return ImmutableArray.Create<MemberDeclarationSyntax>(method);
        }
    }
}
