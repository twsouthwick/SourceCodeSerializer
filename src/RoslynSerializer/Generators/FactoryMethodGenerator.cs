using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.CSharp;

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

        protected override SyntaxNode GetMember(SourceCodeSerializer serializer, ExpressionSyntax node, Type type)
        {
            return MethodDeclaration(serializer.GetTypeName(type), Identifier(MethodName))
                  .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                  .WithBody(Block(SingletonList<StatementSyntax>(ReturnStatement(node))));
        }
    }
}
