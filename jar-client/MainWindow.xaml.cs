using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JAR.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WindowDetails Details;

        public MainWindow()
        {
            InitializeComponent();
            //System.Windows.Forms.NotifyIcon nIcon = new System.Windows.Forms.NotifyIcon();

            //nIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            //nIcon.Visible = true;
            //nIcon.ShowBalloonTip(5000, "Title", "Text", System.Windows.Forms.ToolTipIcon.Info);
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void btnHide_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        internal void Show(WindowDetails details)
        {
            Details = details;
            if (IsLoaded)
            {
                identpic.Value = details.ident;
                hostname.Text = details.hostname;
            }
            Show();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            identpic.Value = Details.ident;
            hostname.Text = Details.hostname;
        }

        internal event EventHandler<ConnectArgs> OnConnectAllowed;

        private void btnYes_Click(object sender, RoutedEventArgs e)
        {
            OnConnectAllowed?.Invoke(this, new ConnectArgs(Details.wsHost));
            Hide();
        }

        private void btnNo_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }

    internal class ConnectArgs : EventArgs
    {
        internal string Host { get; set; }
        internal ConnectArgs(string host)
        {
            Host = host;
        }
    }

    internal class WindowDetails
    {
        internal string hostname { get; set; }
        internal string ident { get; set; }
        internal string wsHost { get; set; }
        internal string wsToken { get; set; }
    }
}
