using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TWLogAnalyzer.voice
{
    interface VoiceAdapter : IDisposable
    {
        void Speak(string message);
        void Announce(string info);
    }
}
