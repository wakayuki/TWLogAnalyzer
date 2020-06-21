using System.Collections.ObjectModel;
using System.ComponentModel;
using TWLogAnalyzer.tw;

namespace TWLogAnalyzer
{
    class MainViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// クラブチャット表示用
        /// </summary>
        public ObservableCollection<TwChatLog> ClubChats { set; get; } = new ObservableCollection<TwChatLog>();

        /// <summary>
        /// チームチャット表示用
        /// </summary>
        public ObservableCollection<TwChatLog> TeamChats { set; get; } = new ObservableCollection<TwChatLog>();

        /// <summary>
        /// 耳打ち表示用
        /// </summary>
        public ObservableCollection<TwChatLog> WisperChats { set; get; } = new ObservableCollection<TwChatLog>();

        /// <summary>
        /// ステータスバー表示用
        /// </summary>
        private string _status = "";
        public string Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                OnPropertyChanged("Status");
            }
        }


        // ステータスバーに表示する情報
        private string _watchStatus = "";
        private string _byouyomiState = "";

        public void Init()
        {
            ClubChats.Clear();
            TeamChats.Clear();
            WisperChats.Clear();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void updateWatchStatus(string watchStatus)
        {
            this._watchStatus = watchStatus;
            this.Status = $"{_watchStatus}, ${_byouyomiState}";
        }

        public void updateBouyomiState(string byouyomiState)
        {
            this._byouyomiState = byouyomiState;
            this.Status = $"{_watchStatus}, {_byouyomiState}";
        }
    }
}
