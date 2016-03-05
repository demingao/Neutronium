﻿using System;
using System.Windows.Controls;
using System.Threading;
using System.Reflection;

using Xunit;
using FluentAssertions;

using MVVM.HTML.Core.Infra;
using MVVM.ViewModel.Example;
using MVVM.HTML.Core.Exceptions;
using System.IO;
using MVVM.HTML.Core;
using HTML_WPF.Component;
using System.Threading.Tasks;
using MVVM.HTML.Core.Navigation;
using IntegratedTest.WPF.Infra;

namespace IntegratedTest.WPF
{
    public abstract class Test_HTMLViewControl
    {
        protected abstract WindowTestEnvironment GetEnvironment();

        private WindowTest BuildWindow(Func<HTMLViewControl> iWebControlFac)
        {
            return new WindowTest(
                (w) =>
                {
                    StackPanel stackPanel = new StackPanel();
                    w.Content = stackPanel;
                    var iWebControl = iWebControlFac();
                    w.RegisterName(iWebControl.Name, iWebControl);
                    w.Closed += (o, e) => { iWebControl.Dispose(); };
                    stackPanel.Children.Add(iWebControl);
                } );
        }

        internal void Test(Action<HTMLViewControl, WindowTest> Test, bool iDebug = false, 
                            WindowTestEnvironment environment=null)
        {
            environment = environment ?? GetEnvironment();
            

            AssemblyHelper.SetEntryAssembly();
            HTMLViewControl wc1 = null;
            Func<HTMLViewControl> iWebControlFac = () =>
            {
                environment.Register();
                wc1 = new HTMLViewControl();
                wc1.IsDebug = iDebug;
                return wc1;
            };

            using (var wcontext = BuildWindow(iWebControlFac))
            {
                Test(wc1, wcontext);
            }
        }

        [Fact]
        public void Basic_Option()
        {
            Test((c, w) =>
                {
                    var mre = new ManualResetEvent(false);

                    w.RunOnUIThread(() =>
                        {
                            c.SessionPath.Should().BeNull();
                            c.Mode.Should().Be(JavascriptBindingMode.TwoWay);
                            c.Uri.Should().BeNull();
                            mre.Set();
                        });

                    mre.WaitOne();
                });
        }

        [Fact]
        public void Basic_Option_Find_Path()
        {
            Test((c, w) =>
            {
                var mre = new ManualResetEvent(false);

                w.RunOnUIThread(() =>
                {
                    c.SessionPath.Should().BeNull();
                    c.Mode.Should().Be(JavascriptBindingMode.TwoWay);
                    c.Uri.Should().BeNull();

                    string relp = "javascript\\navigation_1.html";
                    Action act = () => c.RelativeSource = relp;
                    act.ShouldThrow<MVVMCEFGlueException>();
                    mre.Set();
                });

                mre.WaitOne();
            });
        }

        [Fact]
        public void Basic_RelativeSource()
        {
            Test((c, w) =>
            {
                var mre = new ManualResetEvent(false);

                w.RunOnUIThread(() =>
                {
                    c.SessionPath.Should().BeNull();
                    c.Mode.Should().Be(JavascriptBindingMode.TwoWay);
                    c.Uri.Should().BeNull();

                    string relp = "javascript\\navigation_1.html";

                    string path = string.Format("{0}\\{1}", typeof(HTMLViewControl).Assembly.GetPath(), relp);
                    string nd = Path.GetDirectoryName(path);
                    Directory.CreateDirectory(nd);

                    File.Copy("javascript\\navigation_1.html", path);

                    c.RelativeSource = relp;
                   
                    File.Delete(path);
                    mre.Set();
                });

                mre.WaitOne();
            });
        }

