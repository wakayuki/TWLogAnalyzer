using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Configuration;

using TWLogAnalyzer.common;

namespace TWLogAnalyzer.tw
{
    /// <summary>
    /// TWのチャットログ1個
    /// </summary>
    public class TwChatLog
    {
        /// <summary>
        /// チャットログの正規表現
        /// </summary>
        private static Regex Regex = new Regex(@"<font size=""2"" color=""white""> \[\s*(?<hour>[0-9].*)時\s*(?<min>[0-9].*)分\s*(?<sec>[0-9].*)秒\] <\/font> <font size=""2"" color=""(?<color>#[0-9a-f]*)"">(?<message>.*)<\/font><\/br>");

        /// <summary>
        /// 経験値ログの正規表現
        /// </summary>
        private static Regex ExpRegex = new Regex("^経験値が (?<exp>[0-9].*) 上がりました。");

        /// <summary>
        /// ルーン経験値ログの正規表現
        /// </summary>
        private static Regex RuneExpRegex = new Regex("^ルーン経験値が (?<rune_exp>[0-9].*) 上がりました。");

        private static Dictionary<string, TwLogKind> LogKindMap;

        static TwChatLog()
        {
            LogKindMap = new Dictionary<string, TwLogKind>();
            LogKindMap.Add(ConfigurationManager.AppSettings["Chat.Open"], TwLogKind.OPEN);
            LogKindMap.Add(ConfigurationManager.AppSettings["Chat.Hint"], TwLogKind.HINT);
            LogKindMap.Add(ConfigurationManager.AppSettings["Chat.System"], TwLogKind.SYSTEM);
            LogKindMap.Add(ConfigurationManager.AppSettings["Chat.Team"], TwLogKind.TEAM);
            LogKindMap.Add(ConfigurationManager.AppSettings["Chat.Club"], TwLogKind.CLUB);
            LogKindMap.Add(ConfigurationManager.AppSettings["Chat.Shout"], TwLogKind.SHOUT);
            LogKindMap.Add(ConfigurationManager.AppSettings["Chat.Notice1"], TwLogKind.NOTICE1);
            LogKindMap.Add(ConfigurationManager.AppSettings["Chat.Notice2"], TwLogKind.NOTICE2);
            LogKindMap.Add(ConfigurationManager.AppSettings["Chat.Whisper"], TwLogKind.WHISPER);
            LogKindMap.Add(ConfigurationManager.AppSettings["Chat.GM"], TwLogKind.GM);
        }

        private static TwLogKind ColorToLogKind(string color)
        {
            if (LogKindMap.ContainsKey(color))
            {
                return LogKindMap[color];
            }

            Logger.Instance.PutMessage($"Unknown message color: {color}");
            return TwLogKind.UNKNOWN;
        }

        public static TwChatLog createChatLog(string date, string line)
        {
            Match match = Regex.Match(line);
            if (!match.Success)
            {
                return null;
            }

            DateTime timeStamp = DateTime.Parse(String.Format("{0} {1}:{2}:{3}",
                date,
                match.Groups["hour"].Value,
                match.Groups["min"].Value,
                match.Groups["sec"].Value
                ));
            string message = match.Groups["message"].Value;
            TwLogKind kind = ColorToLogKind(match.Groups["color"].Value);

            var detail = toDetailLog(kind, message);

            return new TwChatLog(timeStamp, detail.kind, message, detail.value);
        }

        private static (TwLogKind kind, int value) toDetailLog(TwLogKind kind, string message)
        {
            if (kind == TwLogKind.SYSTEM)
            {
                Match ExpMatch = ExpRegex.Match(message);
                if (ExpMatch.Success)
                {
                    return (TwLogKind.EXP, int.Parse(ExpMatch.Groups["exp"].Value));
                }

                Match RuneExpMatch = RuneExpRegex.Match(message);
                if (RuneExpMatch.Success)
                {
                    return (TwLogKind.RUNE_EXP, int.Parse(RuneExpMatch.Groups["rune_exp"].Value));
                }
            }

            return (kind, 0);
        }

        public DateTime TimeStamp { get; private set; }

        public string Message { get; private set; }

        public TwLogKind Kind { get; private set; }

        /// <summary>
        /// 経験値ログの場合に経験値を入れておく場所。特殊ログがもっと増えたらいい方法考える
        /// </summary>
        public int Value { get; private set; }

        private TwChatLog(DateTime timeStamp, TwLogKind kind, string message, int value)
        {
            this.TimeStamp = timeStamp;
            this.Kind = kind;
            this.Message = message;
            this.Value = value;
        }
    }
}
