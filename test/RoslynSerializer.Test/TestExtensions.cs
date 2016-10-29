using System.IO;
using System.Text;
using Xunit.Abstractions;

namespace RoslynSerializer
{
    internal static class TestExtensions
    {
        public static TextWriter ToTextWriter(this ITestOutputHelper helper) => new OutputHelperTextWriter(helper);

        private class OutputHelperTextWriter : TextWriter
        {
            private readonly ITestOutputHelper _helper;
            private readonly StringBuilder _sb;

            public OutputHelperTextWriter(ITestOutputHelper helper)
            {
                _helper = helper;
                _sb = new StringBuilder();
            }

            public override Encoding Encoding { get; } = Encoding.UTF8;

            public override void Write(char value)
            {
                _sb.Append(value);
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                if (disposing)
                {
                    _helper.WriteLine(_sb.ToString());
                }
            }
        }
    }
}
