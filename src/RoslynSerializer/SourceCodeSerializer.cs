using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using RoslynSerializer.Converters;
using System.IO;

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

        public ExpressionSyntax Serialize<T>(T obj)
        {
            return WriteValue(obj);
        }

        public void Serialize<T>(TextWriter writer, T obj)
        {
            var node = Serialize(obj);

            Serialize(writer, node);
        }

        public void Serialize(TextWriter writer, ExpressionSyntax node)
        {
            var cu = CompilationUnit()
                  .WithMembers(
                      SingletonList<MemberDeclarationSyntax>(
                          ClassDeclaration("C")
                          .WithMembers(
                              SingletonList<MemberDeclarationSyntax>(
                                  MethodDeclaration(
                                      PredefinedType(
                                          Token(SyntaxKind.VoidKeyword)),
                                      Identifier("Test"))
                                  .WithModifiers(
                                      TokenList(
                                          Token(SyntaxKind.PublicKeyword)))
                                  .WithBody(
                                      Block(SingletonList<StatementSyntax>(
                                          ExpressionStatement(
                                              AssignmentExpression(
                                                  SyntaxKind.SimpleAssignmentExpression,
                                                  IdentifierName("value"),
                                                  node)
                                              )
                                          )
                                      )
                                  )
                              )
                          )
                      )
                  );

            using (var ws = new AdhocWorkspace())
            {
                ws.Options = ws.Options
                    .WithChangedOption(CSharpFormattingOptions.NewLineForMembersInObjectInit, true)
                    .WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInObjectCollectionArrayInitializers, true);

                var result = Formatter.Format(cu, ws, ws.Options);

                result.WriteTo(writer);
            }
        }

        public ExpressionSyntax WriteValue(object obj)
        {
            if (obj == null)
            {
                return ParseExpression("null");
            }

            var type = obj.GetType();

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
