Source Code Serializer
=================

[![Build status](https://ci.appveyor.com/api/projects/status/1uj500v255y70odw?svg=true)](https://ci.appveyor.com/project/twsouthwick/sourcecodeserializer)
[![MyGet](https://img.shields.io/myget/source-code-serializer/v/RoslynSerializer.svg?style=plastic)]()

This is a tool to take a .NET object representation in memory and serialize it to C# code. This is useful for creating test data, where an object can be serialized to C# and easily replicated within a test case.

For example:

```csharp
using System.Collections.Immutable;
using Xunit;
using Xunit.Abstractions;

namespace RoslynSerializer
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
                Usings = ImmutableArray.Create("RoslynSerializer")
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

```

The serialized `result` then looks like this:

```
new Foo
{
    Bar = 5,
    Bar2 = new FooBar
    {
        A = @"hello"
    }
}
```

Serialization settings
----------

The serializer takes a settings object that determines how things are serialized. These settings affect how the source code is generated and can be supplied
during the call to `SourceCodeSerializer.Serialize`.

```csharp
public class SerializerSettings
{
    public ImmutableArray<ExpressionConverter> Converters { get; set; }

    public bool ObjectInitializationNewLine { get; set; } = true;

    public SourceGenerator Generator { get; set; }

    public ImmutableArray<string> Usings { get; set; } = ImmutableArray<string>.Empty;
}
```

ExpressionConverter
-------------------

Every type that is to be serialized will be serialized via either a built in or custom `ExpressionConverter`. Many primitives and structs are handled, but some may need to be added manually. There is a default object 
converter that serializes all public properties with a getter and setter. Properties with an attribute called `IncludeAttribute` will also be included, and properties with an attribute named `ExcludeAttribute` will be
excluded. In order to customize which converters are used, the property `SerializerSettings.Converters` may be modified; the default one should work for many basic cases. 

SourceGenerator
---------------

Source generators are objects that derive from `SourceGenerator` and provide a way to customize how the final source is generated. The built in generators are:

- `ConstructorGenerator`: This generator subclasses the class and sets the values within the ConstructorGenerator
- `FactoryMethodGenerator`: This generator will create a factory method that initializes all the values

A source generator is set by adding it to the `SerializerSettings.Generator`property.