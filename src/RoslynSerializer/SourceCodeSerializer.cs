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

        private readonly ImmutableList<string> _usings;
        private readonly TextWriter _textWriter;
        private readonly CreateMethodInfo _createMethodInfo;

        public SerializerSettings Settings { get; }

        private SourceCodeSerializer(TextWriter writer, CreateMethodInfo createMethodInfo, ImmutableList<string> usings, SerializerSettings settings)
        {
            _textWriter = writer;
            _createMethodInfo = createMethodInfo;
            _usings = usings;
            Settings = settings;
        }

        public static SourceCodeSerializer Create()
        {
            return new SourceCodeSerializer(null, null, ImmutableList.Create<string>("System"), SerializerSettings.Create());
        }

        public SourceCodeSerializer AddTextWriter(TextWriter writer)
        {
            return new SourceCodeSerializer(writer, _createMethodInfo, _usings, Settings);
        }

        public SourceCodeSerializer AddConstructor(string ns, string className)
        {
            var createMethodInfo = new CreateMethodInfo
            {
                Namespace = ns,
                ClassName = className,
                MethodName = className,
                IsConstructor = true
            };

            return new SourceCodeSerializer(_textWriter, createMethodInfo, _usings, Settings);
        }

        public SourceCodeSerializer AddCreateMethod(string ns, string className, string methodName = "Create")
        {
            var createMethodInfo = new CreateMethodInfo
            {
                Namespace = ns,
                ClassName = className,
                MethodName = methodName,
                IsConstructor = false
            };

            return new SourceCodeSerializer(_textWriter, createMethodInfo, _usings, Settings);
        }

        public SourceCodeSerializer AddUsing(string @using)
        {
            return new SourceCodeSerializer(_textWriter, _createMethodInfo, _usings.Add(@using), Settings);
        }

        public SourceCodeSerializer WithSettings(SerializerSettings settings)
        {
            return new SourceCodeSerializer(_textWriter, _createMethodInfo, _usings, settings);
        }

        public SyntaxNode Serialize<T>(T obj)
        {
            var node = Format(WriteValue(obj), typeof(T));

            Write(node);

            return node;
        }

        public TypeSyntax GetTypeName(Type type)
        {
            return type.GetSyntaxNode(_usings);
        }

        private SyntaxNode GetMember(ExpressionSyntax node, Type type)
        {
            if (_createMethodInfo.IsConstructor)
            {
                var obj = node as ObjectCreationExpressionSyntax;
                if (obj != null)
                {
                    return ConstructorDeclaration(_createMethodInfo.MethodName)
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
                return MethodDeclaration(GetTypeName(type), Identifier(_createMethodInfo.MethodName))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithBody(Block(SingletonList<StatementSyntax>(ReturnStatement(node))));
            }
        }

        private SyntaxNode AddCreateMethod(ExpressionSyntax node, Type type)
        {
            if (_createMethodInfo == null)
            {
                return node;
            }

            var usings = _usings.Select(@using => UsingDirective(IdentifierName(@using)));
            var member = GetMember(node, type);

            return CompilationUnit()
                .WithUsings(List(usings))
                .WithMembers(
                    SingletonList<MemberDeclarationSyntax>(NamespaceDeclaration(IdentifierName(_createMethodInfo.Namespace))
                    .WithMembers(
                        SingletonList<MemberDeclarationSyntax>(
                            ClassDeclaration(_createMethodInfo.ClassName)
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

        private void Write(SyntaxNode node)
        {
            if (_textWriter == null)
            {
                return;
            }

            node.WriteTo(_textWriter);
        }

        public ExpressionSyntax WriteValue(object obj)
        {
            var type = obj?.GetType();

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

        private class CreateMethodInfo
        {
            public string Namespace { get; set; }
            public string ClassName { get; set; }
            public string MethodName { get; set; }
            public bool IsConstructor { get; set; }
        }
    }
}
