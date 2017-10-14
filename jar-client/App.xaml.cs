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

namespace JAR.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Syncer _Syncer;
        private bool _ProtocolHandlerInstalled;

        protected override void OnStartup(StartupEventArgs startupEventArgs)
        {
            Console.WriteLine("App:Starting...");
            base.OnStartup(startupEventArgs);

            Console.WriteLine("App:CreateMainWindow");
            MainWindow = new MainWindow();
            _Syncer = new Syncer(MainWindow as MainWindow);


            Console.WriteLine("APP:CreateTray");
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

            var mniExit = new Forms.ToolStripMenuItem("E&xit");
            mniExit.Click += (_, __) => { Current.Shutdown(); };
            trayIcon.ContextMenuStrip.Items.Add(mniExit);


            _Syncer.ntfyIcon = trayIcon;

            try
            {
                Console.WriteLine("APP:SetProtoHandler:" + Constants.Protocol);

                Installer.RegisterThisApplication(Constants.Protocol, Constants.Title);
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
                trayIcon.ShowBalloonTip(
                     tipTitle: Constants.Title,
                     tipText: "Protocol Handler for '" + Constants.Protocol + "' could not be installed.",
                     tipIcon: Forms.ToolTipIcon.Warning,
                     timeout: 100
                   );
            }

            Console.WriteLine("APP:AttemptFirstClientInstanceClientSync:" + string.Join(",", startupEventArgs.Args));

            _Syncer.Sync(startupEventArgs.Args);
        }

        internal void OnNextStartup(VB.StartupNextInstanceEventArgs nextInstanceEventArgs)
        {
            Console.WriteLine("App:NextInstanceLaunched:" + string.Join(",", nextInstanceEventArgs.CommandLine));

            _Syncer.Sync(nextInstanceEventArgs.CommandLine.ToArray());
        }
    }
}
