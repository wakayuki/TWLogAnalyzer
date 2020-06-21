using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using TWLogAnalyzer.common;
using TWLogAnalyzer.tw;
using TWLogAnalyzer.voice.bouyomi;

namespace TWLogAnalyzer
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private static TwLogAnalyzerSettings LoadSettings()
        {
            // BaseDirectoryは\をつけたりつけなかったりするらしいので無条件でtrimして付け直す
            string fileName = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + '\\' + ConfigurationManager.AppSettings["setting.file"];

            if (!File.Exists(fileName))
            {
                // ファイルがない場合は初回起動なので新品の設定を返す
                return new TwLogAnalyzerSettings();
            }

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(TwLogAnalyzerSettings));
            try
            {
                using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    return (TwLogAnalyzerSettings)serializer.ReadObject(stream);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.PutMessage($"設定ファイルの読み込みに失敗: {ex.Message}");
                MessageBox.Show($"{fileName}を読み込みませんでした。原因:アクセス権がないか、ファイルが破損しています。", "設定の読み込みに失敗しました");
                return new TwLogAnalyzerSettings();
            }
        }

        private static void SaveSettings(TwLogAnalyzerSettings settings)
        {
            // BaseDirectoryは\をつけたりつけなかったりするらしいので無条件でtrimして付け直す
            string fileName = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + '\\' + ConfigurationManager.AppSettings["setting.file"];

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(TwLogAnalyzerSettings));
            try
            {
                using (FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    serializer.WriteObject(stream, settings);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.PutMessage($"設定の書き込みに失敗: {ex.Message}");
                MessageBox.Show($"{fileName}に設定を保存できませんでした。", "設定の保存に失敗しました");
            }
        }


        private TwManager Manager;
        private TwLogAnalyzerSettings Settings;
        private MainWindowReceiver Receiver;

        // 棒読みちゃん連携に失敗したときに自動的にチェックボックスをアンチェックしたいが
        // ユーザがアンチェックしたときと処理を分けたいので、そのためのフラグ
        private bool uncheckBouyomiRenkeiFromProgram = false;

        // 次のボスアナウンス時間
        private DateTime NextGolronTime = DateTime.MaxValue;
        private DateTime NextGolmodafTime = DateTime.MaxValue;
        MainViewModel Model { get; }

        public MainWindow()
        {
            InitializeComponent();

            Model = new MainViewModel();
            this.DataContext = Model;

            // しばらくオプション増える予定ないし、コマンドラインの解析はさぼる。。。
            bool isLogging = false;
            foreach (string line in App.CommandLineArgs)
            {
                if (line.Equals("debug"))
                {
                    isLogging = true;
                }
            }

            Logger.init(Directory.GetParent(Assembly.GetEntryAssembly().Location) + @"\debug.log", isLogging);

            try
            {
                Receiver = new MainWindowReceiver(this);
                Manager = new TwManager(Receiver);
            }
            catch (TwException ex)
            {
                Logger.Instance.PutMessage(ex.Message);
                MessageBox.Show(ex.Message, "起動できません", MessageBoxButton.OK);
                Application.Current.Shutdown();
            }

            // コントロールの初期値を設定
            chkBossAnnounce.IsEnabled = false;
            btnEnd.IsEnabled = false;

            // コントロールの値を設定ファイルから読み込んだ内容に変更
            Settings = LoadSettings();
            txtBouyomiPath.Text = Settings.BousyomiChanSettings.exeFilePath;
            chkBouyomi.IsChecked = Settings.BousyomiChanSettings.IsEnabled;
            chkBossAnnounce.IsChecked = Settings.BousyomiChanSettings.IsBossAnnounceEnabled;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        internal void InitChatLog()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                Model.Init();
            }));
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!Manager.IsWatching)
            {
                return;
            }

            // 経験値のアップデート
            lblExpValue.Content = String.Format("{0:#,0} / {1:#,0}",
                Receiver.GetExpPerMin(5) * 60,
                Receiver.GetExpPerMin(10) * 60);
            lblRuneExpValue.Content = String.Format("{0:#,0} / {1:#,0}",
                Receiver.GetRuneExpPerMin(5) * 60,
                Receiver.GetRuneExpPerMin(10) * 60);

            // ボスアナウンスのチェック
            if (chkBossAnnounce.IsChecked == true)
            {
                if (NextGolronTime < DateTime.Now)
                {
                    NextGolronTime = DateTime.MaxValue;
                    Receiver.Speak("そろそろ ごるろん が沸く時間だよ");
                }

                if (NextGolmodafTime < DateTime.Now)
                {
                    NextGolmodafTime = DateTime.MaxValue;
                    Receiver.Speak("そろそろ ごるもだふ が沸く時間だよ");
                }
            }
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            btnStart.IsEnabled = false;
            Model.updateWatchStatus("監視初期化中");
            Task.Run(() =>
            {
                Manager.WatchStart();
                Dispatcher.Invoke((Action)(() =>
                {
                    btnEnd.IsEnabled = true;
                    Model.updateWatchStatus("監視中");
                }));
            });
        }

        private void BtnEnd_Click(object sender, RoutedEventArgs e)
        {
            btnEnd.IsEnabled = false;
            Model.updateWatchStatus("監視停止処理中");
            Task.Run(() =>
            {
                Manager.WatchStop();
                Dispatcher.Invoke((Action)(() =>
                {
                    btnStart.IsEnabled = true;
                    Model.updateWatchStatus("");
                }));
            });
        }

        private void ChkBouyomi_Checked(object sender, RoutedEventArgs e)
        {
            // 棒読みちゃんクライアントの初期化中のアンチェックを防ぐため、UIから設定不可能にする
            chkBouyomi.IsEnabled = false;

            // 設定ONの時に変更不可・ONじゃないと変更不可のコントロール設定
            txtBouyomiPath.IsEnabled = false;
            chkBossAnnounce.IsEnabled = true;

            Settings.BousyomiChanSettings.exeFilePath = this.txtBouyomiPath.Text;
            Settings.BousyomiChanSettings.IsEnabled = true;

            Task.Run(() =>
            {
                try
                {
                    UpdateBouyomiChanState("棒読みちゃんの初期化中です");
                    Receiver.EnableVoiceAdapter(new BouyomiAdapter(Settings.BousyomiChanSettings.exeFilePath));
                    UpdateBouyomiChanState("棒読みちゃん連携中");
                }
                catch (Exception ex)
                {
                    Logger.Instance.PutMessage($"棒読みちゃん初期化エラー： {ex.Message}");
                    UpdateBouyomiChanState("棒読みちゃんと接続できませんでした");
                    uncheckBouyomiRenkeiFromProgram = true;
                    Dispatcher.Invoke((Action)(() =>
                    {
                        chkBouyomi.IsChecked = false;
                    }));
                }

                // 初期化が終わったのでUIで触れるようにしておく
                Dispatcher.Invoke((Action)(() =>
                {
                    chkBouyomi.IsEnabled = true;
                }));
            });
        }

        internal void Notify(string message)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                Model.updateBouyomiState(message);
            }));
        }

        private void ChkBouyomi_Unchecked(object sender, RoutedEventArgs e)
        {
            // 棒読みちゃん終了中にチェックされるのを防ぐためUIから設定不可能にする
            chkBouyomi.IsEnabled = false;

            // 設定ONの時に変更不可・ONじゃないと変更不可のコントロール設定
            txtBouyomiPath.IsEnabled = true;
            chkBossAnnounce.IsEnabled = false;

            Settings.BousyomiChanSettings.IsEnabled = false;

            try
            {
                Receiver.DisableVoiceAdapter();
            }
            catch (Exception ex)
            {
                Logger.Instance.PutMessage($"棒読みちゃん終了エラー: {ex.Message}");
            }


            // 連携失敗時の自動アンチェックの場合はラベル書き換えをせずに終了する
            if (uncheckBouyomiRenkeiFromProgram)
            {
                uncheckBouyomiRenkeiFromProgram = false;
            }
            else
            {
                UpdateBouyomiChanState("");
            }

            chkBouyomi.IsEnabled = true;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (Receiver != null) Receiver.DisableVoiceAdapter();

            SaveSettings(Settings);
        }

        private void UpdateBouyomiChanState(string message)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                Model.updateBouyomiState(message);
            }));
        }

        private void ChkBossAnnounce_Checked(object sender, RoutedEventArgs e)
        {
            Settings.BousyomiChanSettings.IsBossAnnounceEnabled = true;
            Receiver.IsBossAnnounce = true;
        }

        private void ChkBossAnnounce_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.BousyomiChanSettings.IsBossAnnounceEnabled = false;
            Receiver.IsBossAnnounce = false;
        }

        public void UpdateNextGolronTime(DateTime time)
        {
            NextGolronTime = time.AddMinutes(-3);
        }

        public void UpdateNextGolmodafTime(DateTime time)
        {
            NextGolmodafTime = time.AddMinutes(-3);
        }

        public void AddClubChat(TwChatLog log)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                Model.ClubChats.Add(log);
                ScrollToLast(this.clubChat);
            }));
        }

        public void AddTeamChat(TwChatLog log)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                Model.TeamChats.Add(log);
                ScrollToLast(this.teamChat);
            }));
        }
        public void AddWisperChat(TwChatLog log)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                Model.WisperChats.Add(log);
                ScrollToLast(this.wisperChat);
            }));
        }

        private void ScrollToLast(DataGrid grid)
        {
            if (grid.Items.Count > 0 && VisualTreeHelper.GetChildrenCount(grid) != 0)
            {
                Decorator border = VisualTreeHelper.GetChild(grid, 0) as Decorator;
                if (border != null)
                {
                    ScrollViewer scroll = border.Child as ScrollViewer;
                    if (scroll != null) scroll.ScrollToEnd();
                }
            }
        }

        private void BtnOpenLog_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("EXPLORER.EXE", $"{Manager.InstDir}\\ChatLog");
        }
    }
}
