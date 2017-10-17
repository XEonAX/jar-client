using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using VB = Microsoft.VisualBasic.ApplicationServices;
using Forms = System.Windows.Forms;
using System.Windows.Interop;

namespace JAR.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Syncer _Syncer;
        private bool _ProtocolHandlerInstalled;
        private ClipBoardMonitor clipboardMonitor;

        /// <summary>
        /// This will be called for the first launch instance
        /// In this will initialize the MainWindow, Syncer, WsClient and Ticker
        /// We will also show the TrayIcon
        /// </summary>
        /// <param name="startupEventArgs"></param>
        protected override void OnStartup(StartupEventArgs startupEventArgs)
        {
            Console.WriteLine("App:Starting...");
            base.OnStartup(startupEventArgs);

            Console.WriteLine("App:CreateMainWindow");
            MainWindow = new MainWindow();
            _Syncer = new Syncer(MainWindow as MainWindow);
            //MainWindow.Show();

            Console.WriteLine("APP:CreateTrayIcon");
            Forms.NotifyIcon trayIcon = new Forms.NotifyIcon();

            trayIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Forms.Application.ExecutablePath);
            trayIcon.Visible = true;
            trayIcon.Text = Constants.Title;
            trayIcon.ShowBalloonTip(
                tipTitle: Constants.Title,
                tipText: "Starting Up...",
                tipIcon: Forms.ToolTipIcon.Info,
                timeout: 100
              );

            trayIcon.ContextMenuStrip = new Forms.ContextMenuStrip();

            var mniUninstall = new Forms.ToolStripMenuItem("&Uninstall Protocol Handler");
            mniUninstall.Click += (_, __) =>
            {
                try
                {
                    Installer.UnregisterThisApplication(Constants.Protocol);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error occured while uninstalling." + ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
            trayIcon.ContextMenuStrip.Items.Add(mniUninstall);

            var mniClipboard = new Forms.ToolStripMenuItem("Monitor Clipboard");
            mniClipboard.CheckOnClick = true;
            mniClipboard.CheckedChanged += (sender, e) =>
            {
                if (clipboardMonitor == null)
                {
                    clipboardMonitor = new ClipBoardMonitor();
                    clipboardMonitor.ClipboardChanged += ClipboardMonitor_ClipboardChanged;
                }

                if (mniClipboard.Checked)
                    clipboardMonitor.Enable(MainWindow as MainWindow);
                else
                    clipboardMonitor.Disable();
            };
            trayIcon.ContextMenuStrip.Items.Add(mniClipboard);

            var mniExit = new Forms.ToolStripMenuItem("E&xit");
            mniExit.Click += (_, __) => { trayIcon.Visible = false; Current.Shutdown(); };
            trayIcon.ContextMenuStrip.Items.Add(mniExit);

            _Syncer.ntfyIcon = trayIcon;
            try
            {
                Installer.RegisterThisApplication(Constants.Protocol, Constants.Title);
                Console.WriteLine("APP:RegisterProtocolHandler:" + Constants.Protocol + ":Success");
                _ProtocolHandlerInstalled = true;
                trayIcon.ShowBalloonTip(
                    tipTitle: Constants.Title,
                    tipText: "Protocol Handler  for '" + Constants.Protocol + "' installed.",
                    tipIcon: Forms.ToolTipIcon.Info,
                    timeout: 100
                  );
            }
            catch (Exception ex)
            {
                _ProtocolHandlerInstalled = false;
                Console.WriteLine("APP:RegisterProtocolHandler:" + Constants.Protocol + ":Failure");
                trayIcon.ShowBalloonTip(
                     tipTitle: Constants.Title,
                     tipText: "Protocol Handler for '" + Constants.Protocol + "' could not be installed.",
                     tipIcon: Forms.ToolTipIcon.Warning,
                     timeout: 100
                   );
                mniClipboard.Checked = true;
            }

            Console.WriteLine("APP:FirstSync:" + string.Join(",", startupEventArgs.Args));
            _Syncer.Sync(startupEventArgs.Args);
        }

        private void ClipboardMonitor_ClipboardChanged(object sender, EventArgs e)
        {
            string clipdata;
            try
            {
                if (Clipboard.ContainsText())
                {
                    clipdata = Clipboard.GetText(TextDataFormat.UnicodeText);
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("App:ClipChanged:Error:" + ex.ToString());
                return;
            }

            if (clipdata.StartsWith(Constants.Protocol))
            {
                Console.WriteLine("App:ClipChanged:StartsWithProto:" + clipdata);
                _Syncer.Sync(new[] { clipdata });
                Clipboard.Clear();
            }
            else
            {
                var len = clipdata.Length;
                clipdata = string.Empty;
                if (len > 4096)
                    GC.Collect();
            }
        }

        internal void OnNextStartup(VB.StartupNextInstanceEventArgs nextInstanceEventArgs)
        {
            Console.WriteLine("App:NextSync:" + string.Join(",", nextInstanceEventArgs.CommandLine));
            _Syncer.Sync(nextInstanceEventArgs.CommandLine.ToArray());
        }
    }
}
