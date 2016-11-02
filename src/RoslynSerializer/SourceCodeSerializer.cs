using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using RoslynSerializer.Converters;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RoslynSerializer
{
    public class SourceCodeSerializer
    {
        private static ExpressionConverter[] s_converters = new ExpressionConverter[]
        {
            new PrimitiveConverter(),
            new StringConverter(),
            new EnumConverter(),
            new DateTimeConverter(),
            new DateTimeOffsetConverter(),
            new DecimalConverter(),
            new UnknownValueTypeConverter(),
            new EnumerableConverter(),
            new ObjectConverter()
        };

        public SerializerSettings Settings { get; }

        public SourceCodeSerializer()
            : this(null)
        {
        }

        public SourceCodeSerializer(SerializerSettings settings)
        {
            Settings = settings ?? new SerializerSettings();
        }

        public static void Serialize<T>(TextWriter writer, T obj, SerializerSettings settings = null)
        {
            var serializer = new SourceCodeSerializer(settings);

            serializer.Serialize(writer, obj);
        }

        private void Serialize<T>(TextWriter writer, T obj)
        {
            var node = Format(WriteValue(obj), typeof(T));

            node.WriteTo(writer);
        }

        public TypeSyntax GetTypeName(Type type)
        {
            return type.GetSyntaxNode(Settings.Usings);
        }

        private SyntaxNode GetMember(ExpressionSyntax node, Type type)
        {
            var constructor = Settings.Generator as Constructor;
            var factory = Settings.Generator as FactoryMethod;

            if (constructor != null)
            {
                var obj = node as ObjectCreationExpressionSyntax;
                if (obj != null)
                {
                    return ConstructorDeclaration(constructor.ClassName)
                        .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                        .WithBody(Block(SeparatedList(obj.Initializer.Expressions.Select(ExpressionStatement))));
                }
                else
                {
                    Debug.Fail($"Unknown kind: {node.Kind()}");

                    return node;
                }
            }
            else
            {
                return MethodDeclaration(GetTypeName(type), Identifier(factory.MethodName))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithBody(Block(SingletonList<StatementSyntax>(ReturnStatement(node))));
            }
        }

        private SyntaxNode AddCreateMethod(ExpressionSyntax node, Type type)
        {
            if (Settings.Generator == null)
            {
                return node;
            }

            var usings = Settings.Usings.Select(@using => UsingDirective(IdentifierName(@using)));
            var member = GetMember(node, type);

            return CompilationUnit()
                .WithUsings(List(usings))
                .WithMembers(
                    SingletonList<MemberDeclarationSyntax>(NamespaceDeclaration(IdentifierName(Settings.Generator.Namespace))
                    .WithMembers(
                        SingletonList<MemberDeclarationSyntax>(
                            ClassDeclaration(Settings.Generator.ClassName)
                            .WithModifiers(TokenList(Token(SyntaxKind.PartialKeyword)))
                            .WithMembers(SingletonList(member))
                ))));
        }


        private SyntaxNode Format(ExpressionSyntax node, Type type)
        {
            var cu = AddCreateMethod(node, type);

            using (var ws = new AdhocWorkspace())
            {
                return Formatter.Format(cu, ws);
            }
        }

        public ExpressionSyntax WriteValue(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            var type = obj.GetType();

            if (type.GetDefault()?.Equals(obj) == true)
            {
                return null;
            }

            foreach (var converter in s_converters)
            {
                if (converter.CanConvert(type))
                {
                    return converter.ConvertSyntax(type, obj, this);
                }
            }

            throw new UnknownTypeException(type);
        }
    }
}
