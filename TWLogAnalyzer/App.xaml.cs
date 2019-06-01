using System.Windows;

namespace TWLogAnalyzer
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        public static string[] CommandLineArgs { get; private set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            CommandLineArgs = e.Args;
        }
    }
}
