using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RoslynSerializer.Generators
{
    public abstract class ClassMemberGenerator : SourceGenerator
    {
        public ClassMemberGenerator(string ns, string className)
        {
            Namespace = ns;
            ClassName = className;
        }

        public string Namespace { get; }

        public string ClassName { get; }

        protected abstract SyntaxNode GetMember(SourceCodeSerializer serializer, ExpressionSyntax node, Type type);

        public sealed override SyntaxNode Generate(SourceCodeSerializer serializer, ExpressionSyntax node, Type type)
        {
            var usings = serializer.Settings.Usings.Select(@using => UsingDirective(IdentifierName(@using)));
            var member = GetMember(serializer, node, type);

            return CompilationUnit()
                .WithUsings(List(usings))
                .WithMembers(
                    SingletonList<MemberDeclarationSyntax>(NamespaceDeclaration(IdentifierName(Namespace))
                    .WithMembers(
                        SingletonList<MemberDeclarationSyntax>(
                            ClassDeclaration(ClassName)
                            .WithModifiers(TokenList(Token(SyntaxKind.PartialKeyword)))
                            .WithMembers(SingletonList(member))
                ))));
        }
    }
}
