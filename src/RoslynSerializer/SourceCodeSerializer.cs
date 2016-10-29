using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RoslynSerializer
{
    public class SourceCodeSerializer
    {
        public ExpressionSyntax Serialize<T>(T obj)
        {
            return WriteObject(obj);
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

            if (obj.GetType().GetTypeInfo().IsEnum)
            {
                return ParseExpression($"{obj.GetType().FullName}.{obj}");
            }

            ExpressionSyntax result;
            if (TryWrite(obj as IEnumerable, out result))
            {
                return result;
            }

            if (obj.GetType().GetTypeInfo().IsPrimitive)
            {
                return ParseExpression(obj.ToString());
            }

            if (obj.GetType() == typeof(string))
            {
                return ParseExpression($"\"{obj as string}\"");
            }

            if (obj.GetType() == typeof(DateTime))
            {
                var dt = (DateTime)obj;

                return ParseExpression($"new DateTime({dt.Ticks}))");
            }

            if (obj.GetType() == typeof(DateTimeOffset))
            {
                var dt = (DateTimeOffset)obj;

                return ParseExpression($"new DateTimeOffset({dt.Ticks}, new TimeSpan({dt.Offset.Ticks}))");
            }

            if (obj.GetType() == typeof(decimal))
            {
                return ParseExpression(obj.ToString());
            }

            if (obj.GetType().GetTypeInfo().IsValueType)
            {
                if (unknownTypes.Add(obj.GetType()))
                {
                    Console.WriteLine("Unknown type: {0}", obj.GetType());
                }

                return ParseExpression("new object()");
            }

            return WriteObject(obj);
        }

        private HashSet<Type> unknownTypes = new HashSet<Type>();

        public ExpressionSyntax WriteObject(object obj)
        {
            var properties = GetProperties(obj.GetType());

            var propertyNodes = properties.Select(property =>
            {
                try
                {
                    var value = property.GetValue(obj);
                    var expression = WriteValue(value);

                    return AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName(property.Name), expression);
                    //.WithLeadingTrivia(TriviaList(LineFeed));
                }
                catch (Exception)
                {
                    return null;
                }
            }).Where(prop => prop != null);

            var typeName = IdentifierName(obj.GetType().ToString());

            return ObjectCreationExpression(typeName)
                     .WithInitializer(
                       InitializerExpression(SyntaxKind.ObjectInitializerExpression, SeparatedList<ExpressionSyntax>(propertyNodes)));
        }

        private bool TryWrite(IEnumerable collection, out ExpressionSyntax node)
        {
            if (collection == null)
            {
                node = null;
                return false;
            }

            var generic = LinqHelper(collection);

            // TODO: Get type instead of assuming same type as first element
            var type = generic.First().GetType();
            var items = generic.Select(item => WriteValue(item).WithLeadingTrivia(TriviaList(LineFeed)));

            node = ArrayCreationExpression(
                ArrayType(ParseTypeName(type.FullName), SingletonList<ArrayRankSpecifierSyntax>(
                                                        ArrayRankSpecifier(
                                                            SingletonSeparatedList<ExpressionSyntax>(
                                                                OmittedArraySizeExpression())))))
                .WithInitializer(
                    InitializerExpression(
                        SyntaxKind.ArrayInitializerExpression,
                        SeparatedList(items)));
            return true;
        }

        private IEnumerable<PropertyInfo> GetProperties(Type type)
        {
            return new HashSet<PropertyInfo>(type.GetTypeInfo().DeclaredProperties).Where(prop =>
            {
                if (!prop.CanRead || !prop.CanWrite)
                {
                    return false;
                }

                if (prop.SetMethod.IsPublic && prop.GetMethod.IsPublic)
                {
                    return true;
                }

                // Skip index parameters for now
                if (prop.GetIndexParameters().Length == 0)
                {
                    return false;
                }

                return prop.CustomAttributes.Any(attr => string.Equals("IncludeAttribute", attr.AttributeType.Name, StringComparison.Ordinal));
            }).OrderBy(prop => prop.Name).ToList();
        }

        private IEnumerable<object> LinqHelper(IEnumerable e)
        {
            foreach (var item in e)
            {
                yield return e;
            }
        }
    }
}
