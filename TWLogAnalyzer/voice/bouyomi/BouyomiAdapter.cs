using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;
using FNF.Utility;
using TWLogAnalyzer.common;
using TWLogAnalyzer.tw;

namespace TWLogAnalyzer.voice.bouyomi
{
    class BouyomiAdapter : VoiceAdapter
    {
        private BouyomiChanClient Client = new BouyomiChanClient();
        private bool IsDisposed = false;

        private Process SelfBootBouyomiChan;

        public BouyomiAdapter(string bouyomiChanPath)
        {
            BouyomiChanStart(bouyomiChanPath);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (stopwatch.Elapsed.Seconds < 30)
            {
                try
                {
                    Speak("棒読みちゃん連携を開始しました");
                    return;
                }
                catch
                {
                    // 起動完了まではエラーになるので、いったん握りつぶす
                }
                Thread.Sleep(3000);
            }

            throw new TwException("棒読みちゃん接続タイムアウト");
        }

        ~BouyomiAdapter()
        {
            if (!IsDisposed)
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            IsDisposed = true;
            try
            {
                Client.Dispose();

                // ↑でエラーが起きると棒読みちゃんが残ったままになるが、手動で落とせるしまぁいいだろう。。。
                BouyomiChanShutdown();
            }
            catch (Exception ex)
            {
                Logger.Instance.PutMessage($"棒読みちゃん終了エラー: {ex.Message}");
            }
        }

        public void Speak(string message)
        {
            Client.AddTalkTask(message);
        }

        public void Announce(string info)
        {
            Client.AddTalkTask(info);
        }

        private void BouyomiChanStart(string bouyomiChanPath)
        {
            // 起動しているかチェック。さぼりで、bouyomichan.exeがあるか調べる
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            foreach (Process process in Process.GetProcesses())
            {
                try
                {
                    if (ConfigurationManager.AppSettings["Bouyomi.filename"].Equals(Path.GetFileName(process.MainModule.FileName), StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }
                }
                catch
                {
                    // 権限のないプロセスや32bitから64bitのプロセスを参照しようとしたときに例外がでるため、
                    // ここではログにも出さずにNOP
                }
            }
            Logger.Instance.PutMessage($"棒読みちゃんプロセス検索時間: {stopwatch.Elapsed.Seconds}秒");

            // 起動していない場合は自分で起動する
            SelfBootBouyomiChan = Process.Start(bouyomiChanPath);
            SelfBootBouyomiChan.WaitForInputIdle();
        }

        private void BouyomiChanShutdown()
        {
            if (SelfBootBouyomiChan != null)
            {
                SelfBootBouyomiChan.CloseMainWindow();
            }
        }
    }
}
