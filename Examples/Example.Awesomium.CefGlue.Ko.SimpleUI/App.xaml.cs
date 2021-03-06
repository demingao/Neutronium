﻿using System.Windows;
using HTMLEngine.Awesomium;
using HTMLEngine.CefGlue;
using Neutronium.JavascriptFramework.Knockout;
using Neutronium.WebBrowserEngine.Awesomium;
using Neutronium.WebBrowserEngine.CefGlue;
using Neutronium.WPF;

namespace Example.Awesomium.CefGlue.Ko.SimpleUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var engine = HTMLEngineFactory.Engine;
            engine.RegisterHTMLEngine(new AwesomiumWPFWebWindowFactory() );
            engine.RegisterHTMLEngine(new CefGlueWPFWebWindowFactory());
            engine.RegisterJavaScriptFramework(new KnockoutFrameworkManager());
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            HTMLEngineFactory.Engine.Dispose();
            base.OnExit(e);
        }
    }
}
