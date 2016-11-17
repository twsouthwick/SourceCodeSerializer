using System.Collections.Immutable;
using Xunit;
using Xunit.Abstractions;

namespace SourceCodeSerializer
{
    public class Example
    {
        private readonly ITestOutputHelper _helper;

        public Example(ITestOutputHelper helper)
        {
            _helper = helper;
        }

        [Fact]
        public void ReadmeExample()
        {
            var j = new Foo();
            j.Bar = 5;
            j.Bar2 = new FooBar { A = "hello" };

            var settings = new SerializerSettings
            {
                Usings = ImmutableArray.Create("SourceCodeSerializer")
            };

            var result = SourceCodeSerializer.Serialize(j, settings);

            _helper.WriteLine(result);
        }
    }

    public class Foo
    {
        public int Bar { get; set; }
        public FooBar Bar2 { get; set; }
    }

    public class FooBar
    {
        public string A { get; set; }
    }
}
