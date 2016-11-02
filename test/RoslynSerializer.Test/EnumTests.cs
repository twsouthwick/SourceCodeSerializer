using RoslynSerializer.Converters;
using System;
using System.Collections.Immutable;
using Xunit;
using Xunit.Abstractions;

namespace RoslynSerializer
{
    public class EnumTests
    {
        private readonly ITestOutputHelper _helper;

        public EnumTests(ITestOutputHelper helper)
        {
            _helper = helper;
        }

        [Theory]
        [InlineData(typeof(EnumTest1), true)]
        [InlineData(typeof(EnumTest2), true)]
        public void CanConvertTests(Type type, bool expected)
        {
            var converter = new EnumConverter();

            Assert.Equal(expected, converter.CanConvert(type));
        }

        [Theory]
        [InlineData(typeof(EnumTest1), EnumTest1.Item1, "RoslynSerializer.EnumTest1.Item1")]
        [InlineData(typeof(EnumTest2), EnumTest2.Item0, "RoslynSerializer.EnumTest2.Item0")]
        [InlineData(typeof(EnumTest2), EnumTest2.Item1, "RoslynSerializer.EnumTest2.Item1")]
        [InlineData(typeof(EnumTest2), EnumTest2.Item2, "RoslynSerializer.EnumTest2.Item2")]
        [InlineData(typeof(EnumTest2), EnumTest2.Item1 | EnumTest2.Item3, "RoslynSerializer.EnumTest2.Item1 | RoslynSerializer.EnumTest2.Item3")]
        public void BasicEnumTest(Type type, object value, string expected)
        {
            var serializer = new SourceCodeSerializer();
            var converter = new EnumConverter();
            var result = converter.ConvertSyntax(type, value, serializer);

            _helper.WriteLine($"Expected: {expected}");
            _helper.WriteLine($"Result: {result}");

            Assert.Equal(expected, result.ToString());
        }

        [Theory]
        [InlineData(typeof(EnumTest1), EnumTest1.Item1, "EnumTest1.Item1")]
        [InlineData(typeof(EnumTest2), EnumTest2.Item0, "EnumTest2.Item0")]
        [InlineData(typeof(EnumTest2), EnumTest2.Item1, "EnumTest2.Item1")]
        [InlineData(typeof(EnumTest2), EnumTest2.Item2, "EnumTest2.Item2")]
        [InlineData(typeof(EnumTest2), EnumTest2.Item1 | EnumTest2.Item3, "EnumTest2.Item1 | EnumTest2.Item3")]
        public void BasicEnumTestWithUsing(Type type, object value, string expected)
        {
            var settings = new SerializerSettings
            {
                Usings = ImmutableArray.Create("RoslynSerializer")
            };
            var serializer = new SourceCodeSerializer(settings);
            var converter = new EnumConverter();
            var result = converter.ConvertSyntax(type, value, serializer);

            _helper.WriteLine($"Expected: {expected}");
            _helper.WriteLine($"Result: {result}");

            Assert.Equal(expected, result.ToString());
        }
    }

    public enum EnumTest1
    {
        Item1,
        Item2
    };

    [Flags]
    public enum EnumTest2
    {
        Item0 = 0x0,
        Item1 = 0x1,
        Item2 = 0x2,
        Item3 = 0x4
    }
}
