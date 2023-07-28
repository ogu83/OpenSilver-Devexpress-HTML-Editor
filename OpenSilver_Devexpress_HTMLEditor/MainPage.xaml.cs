using System.Windows;
using System.Windows.Controls;

namespace OpenSilver_Devexpress_HTMLEditor
{
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void Editor_TabItem_Loaded(object sender, RoutedEventArgs e)
        {
            htmlEditor.SetHeight(500);
            tabControl.SelectionChanged += TabControl_SelectionChanged;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            htmlEditor.SetHeight(500);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            htmlPresenter.Html = htmlEditor.Html;
        }
    }
}
