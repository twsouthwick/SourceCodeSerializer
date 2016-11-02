using RoslynSerializer.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
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
        public void IntegerPropertyDefaultValue()
        {
            using (var log = new StringWriter())
            {
                var obj = new TestClass1 { Test = 0 };

                var node = SourceCodeSerializer.Create()
                    .AddTextWriter(log)
                    .Serialize(obj);

                _helper.WriteLine(log.ToString());

                var expected = @"new RoslynSerializer.TestClass1 { }";

                Assert.Equal(expected, log.ToString());
            }
        }

        [Fact]
        public void IntegerPropertyNoNewLine()
        {
            using (var log = new StringWriter())
            {
                var obj = new TestClass1 { Test = 5 };

                var node = SourceCodeSerializer.Create()
                    .AddTextWriter(log)
                    .WithSettings(SerializerSettings.Create().WithObjectInitializationNewLine(false))
                    .Serialize(obj);

                _helper.WriteLine(log.ToString());

                var expected = @"new RoslynSerializer.TestClass1 { Test = 5 }";

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
    Test = @""Hello world""
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
        Test = @""Hello world""
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
                    .AddCreateMethod("Test", "Factory")
                    .Serialize(obj);

                _helper.WriteLine(log.ToString());

                var expected = @"using System;

namespace Test
{
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
                    Test = @""Hello world""
                }
            };
        }
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
                    .AddCreateMethod("Test", "Factory")
                    .AddUsing("RoslynSerializer")
                    .Serialize(obj);

                _helper.WriteLine(log.ToString());

                var expected = @"using System;
using RoslynSerializer;

namespace Test
{
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
                    Test = @""Hello world""
                }
            };
        }
    }
}";

                Assert.Equal(log.ToString(), expected);
            }
        }


        [Fact]
        public void NestedObjectsWithConstructorAndUsings()
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
                    .AddConstructor("Test", "Factory")
                    .AddUsing("RoslynSerializer")
                    .Serialize(obj);

                _helper.WriteLine(log.ToString());

                var expected = @"using System;
using RoslynSerializer;

namespace Test
{
    partial class Factory
    {
        public Factory()
        {
            Class1 = new TestClass1
            {
                Test = 6
            };
            Class2 = new TestClass2
            {
                Test = @""Hello world""
            };
        }
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

        [Fact]
        public void GenericList()
        {
            using (var log = new StringWriter())
            {
                var obj = new GenericList1
                {
                    List = new List<TestClass2>()
                };

                obj.List.Add(new TestClass2 { Test = "Item1" });
                obj.List.Add(new TestClass2 { Test = "Item2" });

                var node = SourceCodeSerializer.Create()
                    .AddTextWriter(log)
                    .AddUsing("RoslynSerializer")
                    .Serialize(obj);

                _helper.WriteLine(log.ToString());

                var expected = @"new GenericList1
{
    List = new TestClass2[] {
        new TestClass2
        {
            Test = @""Item1""
        },
        new TestClass2
        {
            Test = @""Item2""
        }
    }
}";

                Assert.Equal(log.ToString(), expected);
            }
        }


        [Fact]
        public void GenericList2()
        {
            using (var log = new StringWriter())
            {
                var obj = new GenericList2
                {
                    List = new TestClass2[]
                    {
                        new TestClass2 { Test = "Item1" },
                        new TestClass2 { Test = "Item2" }
                    }
                };

                var node = SourceCodeSerializer.Create()
                    .AddTextWriter(log)
                    .AddUsing("RoslynSerializer")
                    .Serialize(obj);

                _helper.WriteLine(log.ToString());

                var expected = @"new GenericList2
{
    List = new TestClass2[] {
        new TestClass2
        {
            Test = @""Item1""
        },
        new TestClass2
        {
            Test = @""Item2""
        }
    }
}";

                Assert.Equal(log.ToString(), expected);
            }
        }

        [Fact]
        public void NonGenericList()
        {
            using (var log = new StringWriter())
            {
                var obj = new NonGenericList1
                {
                    List = new object[]
                    {
                        new TestClass1 { Test = 2 },
                        new TestClass2 { Test = "Item2" }
                    }
                };

                var node = SourceCodeSerializer.Create()
                    .AddTextWriter(log)
                    .AddUsing("RoslynSerializer")
                    .Serialize(obj);

                _helper.WriteLine(log.ToString());

                var expected = @"new NonGenericList1
{
    List = new object[] {
        new TestClass1
        {
            Test = 2
        },
        new TestClass2
        {
            Test = @""Item2""
        }
    }
}";

                Assert.Equal(log.ToString(), expected);
            }
        }

        [Fact]
        public void DerivedMembers()
        {
            using (var log = new StringWriter())
            {
                var obj = new DerivedTest1
                {
                    Other = 1,
                    Test = 6
                };

                var node = SourceCodeSerializer.Create()
                    .AddTextWriter(log)
                    .AddUsing("RoslynSerializer")
                    .Serialize(obj);

                _helper.WriteLine(log.ToString());

                var expected = @"new DerivedTest1
{
    Other = 1,
    Test = 6
}";

                Assert.Equal(log.ToString(), expected);
            }
        }

        [Fact]
        public void IgnorablePropertyTest()
        {
            using (var log = new StringWriter())
            {
                var obj = new IgnorableProperty
                {
                    Field1 = 1,
                    Field2 = 2,
                    Field3 = 3,
                };

                var node = SourceCodeSerializer.Create()
                    .AddTextWriter(log)
                    .AddUsing("RoslynSerializer")
                    .Serialize(obj);

                _helper.WriteLine(log.ToString());

                var expected = @"new IgnorableProperty
{
    Field1 = 1,
    Field3 = 3
}";

                Assert.Equal(log.ToString(), expected);
            }
        }

        [Fact]
        public void IgnorableProperty2Test()
        {
            using (var log = new StringWriter())
            {
                var obj = new IgnorableProperty2
                {
                    Field1 = 1,
                    Field2 = 2,
                    Field3 = 3,
                };

                var node = SourceCodeSerializer.Create()
                    .AddTextWriter(log)
                    .AddUsing("RoslynSerializer")
                    .Serialize(obj);

                _helper.WriteLine(log.ToString());

                var expected = @"new IgnorableProperty2
{
    Field1 = 1,
    Field2 = 2,
    Field3 = 3
}";

                Assert.Equal(log.ToString(), expected);
            }
        }

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

    public class GenericList1
    {
        public IList<TestClass2> List { get; set; }
    }

    public class GenericList2
    {
        public TestClass2[] List { get; set; }
    }

    public class NonGenericList1
    {
        public IList List { get; set; }
    }

    public class DerivedTest1 : TestClass1
    {
        public int Other { get; set; }
    }

    public class IgnoreAttribute : Attribute { }

    public class IgnorableProperty
    {
        public virtual int Field1 { get; set; }
        [Ignore]
        public virtual int Field2 { get; set; }
        public virtual int Field3 { get; set; }
    }

    public class IgnorableProperty2 : IgnorableProperty
    {
        public override int Field2 { get; set; }
    }
}