        [Fact]
        public void Basic_Option_Simple()
        {
            Test((c, w) =>
            {
                var tcs = new TaskCompletionSource<DisplayEvent>();

                EventHandler<DisplayEvent> ea = null;
                ea = (o, e) => { tcs.TrySetResult(e); c.OnDisplay -= ea; };
                c.OnDisplay += ea;
                var dc = new Person();

                w.RunOnUIThread(() =>
                {
                    c.Mode = JavascriptBindingMode.OneWay;
                    string relp = "javascript\\navigation_1.html";
                    c.Uri = new Uri(string.Format("{0}\\{1}", Assembly.GetAssembly(typeof(Test_HTMLViewControl)).GetPath(), relp));
                    w.Window.DataContext = dc;
                });

                var de = tcs.Task.Result;
                de.Should().NotBeNull();
                de.DisplayedViewModel.Should().Be(dc);
            });
        }

        [Fact]
        public void Basic_Option_Simple_UsingRelativePath()
        {
            Test((c, w) =>
            {
                var finalmre = new ManualResetEvent(false);

                DisplayEvent de = null;
                EventHandler<DisplayEvent> ea = null;
                ea = (o, e) => { de = e; c.OnDisplay -= ea; finalmre.Set(); };
                c.OnDisplay += ea;
                var dc = new Person();

                string relp = "javascript\\navigation_1.html";
                string path = string.Format("{0}\\{1}", typeof(HTMLViewControl).Assembly.GetPath(), relp);
                var jvs = PrepareFiles();

                w.RunOnUIThread(() =>
                {
                    c.Mode = JavascriptBindingMode.OneWay;
                    c.RelativeSource = relp;
                    w.Window.DataContext = dc;
                });

                finalmre.WaitOne();
                foreach (string jv in jvs)
                {
                    string p = string.Format("{0}\\javascript\\src\\{1}", typeof(HTMLViewControl).Assembly.GetPath(), jv);
                    File.Delete(p);
                }
                File.Delete(path);
                de.Should().NotBeNull();
                de.DisplayedViewModel.Should().Be(dc);
            });
        }

        private string[] PrepareFiles()
        {
            string relp = "javascript\\navigation_1.html";
            string path = string.Format("{0}\\{1}", typeof(HTMLViewControl).Assembly.GetPath(), relp);
            string nd = Path.GetDirectoryName(path);
            Directory.CreateDirectory(nd);

            if (!File.Exists(path))
                File.Copy("javascript\\navigation_1.html", path);

            string[] jvs = new string[] { "Ko_register.js", "Ko_Extension.js", "knockout.js" };

            string src = string.Format("{0}\\javascript\\src", typeof(HTMLViewControl).Assembly.GetPath());
            Directory.CreateDirectory(src);

            foreach (string jv in jvs)
            {
                string p = string.Format("{0}\\javascript\\src\\{1}", typeof(HTMLViewControl).Assembly.GetPath(), jv);
                if (!File.Exists(p))
                    File.Copy(string.Format("javascript\\src\\{0}", jv), p);
            }

            return jvs;
        }

        [Fact]
        public void Basic_Option_Simple_UsingRelativePath_AfterDataContext()
        {
            Test((c, w) =>
            {
                var mre = new ManualResetEvent(false);

                DisplayEvent de = null;
                EventHandler<DisplayEvent> ea = null;
                ea = (o, e) => { de = e; c.OnDisplay -= ea;   mre.Set();};
                c.OnDisplay += ea;
                var dc = new Person();

                string relp = "javascript\\navigation_1.html";
                string path = string.Format("{0}\\{1}", typeof(HTMLViewControl).Assembly.GetPath(), relp);
                var jvs = PrepareFiles();

                w.RunOnUIThread(() =>
                {
                    c.Mode = JavascriptBindingMode.OneWay; 
                    w.Window.DataContext = dc;
                    c.RelativeSource = relp;
                  
                });

                mre.WaitOne();

                foreach (string jv in jvs)
                {
                    string p = string.Format("{0}\\javascript\\src\\{1}", typeof(HTMLViewControl).Assembly.GetPath(), jv);
                    File.Delete(p);
                }
                File.Delete(path);
                de.Should().NotBeNull();
                de.DisplayedViewModel.Should().Be(dc);
            });
        }
    }
}