﻿using System.Windows;
using MVVM.HTML.Core.JavascriptEngine.Control;
using MVVM.HTML.Core.JavascriptEngine.Window;

namespace HTML_WPF.Component
{
    public class WPFHTMLWindowProvider : IHTMLWindowProvider
    {
        private readonly UIElement _UIElement;
        private readonly HTMLControlBase _HTMLControlBase;
        private readonly IWPFWebWindow _IWPFWebWindow;

        public WPFHTMLWindowProvider(IWPFWebWindow iIWPFWebWindow, HTMLControlBase iHTMLControlBase)
        {
            _IWPFWebWindow = iIWPFWebWindow;
            _HTMLControlBase = iHTMLControlBase;
            _UIElement = _IWPFWebWindow.UIElement;
        }

        public IHTMLWindow HTMLWindow
        {
            get { return _IWPFWebWindow.HTMLWindow; }
        }

        public IWPFWebWindow IWPFWebWindow
        {
            get { return _IWPFWebWindow; }
        }

        public IDispatcher UIDispatcher
        {
            get { return new WPFUIDispatcher(_UIElement.Dispatcher); }
        }

        public void Show()
        {
            _UIElement.Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            _UIElement.Visibility = Visibility.Hidden;
        }

        public bool OnDebugToolsRequest() 
        {
            return _IWPFWebWindow.OnDebugToolsRequest();
        }

        public void CloseDebugTools() 
        {
            _IWPFWebWindow.CloseDebugTools();
        }

        public void Dispose()
        {
            _UIElement.Visibility = Visibility.Hidden;
            _HTMLControlBase.MainGrid.Children.Remove(_UIElement);

            _IWPFWebWindow.Dispose();
        }
    }
}
