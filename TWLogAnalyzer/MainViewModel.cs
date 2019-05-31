using System.Collections.ObjectModel;
using TWLogAnalyzer.tw;

namespace TWLogAnalyzer
{
    class MainViewModel
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

        public void Init()
        {
            ClubChats.Clear();
            TeamChats.Clear();
            WisperChats.Clear();
        }
    }
}
