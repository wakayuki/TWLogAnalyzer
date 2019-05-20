using System;

using TWLogAnalyzer.tw;
using TWLogAnalyzer.common;
using TWLogAnalyzer.voice;
using System.Configuration;

namespace TWLogAnalyzer
{
    class MainWindowReceiver : TwLogReceiver
    {
        private static string BossGolronMessage = "ベリネンルミに「ゴルロン」が出現しました！";
        private static string BossGoldamofMessage = "ベリネンルミに「ゴルモダフ」が出現しました！";
        private static string BossPopTimeForamt = "yyyy/MM/dd HH:mm:ss";
        private static string BossNextTimeForamt = "yyyy/MM/dd HH:mm頃";

        /// <summary>
        /// リアルタイムログ通知モードかどうか。falseの場合は過去ログの読み込み
        /// </summary>
        private bool IsRealTimeLogMode = true;

        /// <summary>
        /// 前回のゴルロン沸き時間
        /// </summary>
        private DateTime GolronTime = DateTime.MinValue;

        /// <summary>
        /// 前回のゴルモダフ沸き時間
        /// </summary>
        private DateTime GolmdoafTime = DateTime.MinValue;

        private int GolronInterval;
        private int GolmodafIntaval;

        private MainWindow Window;
        private VoiceAdapter VoiceAdapter = null;

        private TwExpManager Exps = new TwExpManager();
        private TwExpManager RuneExps = new TwExpManager();

        public bool IsBossAnnounce { get; set; }

        public MainWindowReceiver(MainWindow window)
        {
            this.Window = window;

            GolronInterval = int.Parse(ConfigurationManager.AppSettings["Boss.Golron.Interval"]);
            GolmodafIntaval = int.Parse(ConfigurationManager.AppSettings["Boss.Golmodaf.Interval"]);
        }

        public void Receive(TwChatLog log)
        {
            // リアルタイム通知と初期化どどちらもチェックするやつ
            BossCheck(log);

            // これ以降は、リアルタイム通知以外は処理しない
            if (!IsRealTimeLogMode)
            {
                return;
            }

            UpdateExps(log);


            if (log.Kind == TwLogKind.CLUB)
            {
                Window.Dispatcher.Invoke((Action)(() =>
                {
                    Window.txtChatClub.AppendText(log.Message + Environment.NewLine);
                    Window.txtChatTeam.ScrollToEnd();
                    Speak(log.Message);
                }));
            }


            if (log.Kind == TwLogKind.TEAM)
            {
                Window.Dispatcher.Invoke((Action)(() =>
                {
                    Window.txtChatTeam.AppendText(log.Message + Environment.NewLine);
                    Window.txtChatTeam.ScrollToEnd();
                    Speak(log.Message);
                }));
            }
        }

        private void UpdateExps(TwChatLog log)
        {
            if (log.Kind == TwLogKind.EXP)
            {
                Exps.Update(log);
            }
            if (log.Kind == TwLogKind.RUNE_EXP)
            {
                RuneExps.Update(log);
            }
        }

        public long GetExpPerMin(int min)
        {
            return Exps.getExpPerMin(min);
        }

        public long GetRuneExpPerMin(int min)
        {
            return RuneExps.getExpPerMin(min);
        }

        public DateTime GetNextBossTime(DateTime last, int interval)
        {
            DateTime next = last.AddMinutes(interval);
            while (next < DateTime.Now)
            {
                next = next.AddMinutes(interval);
            }

            return next;
        }

        public void StopRealTimeWatching()
        {
            IsRealTimeLogMode = false;
        }

        public void StartRealTimeWatching()
        {
            IsRealTimeLogMode = true;

            // 読み込み終わったのでUIに初期値を詰める
            Window.Dispatcher.Invoke((Action)(() =>
            {
                Window.txtChatClub.Text = "";
                Window.txtChatTeam.Text = "";
            }));

            // 過去ログから読み込んだ情報でUIをアップデート
            UpdateBossView();

            // 経験値ログを初期化
            Exps.Reset();
            RuneExps.Reset();
        }

