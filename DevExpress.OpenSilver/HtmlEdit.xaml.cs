using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Input;

namespace DevExpress
{
    /// <summary>
    /// Documentation : https://demos.devexpress.com/ASPNetCore/Demo/HtmlEditor/Overview/
    /// </summary>
    public partial class HtmlEdit : UserControl
    {
        public class HtmlEditorEventArgs : EventArgs
        {
            public HtmlEditorEventArgs()
            {

            }

            public HtmlEditorEventArgs(string value)
            {
                Value = value;
            }

            public readonly string Value;
        }

        private static bool _initialized;
        private string _html = string.Empty;
        private bool _isReadOnly;
        private string _thisDivId;
        private bool _documentLoaded;
        private string _lastValue;
        private readonly DelayedExecutor _delayedExecutor;

        private static readonly HashSet<Key> HandleKeys = new HashSet<Key>
        {
            Key.Tab,
            Key.End,
            Key.Home,
            Key.Left,
            Key.Right,
            Key.Up,
            Key.Down,
        };

        private static readonly HashSet<Key> HandleAndCancelKeys = new HashSet<Key>
        {
            Key.PageUp,
            Key.PageDown,
        };

        public static async Task Initialize()
        {
            if (_initialized)
                return;

            const string path = "ms-appx:///DevExpress/js/";

            await OpenSilver.Interop.LoadCssFile($"{path}dx.light.css");
            await OpenSilver.Interop.LoadJavaScriptFile($"{path}jquery-3.5.1.min.js");
            await OpenSilver.Interop.LoadJavaScriptFile($"{path}dx-quill.min.js");
            await OpenSilver.Interop.LoadJavaScriptFile($"{path}dx.all.js");
            await OpenSilver.Interop.LoadJavaScriptFile($"{path}htmlEdit-creator.js");

            _initialized = true;
#if DEBUG
            OpenSilver.Interop.ExecuteJavaScript("console.log('HtmlEdit Javascript files initialized')");
#endif
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs eventArgs)
        {
            base.OnMouseLeftButtonDown(eventArgs);

            if (eventArgs.Handled)
                return;

            eventArgs.Handled = true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Handled) return;

            if (HandleKeys.Contains(e.Key))
            {
                e.Cancellable = false;
                e.Handled = true;
                return;
            }

            if (HandleAndCancelKeys.Contains(e.Key))
            {
                e.Handled = true;
            }
        }

        public HtmlEdit()
        {
            Unloaded += HtmlEdit_Unloaded;
            IsVisibleChanged += OnIsVisibleChanged;
            SizeChanged += OnSizeChanged;

            InitializeComponent();

            _delayedExecutor = new DelayedExecutor(() =>
                IsLoaded && IsVisible && _documentLoaded && !string.IsNullOrEmpty(_thisDivId));
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            htmlEditHost.Width = e.NewSize.Width;
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _delayedExecutor.ExecuteWaitingActions();
        }

        private void HtmlEdit_Unloaded(object sender, RoutedEventArgs e)
        {
            OpenSilver.Interop.ExecuteJavaScript("jsHtmlEditClient.dispose($0);", _thisDivId);
            _thisDivId = null;
            _isReadOnly = false;
            _documentLoaded = false;
        }

        public void ClearHistory()
        {
            _delayedExecutor.ExecuteWhenReady(() =>
            {
                OpenSilver.Interop.ExecuteJavaScript("jsHtmlEditClient.clearHistory($0);", _thisDivId);
            });
        }

        #region Events

        public event EventHandler<HtmlEditorEventArgs> TextChanged;
        public event EventHandler<HtmlEditorEventArgs> HtmlTextChanged;
        public event EventHandler OnInitialized;

        [ScriptableMember(ScriptAlias = "onValueChanged")]
        public void ValueChanged(string nextValue)
        {
            if (nextValue == _lastValue)
                return;

            _lastValue = nextValue;

            TextChanged?.Invoke(this, new HtmlEditorEventArgs(_lastValue));
            HtmlTextChanged?.Invoke(this, new HtmlEditorEventArgs(_lastValue));
        }

        [ScriptableMember(ScriptAlias = "onInitialized")]
        public void Initialized()
        {
            _documentLoaded = true;
            _delayedExecutor.ExecuteWaitingActions();
            OnInitialized?.Invoke(this, null);
            if (!string.IsNullOrEmpty(_lastValue))
            {
                SetValue(_lastValue);
            }
        }

        #endregion

        #region Properties

        public static readonly DependencyProperty HtmlProperty = DependencyProperty.Register(nameof(Html), typeof(string), typeof(UserControl), new PropertyMetadata(null, OnHtmlChanged));

        private static void OnHtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = d as HtmlEdit;
            obj?.ValueChanged(e.NewValue.ToString());
        }

        /// <summary>
        /// The Document inside control, value of DxHtmlEditor instance
        /// </summary>
        public string Html
        {
            get =>
                !_delayedExecutor.IsReady()
                    ? _html
                    : Convert.ToString(
                        OpenSilver.Interop.ExecuteJavaScript("jsHtmlEditClient.getValue($0);", _thisDivId));
            set => SetValue(Regex.Replace(value, @"\t|\n|\r", "").Replace('\'', '`'));
        }

        /// <summary>
        /// Same Property for Html
        /// The Document inside control, value of DxHtmlEditor instance
        /// </summary>
        public string HtmlText
        {
            get => Html;
            set => Html = value;
        }

        public bool ReadOnly
        {

            get => !_delayedExecutor.IsReady()
                ? _isReadOnly
                : Convert.ToBoolean(
                    OpenSilver.Interop.ExecuteJavaScript("jsHtmlEditClient.getReadOnly($0);", _thisDivId));
            set
            {
                _isReadOnly = value;
                _delayedExecutor.ExecuteWhenReady(() =>
                {
                    var val = _isReadOnly ? "true" : "false";
                    OpenSilver.Interop.ExecuteJavaScript($"jsHtmlEditClient.setReadOnly({val}, '{_thisDivId}');");
                });
            }
        }

        #endregion

        #region Methods

        public void SetHeight(double value)
        {
            _delayedExecutor.ExecuteWhenReady(() =>
            {
                OpenSilver.Interop.ExecuteJavaScript($"jsHtmlEditClient.setHeight('{(int)value}px', '{_thisDivId}');");
            });
        }

        public void SetValue(string value)
        {
            SetValue(HtmlProperty, value);
            _html = value;
            _delayedExecutor.ExecuteWhenReady(() =>
            {
                OpenSilver.Interop.ExecuteJavaScript($"jsHtmlEditClient.setValue('{_html}', '{_thisDivId}');");
            });
        }

        #endregion

        private void HtmlEditHost_OnInitialized(object sender, HtmlEditHost.HtmlEditHostInitializedEventArgs args)
        {
            _thisDivId = OpenSilver.Interop.ExecuteJavaScript("$0.parentElement.id", args.DomElement).ToString();
            OpenSilver.Interop.ExecuteJavaScript("jsHtmlEditClient.createHtmlEdit($0, $1, $2)",
                (Action)Initialized,
                (Action<string>)ValueChanged,
                _thisDivId);
        }
    }
}