using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace RoslynSerializer
{

    public class BasicTests : IDisposable
    {
        private readonly TextWriter _textWriter;

        public BasicTests(ITestOutputHelper helper)
        {
            _textWriter = helper.ToTextWriter();
        }

        public void Dispose()
        {
            _textWriter.Dispose();
        }

        [Fact]
        public void Test1()
        {
            var serializer = new SourceCodeSerializer();

            var node = serializer.Serialize(new TestClass1());
            serializer.Serialize(_textWriter, node);
        }

        private class TestClass1
        {
            public int Test { get; set; } = 5;
        }
    }
}
