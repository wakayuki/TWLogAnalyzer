using System;
using System.Configuration;
using Microsoft.Win32;
using TWLogAnalyzer.common;

namespace TWLogAnalyzer.tw
{

    /// <summary>
    /// Talse Weaver関係の操作を統括するクラス
    /// </summary>
    public class TwManager
    {
        public string InstDir { get; private set; }
        public TwLogReceiver Receiver { get; private set; }
        private TwLogWatcher Watcher = null;

        public bool IsWatching { get; private set; }

        public TwManager(TwLogReceiver receiver)
        {
            this.Receiver = receiver;
            this.ResolveTwInstallDirectory();
        }

        private void ResolveTwInstallDirectory()
        {
            string regKey;
            if (Environment.Is64BitProcess)
            {
                regKey = ConfigurationManager.AppSettings["TwReg64"];
            }
            else
            {
                regKey = ConfigurationManager.AppSettings["TwReg32"];
            }

            object regObject = Registry.GetValue(regKey, ConfigurationManager.AppSettings["TwRegInstKey"], null);
            if (regObject == null)
            {
                throw new TwException("Talese Weaverのインストールディレクトリの取得に失敗しました。");
            }
            InstDir = regObject.ToString();
            Logger.Instance.PutMessage("Install Dir: " + InstDir);
        }


        public void WatchStart()
        {
            // 監視中だった場合は一度監視を止める
            if (IsWatching)
            {
                WatchStop();
            }

            lock (this)
            {
                // 最初に昨日と今日のログを読み込んで必要な情報を集める
                Receiver.StopRealTimeWatching();
                TwLogWatcher.CheckWholeFile(InstDir, DateTime.Now.AddDays(-1), Receiver);
                TwLogWatcher.CheckWholeFile(InstDir, DateTime.Now, Receiver);
                Receiver.StartRealTimeWatching();

                // 監視開始
                Watcher = new TwLogWatcher(InstDir, Receiver);
                Watcher.Start();
                IsWatching = true;
            }
        }

        public void WatchStop()
        {
            lock (this)
            {
                if (!IsWatching)
                {
                    return;
                }

                Watcher.Stop();
                Watcher = null;
                IsWatching = false;
                Receiver.StopRealTimeWatching();
            }
        }
    }
}
