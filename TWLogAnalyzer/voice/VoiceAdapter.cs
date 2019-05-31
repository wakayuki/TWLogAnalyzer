using System;

namespace TWLogAnalyzer.voice
{
    interface VoiceAdapter : IDisposable
    {
        void Speak(string message);
        void Announce(string info);
    }
}
