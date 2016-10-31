using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace RoslynSerializer
{
    public class SerializerSettings
    {
        private readonly ImmutableDictionary<string, bool> _values;

        public static SerializerSettings Create()
        {
            return new SerializerSettings(ImmutableDictionary.Create<string, bool>(StringComparer.Ordinal));
        }

        private SerializerSettings(ImmutableDictionary<string, bool> values)
        {
            _values = values;
        }

        public bool ObjectInitializationNewLine => GetValue();

        private bool GetValue([CallerMemberName]string key = null)
        {
            bool value;
            if (_values.TryGetValue(key, out value))
            {
                return value;
            }

            return false;
        }

        public SerializerSettings WithObjectInitializationNewLine(bool value)
        {
            return SetValue(nameof(ObjectInitializationNewLine), value);
        }

        private SerializerSettings SetValue(string key, bool value)
        {
            return new SerializerSettings(_values.Add(key, value));
        }
    }
}
