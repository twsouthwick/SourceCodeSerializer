using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RoslynSerializer
{

    public class BasicTests
    {
        private readonly ITestOutputHelper _helper;

        public BasicTests(ITestOutputHelper helper)
        {
            _helper = helper;
        }

        [Fact]
        public void IntegerProperty()
        {
            using (var log = new StringWriter())
            {
                var obj = new TestClass1 { Test = 5 };

                var node = SourceCodeSerializer.Create()
                    .AddTextWriter(log)
                    .Serialize(obj);

                _helper.WriteLine(log.ToString());

                var expected = @"new RoslynSerializer.BasicTests+TestClass1
{
    Test = 5
}";

                Assert.Equal(log.ToString(), expected);
            }
        }

        [Fact]
        public void StringProperty()
        {
            using (var log = new StringWriter())
            {
                var obj = new TestClass2 { Test = "Hello world" };

                var node = SourceCodeSerializer.Create()
                    .AddTextWriter(log)
                    .Serialize(obj);

                _helper.WriteLine(log.ToString());

                var expected = @"new RoslynSerializer.BasicTests+TestClass2
{
    Test = ""Hello world""
}";

                Assert.Equal(log.ToString(), expected);
            }
        }

        [Fact]
        public void NestedObjects()
        {
            using (var log = new StringWriter())
            {
                var obj = new TestClass3
                {
                    Class1 = new TestClass1 { Test = 6 },
                    Class2 = new TestClass2 { Test = "Hello world" }
                };

                var node = SourceCodeSerializer.Create()
                    .AddTextWriter(log)
                    .Serialize(obj);

                _helper.WriteLine(log.ToString());

                var expected = @"new RoslynSerializer.BasicTests+TestClass3
{
    Class1 = new RoslynSerializer.BasicTests+TestClass1
    {
        Test = 6
    },
    Class2 = new RoslynSerializer.BasicTests+TestClass2
    {
        Test = ""Hello world""
    }
}";

                Assert.Equal(log.ToString(), expected);
            }
        }

        [Fact]
        public void NestedObjectsWithClass()
        {
            using (var log = new StringWriter())
            {
                var obj = new TestClass3
                {
                    Class1 = new TestClass1 { Test = 6 },
                    Class2 = new TestClass2 { Test = "Hello world" }
                };

                var node = SourceCodeSerializer.Create()
                    .AddTextWriter(log)
                    .AddCreateMethod("Factory")
                    .Serialize(obj);

                _helper.WriteLine(log.ToString());

                var expected = @"using System;

partial class Factory
{
    public RoslynSerializer.BasicTests+TestClass3Create()
    {
        return new RoslynSerializer.BasicTests+TestClass3
        {
            Class1 = new RoslynSerializer.BasicTests+TestClass1
            {
                Test = 6
            },
            Class2 = new RoslynSerializer.BasicTests+TestClass2
            {
                Test = ""Hello world""
            }
        };
    }
}";

                Assert.Equal(log.ToString(), expected);
            }
        }

        private class TestClass1
        {
            public int Test { get; set; }
        }

        private class TestClass2
        {
            public string Test { get; set; }
        }

        private class TestClass3
        {
            public TestClass1 Class1 { get; set; }

            public TestClass2 Class2 { get; set; }
        }
    }
}
