namespace TWLogAnalyzer.tw
{
    public interface TwLogReceiver
    {
        /// <summary>
        /// 監視を始めた後の新規ログの通知
        /// </summary>
        /// <param name="log"></param>
        void Receive(TwChatLog log);

        /// <summary>
        /// 過去ログ読み込み開始を通知。これ以降、notifyLogLoadCompleted()が呼ばれるまでに通知されるログは過去ログ。
        /// notifyLogLoadStarted()/notifyLogLoadCompleted()のどちらも実行されていない場合はリアルタイムログ。
        /// </summary>
        void StopRealTimeWatching();

        /// <summary>
        /// 過去ログの読み込み終了を通知。これ以降、notifyLogLoadCompleted()が呼ばれるまでに通知されるログはリアルタイムログ。
        /// notifyLogLoadStarted()/notifyLogLoadCompleted()のどちらも実行されていない場合はリアルタイムログ。
        /// </summary>
        void StartRealTimeWatching();
    }
}