        private void UpdateBossView()
        {
            Window.Dispatcher.Invoke((Action)(() =>
            {
                if (!GolronTime.Equals(DateTime.MinValue))
                {
                    // これはMainWindow.xmls.csで処理するほうが筋な気がする。。。
                    Window.lblGolronTime.Content = GolronTime.ToString(BossPopTimeForamt);
                    DateTime NextBossTime = GetNextBossTime(GolronTime, GolronInterval);
                    Window.lblGolronNextTime.Content = NextBossTime.ToString(BossNextTimeForamt);
                    Window.UpdateNextGolronTime(NextBossTime);

                    // 2重更新をしないようにするため、1度更新に使った情報は消しておく
                    GolronTime = DateTime.MinValue;
                }

                if (!GolmdoafTime.Equals(DateTime.MinValue))
                {
                    Window.lblGolmodafTime.Content = GolmdoafTime.ToString(BossPopTimeForamt);
                    DateTime NextBossTime = GetNextBossTime(GolmdoafTime, GolmodafIntaval);
                    Window.lblGolmodafNextTime.Content = NextBossTime.ToString(BossNextTimeForamt);
                    Window.UpdateNextGolmodafTime(NextBossTime);

                    // 2重更新をしないようにするため、1度更新に使った情報は消しておく
                    GolmdoafTime = DateTime.MinValue;
                }
            }));
        }

        public void EnableVoiceAdapter(VoiceAdapter voiceAdapter)
        {
            if (this.VoiceAdapter != null)
            {
                DisableVoiceAdapter();
            }

            lock (this)
            {
                this.VoiceAdapter = voiceAdapter;
            }
        }

        public void DisableVoiceAdapter()
        {
            lock (this)
            {
                if (this.VoiceAdapter == null)
                {
                    return;
                }

                try
                {
                    this.VoiceAdapter.Dispose();
                }
                catch
                {
                    throw;
                }
                finally
                {
                    this.VoiceAdapter = null;
                }
            }
        }

        private void BossCheck(TwChatLog log)
        {
            if (log.Kind != TwLogKind.SYSTEM)
            {
                return;
            }

            if (log.Message.Equals(BossGolronMessage))
            {
                GolronTime = log.TimeStamp;
                if (IsRealTimeLogMode && IsBossAnnounce)
                {
                    Announce("ごるろん がでたよ");
                }
            }

            if (log.Message.Equals(BossGoldamofMessage))
            {
                GolmdoafTime = log.TimeStamp;
                if (IsRealTimeLogMode && IsBossAnnounce)
                {
                    Announce("ごるもだふ がでたよ");
                }
            }

            if (IsRealTimeLogMode)
            {
                UpdateBossView();
            }
        }

        public void Speak(string message)
        {
            // ロックしておかないと、nullチェックが通った後、disposeとspeakがかぶりそうな気がする。
            lock (this)
            {
                if (VoiceAdapter == null)
                {
                    return;
                }

                try
                {
                    VoiceAdapter.Speak(message);
                }
                catch (Exception ex)
                {
                    Logger.Instance.PutMessage($"棒読みちゃんがしゃべれませんでした: ${ex.Message}");
                    Window.Dispatcher.Invoke((Action)(() =>
                    {
                        Window.lblBouyomiState.Content = "棒読みちゃん連携に失敗しました。\n棒読みちゃんが起動しているか確認してください。";
                    }));
                }
            }
        }

        public void Announce(string message)
        {
            // ロックしておかないと、nullチェックが通った後、disposeとspeakがかぶりそうな気がする。
            lock (this)
            {
                if (VoiceAdapter == null)
                {
                    return;
                }

                try
                {
                    VoiceAdapter.Announce(message);
                }
                catch (Exception ex)
                {
                    Logger.Instance.PutMessage($"棒読みちゃんがしゃべれませんでした: ${ex.Message}");
                    Window.Dispatcher.Invoke((Action)(() =>
                    {
                        Window.lblBouyomiState.Content = "棒読みちゃん連携に失敗しました。\n棒読みちゃんが起動しているか確認してください。";
                    }));
                }
            }
        }
    }
}
