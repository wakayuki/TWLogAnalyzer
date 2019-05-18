using System;
using System.Runtime.Serialization;

namespace TWLogAnalyzer
{

    [DataContract]
    class TwLogAnalyzerSettings
    {
        [DataMember]
        public BouyomiChanSettings BousyomiChanSettings { set; get; } = new BouyomiChanSettings();
    }

    [DataContract]
    class BouyomiChanSettings
    {
        /// <summary>
        /// 棒読みちゃんの実行ファイル(BouyomiChan.exe)のパス
        /// </summary>
        [DataMember]
        public String exeFilePath { set; get; } = "BouyomiChan.exe";

        /// <summary>
        /// 棒読みちゃん連携が有効かどうか
        /// </summary>
        [DataMember]
        public bool IsEnabled { set; get; } = false;

        /// <summary>
        /// ボスが出るかもしれないアナウンスをするかどうか
        /// </summary>
        [DataMember]
        public bool IsBossAnnounceEnabled { set; get; } = false;
    }
}
