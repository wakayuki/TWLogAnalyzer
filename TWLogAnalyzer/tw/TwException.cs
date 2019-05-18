using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TWLogAnalyzer.tw
{
    class TwException : Exception
    {
        public TwException(string message) : base(message) { }

        public TwException(string message, Exception inner) : base(message, inner) { }
    }
}
