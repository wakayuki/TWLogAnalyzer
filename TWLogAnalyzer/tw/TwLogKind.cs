using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TWLogAnalyzer.tw
{
    public enum TwLogKind
    {
        // 単純に色で分けるときに使うやつ
        OPEN,
        HINT,
        TEAM,
        CLUB,
        SYSTEM,
        WHISPER,
        SHOUT,
        NOTICE1,
        NOTICE2,
        GM,
        UNKNOWN,

        // 特別扱いするやつ
        EXP,
        RUNE_EXP,
    }
}
