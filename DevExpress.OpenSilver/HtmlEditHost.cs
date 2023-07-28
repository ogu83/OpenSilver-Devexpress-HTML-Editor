using System;
using System.Windows;
using CSHTML5.Native.Html.Controls;

namespace DevExpress
{
    public class HtmlEditHost : HtmlPresenter
    {
        public class HtmlEditHostInitializedEventArgs : EventArgs
        {

            public readonly object DomElement;

            public HtmlEditHostInitializedEventArgs(object domElement)
            {
                DomElement = domElement;
            }
        }

        public HtmlEditHost()
        {
            Html = "<div></div>";

            Loaded += OnLoaded;
        }

        public event EventHandler<HtmlEditHostInitializedEventArgs> OnInitialized;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
                return;

            OnInitialized?.Invoke(this, new HtmlEditHostInitializedEventArgs(DomElement));
        }
    }
}
