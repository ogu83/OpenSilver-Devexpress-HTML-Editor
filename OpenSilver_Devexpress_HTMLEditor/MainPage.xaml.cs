using CSHTML5.Native.Html.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OpenSilver_Devexpress_HTMLEditor
{
    public partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Editor_TabItem_Loaded(object sender, RoutedEventArgs e)
        {
            htmlEditor.SetHeight(590);
        }
    }
}
