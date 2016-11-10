using RoslynSerializer.Converters;
using System;
using System.Linq;
using Xunit;

namespace RoslynSerializer.Test
{
    public class ObjectConverterTests
    {
        [Fact]
        public void SimpleClass()
        {
            var converter = new ObjectConverter();
            var properties = converter.GetProperties(typeof(A));

            Assert.Equal(new[] { "A1", "A2" }, properties.Select(p => p.Name));
        }

        [Fact]
        public void SimpleDerivedClass()
        {
            var converter = new ObjectConverter();
            var properties = converter.GetProperties(typeof(B));

            Assert.Equal(new[] { "A1", "A2", "B1" }, properties.Select(p => p.Name));
        }

        [Fact]
        public void HiddenDerivedClass()
        {
            var converter = new ObjectConverter();
            var properties = converter.GetProperties(typeof(C));

            Assert.Equal(new[] { "A1", "C1" }, properties.Select(p => p.Name));
        }

        public class A
        {
            public string A1 { get; set; }
            public virtual string A2 { get; set; }
        }

        public class B : A
        {
            public string B1 { get; set; }
        }

        public class C : A
        {
            public string C1 { get; set; }

            [Ignore]
            public override string A2 { get; set; }
        }

        public class IgnoreAttribute : Attribute { }
    }
}
