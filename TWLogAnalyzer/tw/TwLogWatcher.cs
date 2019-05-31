using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using TWLogAnalyzer.common;

namespace TWLogAnalyzer.tw
{
    class TwLogWatcher : IDisposable
    {
        /// <summary>
        /// 監視なしで、単一のログファイルを読み込む。
        /// </summary>
        /// <param name="instDir"></param>
        /// <param name="date"></param>
        /// <param name="receiver"></param>
        public static void CheckWholeFile(string instDir, DateTime date, TwLogReceiver receiver)
        {
            string logfile = String.Format(@"{0}\ChatLog\TWChatLog_{1}.html", instDir, date.ToString("yyyy_MM_dd"));
            if (!File.Exists(logfile))
            {
                return;
            }

            using (FileStream stream = new FileStream(logfile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (TextReader sr = new StreamReader(stream, Encoding.GetEncoding("Shift-JIS")))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        TwChatLog log = TwChatLog.createChatLog(date.ToString("yyyy/MM/dd"), line);
                        if (log == null) continue;
                        receiver.Receive(log);
                    }
                }
            }
        }

        private string LogDir;
        private TwLogReceiver Receiver;

        public bool IsWatching { private set; get; }

        public TwLogWatcher(string InstDir, TwLogReceiver Receiver)
        {
            this.LogDir = $"{InstDir}\\ChatLog";
            this.Receiver = Receiver;
        }

        public void Start()
        {
            Stop();
            lock (this)
            {
                IsWatching = true;
                // さぼりで、スレッドを立ち上げて監視する。FileSystemWatcherにしたい。
                Task.Run(() => WatchingThread());
            }
        }

        public void Stop()
        {
            lock (this)
            {
                if (IsWatching)
                {
                    IsWatching = false;
                }
            }
        }

        private void WatchingThread()
        {
            DateTime dateTime = DateTime.Now.Date;
            string dateString = dateTime.ToString("yyyy/MM/dd");
            FileInfo fileInfo = new FileInfo(String.Format(@"{0}\TWChatLog_{1}.html", LogDir, dateTime.ToString("yyyy_MM_dd")));
            long prevSize = fileInfo.Exists ? fileInfo.Length : 0;
            while (IsWatching)
            {
                try
                {
                    fileInfo.Refresh();
                    if (!fileInfo.Exists || prevSize == fileInfo.Length)
                    {
                        // サイズ変更ない場合、日付が変わったかもしれない
                        if (dateTime < DateTime.Now.Date)
                        {
                            dateTime = DateTime.Now.Date;
                            dateString = dateTime.ToString("yyyy/MM/dd");
                            fileInfo = new FileInfo(String.Format(@"{0}\TWChatLog_{1}.html", LogDir, dateTime.ToString("yyyy_MM_dd")));
                            prevSize = 0;

                            Logger.Instance.PutMessage($"Change target log file to {fileInfo.FullName}");
                        }

                        continue;
                    }

                    using (FileStream fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        fileStream.Position = prevSize;
                        using (StreamReader reader = new StreamReader(fileStream, Encoding.GetEncoding("shift_jis")))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                TwChatLog log = TwChatLog.createChatLog(dateString, line);
                                if (log == null)
                                {
                                    Logger.Instance.PutMessage($"Invalid format message: {line}");
                                    continue;
                                }
                                Receiver.Receive(log);
                            }
                            prevSize = fileStream.Position;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.PutMessage($"File Access Error: {ex.Message}");
                    Logger.Instance.PutMessage($"{ex.StackTrace}");
                }
                finally
                {
                    Thread.Sleep(1000);
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // ここってメモリ開放するところらしいんだけど、これはありなんだろうか。。。
                    Stop();
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        // ~TwLogWatcher()
        // {
        //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
        //   Dispose(false);
        // }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
