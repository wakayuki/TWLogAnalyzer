using System;

namespace TWLogAnalyzer.tw
{
    class TwException : Exception
    {
        public TwException(string message) : base(message) { }

        public TwException(string message, Exception inner) : base(message, inner) { }
    }
}
