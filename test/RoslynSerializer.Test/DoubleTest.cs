using RoslynSerializer.Converters;
using Xunit;

namespace RoslynSerializer.Test
{
    public class DoubleTest
    {
        [Theory]
        [InlineData(1.0d, "1d")]
        [InlineData(1.3d, "1.3d")]
        [InlineData(double.MinValue, "double.MinValue")]
        [InlineData(double.MaxValue, "double.MaxValue")]
        public void TestDouble(double value, string expected)
        {
            var primitive = new PrimitiveConverter();
            var converted = primitive.ConvertSyntax(typeof(double), value, null);

            Assert.Equal(expected, converted.ToString());
        }
    }
}
