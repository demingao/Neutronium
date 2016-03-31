﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Awesomium.Core;
using Awesomium.Windows.Controls;
using HTML_WPF.Component;
using MVVM.HTML.Core.Infra;
using MVVM.HTML.Core.JavascriptEngine.Window;

namespace HTMLEngine.Awesomium
{
    internal class AwesomiumWPFWebWindow : IWPFWebWindow
    {
        private readonly WebSession _Session;
        private readonly WebConfig _WebConfig;
        private readonly WebControl _WebControl;
        private readonly AwesomiumHTMLWindow _AwesomiumHTMLWindow;

        public AwesomiumWPFWebWindow(WebSession iSession, WebConfig webConfig)
        {
            _Session = iSession;
            _WebConfig = webConfig;

            _WebControl = new WebControl()
            {
                WebSession = _Session,
                Visibility = Visibility.Hidden,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                ContextMenu = new ContextMenu() { Visibility = Visibility.Collapsed }
            };

            _AwesomiumHTMLWindow = new AwesomiumHTMLWindow(_WebControl);     
        }

        public IHTMLWindow HTMLWindow
        {
            get { return _AwesomiumHTMLWindow; }
        }
        public UIElement UIElement
        {
            get { return _WebControl; }
        }

        public void Inject(Key KeyToInject)
        {
            IWebView wv = _WebControl;
            KeyEventArgs kev = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, KeyToInject);
            wv.InjectKeyboardEvent(kev.GetKeyboardEvent(WebKeyboardEventType.KeyDown));
        }

        public bool IsUIElementAlwaysTopMost
        {
            get { return false; }
        }


        public bool OnDebugToolsRequest() 
        {
            var port = _WebConfig.RemoteDebuggingPort;
            if (port == 0)
                return false;

            ProcessHelper.OpenLocalUrlInBrowser(port);
            return true;
        }

        public void CloseDebugTools() 
        {
        }

        public void Dispose()
        {
            _WebControl.Dispose();
            _AwesomiumHTMLWindow.Dispose();
        }
    }
}
