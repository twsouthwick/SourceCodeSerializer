using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

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

                var expected = @"new RoslynSerializer.TestClass1
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

                var expected = @"new RoslynSerializer.TestClass2
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

                var expected = @"new RoslynSerializer.TestClass3
{
    Class1 = new RoslynSerializer.TestClass1
    {
        Test = 6
    },
    Class2 = new RoslynSerializer.TestClass2
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
    public RoslynSerializer.TestClass3 Create()
    {
        return new RoslynSerializer.TestClass3
        {
            Class1 = new RoslynSerializer.TestClass1
            {
                Test = 6
            },
            Class2 = new RoslynSerializer.TestClass2
            {
                Test = ""Hello world""
            }
        };
    }
}";

                Assert.Equal(log.ToString(), expected);
            }
        }


        [Fact]
        public void NestedObjectsWithClassAndUsings()
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
                    .AddUsing("RoslynSerializer")
                    .Serialize(obj);

                _helper.WriteLine(log.ToString());

                var expected = @"using System;
using RoslynSerializer;

partial class Factory
{
    public TestClass3 Create()
    {
        return new TestClass3
        {
            Class1 = new TestClass1
            {
                Test = 6
            },
            Class2 = new TestClass2
            {
                Test = ""Hello world""
            }
        };
    }
}";

                Assert.Equal(log.ToString(), expected);
            }
        }

        [Fact]
        public void EnumValues()
        {
            using (var log = new StringWriter())
            {
                var obj = new TestClass4
                {
                    Color = ConsoleColor.Red
                };

                var node = SourceCodeSerializer.Create()
                    .AddTextWriter(log)
                    .Serialize(obj);

                _helper.WriteLine(log.ToString());

                var expected = @"new RoslynSerializer.TestClass4
{
    Color = ConsoleColor.Red
}";

                Assert.Equal(log.ToString(), expected);
            }
        }
    }

    public class TestClass1
    {
        public int Test { get; set; }
    }

    public class TestClass2
    {
        public string Test { get; set; }
    }

    public class TestClass3
    {
        public TestClass1 Class1 { get; set; }

        public TestClass2 Class2 { get; set; }
    }

    public class TestClass4
    {
        public ConsoleColor Color { get; set; }
    }
}
