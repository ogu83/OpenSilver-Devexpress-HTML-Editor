using DevExpress;
using System.Windows;

namespace OpenSilver_Devexpress_HTMLEditor
{
    public sealed partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Startup += App_Startup;
        }

        private async void App_Startup(object sender, StartupEventArgs e)
        {
            await HtmlEdit.Initialize();
            var mainPage = new MainPage();
            Window.Current.Content = mainPage;
        }
    }
}