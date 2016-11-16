using RoslynSerializer.Generators;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
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
            var obj = new TestClass1 { Test = 5 };
            var result = SourceCodeSerializer.Serialize(obj);
            var expected = @"new RoslynSerializer.TestClass1
{
    Test = 5
}";

            _helper.WriteLine(result);
            Assert.Equal(result, expected);
        }

        [Fact]
        public void IntegerPropertyDefaultValue()
        {
            var obj = new TestClass1 { Test = 0 };
            var result = SourceCodeSerializer.Serialize(obj);
            var expected = @"new RoslynSerializer.TestClass1 { }";

            _helper.WriteLine(result);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void IntegerPropertyNoNewLine()
        {
            var obj = new TestClass1 { Test = 5 };

            var settings = new SerializerSettings
            {
                ObjectInitializationNewLine = false
            };

            var result = SourceCodeSerializer.Serialize(obj, settings);
            var expected = @"new RoslynSerializer.TestClass1 { Test = 5 }";

            _helper.WriteLine(result);
            Assert.Equal(result, expected);
        }

        [Fact]
        public void StringProperty()
        {
            var obj = new TestClass2 { Test = "Hello world" };
            var result = SourceCodeSerializer.Serialize(obj);

            var expected = @"new RoslynSerializer.TestClass2
{
    Test = @""Hello world""
}";

            _helper.WriteLine(result);
            Assert.Equal(result, expected);
        }

        [Fact]
        public void NestedObjects()
        {
            var obj = new TestClass3
            {
                Class1 = new TestClass1 { Test = 6 },
                Class2 = new TestClass2 { Test = "Hello world" }
            };

            var result = SourceCodeSerializer.Serialize(obj);
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

            _helper.WriteLine(result);
            Assert.Equal(result, expected);
        }

        [Fact]
        public void NestedObjectsWithClass()
        {
            var obj = new TestClass3
            {
                Class1 = new TestClass1 { Test = 6 },
                Class2 = new TestClass2 { Test = "Hello world" }
            };

            var settings = new SerializerSettings
            {
                Generator = new FactoryMethodGenerator("Test", "Factory", "Create"),
                Usings = ImmutableArray.Create("System")
            };

            var result = SourceCodeSerializer.Serialize(obj, settings);

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

            _helper.WriteLine(result);
            Assert.Equal(result, expected);
        }

        [Fact]
        public void NestedObjectsWithClassAndUsings()
        {
            var obj = new TestClass3
            {
                Class1 = new TestClass1 { Test = 6 },
                Class2 = new TestClass2 { Test = "Hello world" }
            };

            var settings = new SerializerSettings
            {
                Generator = new FactoryMethodGenerator("Test", "Factory", "Create"),
                Usings = ImmutableArray.Create("System", "RoslynSerializer")
            };

            var result = SourceCodeSerializer.Serialize(obj, settings);

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
            _helper.WriteLine(result);
            Assert.Equal(result, expected);
        }


        [Fact]
        public void NestedObjectsWithConstructorAndUsings()
        {
            var obj = new TestClass3
            {
                Class1 = new TestClass1 { Test = 6 },
                Class2 = new TestClass2 { Test = "Hello world" }
            };

            var settings = new SerializerSettings
            {
                Generator = new ConstructorGenerator("Test", "Factory"),
                Usings = ImmutableArray.Create("System", "RoslynSerializer")
            };

            var result = SourceCodeSerializer.Serialize(obj, settings);

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

            _helper.WriteLine(result);
            Assert.Equal(result, expected);
        }

        [Fact]
        public void GenericList()
        {
            var obj = new GenericList1
            {
                List = new List<TestClass2>()
            };

            obj.List.Add(new TestClass2 { Test = "Item1" });
            obj.List.Add(new TestClass2 { Test = "Item2" });

            var settings = new SerializerSettings
            {
                Usings = ImmutableArray.Create("RoslynSerializer")
            };

            var result = SourceCodeSerializer.Serialize(obj, settings);

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

            _helper.WriteLine(result);
            Assert.Equal(result, expected);
        }


        [Fact]
        public void GenericList2()
        {
            var obj = new GenericList2
            {
                List = new TestClass2[]
                {
                        new TestClass2 { Test = "Item1" },
                        new TestClass2 { Test = "Item2" }
                }
            };

            var settings = new SerializerSettings
            {
                Usings = ImmutableArray.Create("RoslynSerializer")
            };

            var result = SourceCodeSerializer.Serialize(obj, settings);

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

            _helper.WriteLine(result);
            Assert.Equal(result, expected);
        }

        [Fact]
        public void NonGenericList()
        {
            var obj = new NonGenericList1
            {
                List = new object[]
                {
                        new TestClass1 { Test = 2 },
                        new TestClass2 { Test = "Item2" }
                }
            };

            var settings = new SerializerSettings
            {
                Usings = ImmutableArray.Create("RoslynSerializer")
            };

            var result = SourceCodeSerializer.Serialize(obj, settings);

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

            _helper.WriteLine(result);
            Assert.Equal(result, expected);
        }

        [Fact]
        public void DerivedMembers()
        {
            var obj = new DerivedTest1
            {
                Other = 1,
                Test = 6
            };

            var settings = new SerializerSettings
            {
                Usings = ImmutableArray.Create("RoslynSerializer")
            };

            var result = SourceCodeSerializer.Serialize(obj, settings);

            var expected = @"new DerivedTest1
{
    Other = 1,
    Test = 6
}";

            _helper.WriteLine(result);
            Assert.Equal(result, expected);
        }

        [Fact]
        public void IgnorablePropertyTest()
        {
            var obj = new IgnorableProperty
            {
                Field1 = 1,
                Field2 = 2,
                Field3 = 3,
            };

            var settings = new SerializerSettings
            {
                Usings = ImmutableArray.Create("RoslynSerializer")
            };

            var result = SourceCodeSerializer.Serialize(obj, settings);

            var expected = @"new IgnorableProperty
{
    Field1 = 1,
    Field3 = 3
}";

            _helper.WriteLine(result);
            Assert.Equal(result, expected);
        }

        [Fact]
        public void IgnorableProperty2Test()
        {
            var obj = new IgnorableProperty2
            {
                Field1 = 1,
                Field2 = 2,
                Field3 = 3,
            };

            var settings = new SerializerSettings
            {
                Usings = ImmutableArray.Create("RoslynSerializer")
            };

            var result = SourceCodeSerializer.Serialize(obj, settings);

            var expected = @"new IgnorableProperty2
{
    Field1 = 1,
    Field2 = 2,
    Field3 = 3
}";

            _helper.WriteLine(result);
            Assert.Equal(result, expected);
        }

        [Fact]
        public void IgnoreDefaultValueTest()
        {
            var obj = new ClassWithUShort { Item = 0 };

            var settings = new SerializerSettings
            {
                IgnoreDefaultValues = false,
                Usings = ImmutableArray.Create("RoslynSerializer")
            };

            var result = SourceCodeSerializer.Serialize(obj, settings);

            var expected = @"new ClassWithUShort
{
    Item = 0
}";

            _helper.WriteLine(result);
            Assert.Equal(result, expected);
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

    public class ClassWithUShort
    {
        public ushort Item { get; set; }
    }
}
