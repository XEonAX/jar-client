using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace JAR.Client
{
    class Syncer
    {
        private MainWindow mainWindow;
        private WsClient wsclient = new WsClient();
        public Syncer(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            this.mainWindow.OnConnectAllowed += MainWindow_OnConnectAllowed;
            wsclient.OnConnect += Wsclient_OnConnect;
            wsclient.OnDisconnect += Wsclient_OnDisconnect;
            wsclient.OnBrowserDisconnect += Wsclient_OnBrowserDisconnect;
            wsclient.OnSubscribe += Wsclient_OnSubscribe;
            wsclient.OnLogout += Wsclient_OnLogout;
        }

        private void MainWindow_OnConnectAllowed(object sender, ConnectArgs e)
        {
            Connect(e.Host);
        }

        private void Wsclient_OnLogout(object sender, WsClient.WSTokenArgs e)
        {
            Rooms.Remove(e.Token);
            var pauseTicker = true;
            foreach (var _ctoken in Rooms.Keys)
            {
                var _room = Rooms[_ctoken];
                if (_room.state == RoomStates.Subscribed)
                {
                    pauseTicker = false;
                    break;
                }
            }
            if (pauseTicker)
            {
                ticker.Pause();
            }
        }

        private void Wsclient_OnSubscribe(object sender, WsClient.WSTokenArgs e)
        {
            if (Rooms.ContainsKey(e.Token) && Rooms[e.Token].state == RoomStates.Subscribing)
            {
                Rooms[e.Token].state = RoomStates.Subscribed;
                ticker.Init();
                ntfyIcon.ShowBalloonTip(
                    tipTitle: Constants.Title,
                    tipText: "Subscribed to " + Rooms[e.Token].url,
                    tipIcon: ToolTipIcon.Info,
                    timeout: 2000
                  );
            }
        }

        private void Wsclient_OnBrowserDisconnect(object sender, WsClient.WSTokenArgs e)
        {
            Rooms[e.Token].state = RoomStates.Disconnected;
            var pauseTicker = true;
            foreach (var _ctoken in Rooms.Keys)
            {
                var _room = Rooms[_ctoken];
                if (_room.state == RoomStates.Subscribed)
                {
                    pauseTicker = false;
                    break;
                }
            }
            if (pauseTicker)
            {
                ticker.Pause();
            }
        }

        private void Wsclient_OnDisconnect(object sender, WsClient.WSHostArgs e)
        {
            ntfyIcon.ShowBalloonTip(
                tipTitle: Constants.Title,
                tipText: "Disconnected from " + e.Host,
                tipIcon: ToolTipIcon.Warning,
                timeout: 2000
              );
            foreach (var ctoken in Rooms.Keys)
            {
                var room = Rooms[ctoken];
                if (room.url == e.Host)
                    Rooms.Remove(ctoken);

            }
            var pauseTicker = true;
            foreach (var _ctoken in Rooms.Keys)
            {
                var _room = Rooms[_ctoken];
                if (_room.state == RoomStates.Subscribed)
                {
                    pauseTicker = false;
                    break;
                }

            }
            if (pauseTicker)
            {
                ticker.Pause();
            }
        }

        private void Wsclient_OnConnect(object sender, WsClient.WSHostArgs e)
        {
            Console.WriteLine("Syncer:Handlers:OnConnect:" + e.Host);
            ntfyIcon.ShowBalloonTip(
                tipTitle: Constants.Title,
                tipText: "Connected to " + e.Host,
                tipIcon: ToolTipIcon.Info,
                timeout: 2000
              );
            foreach (var ctoken in Rooms.Keys)
            {
                var room = Rooms[ctoken];
                if (room.url == e.Host && room.state == RoomStates.New)
                {
                    room.state = RoomStates.Subscribing;
                    wsclient.Subscribe(room.url, room.ctoken);
                }
            }
        }

        public Dictionary<string, Room> Rooms = new Dictionary<string, Room>();
        internal NotifyIcon ntfyIcon;
        private Ticker ticker = new Ticker();

        internal void Sync(string[] args)
        {
            var protostr = args.Count() == 0 ? "" : args.Last();
            if (!protostr.StartsWith(Constants.Protocol + "://"))
                return;
            try
            {
                var wsurl = new UriBuilder(protostr.Substring((Constants.Protocol + "://").Length)).Uri;
                var wsHostUrl = wsurl.GetLeftPart(UriPartial.Authority) + "/";
                var wsToken = wsurl.PathAndQuery.Substring(1);

                Console.WriteLine("Syncer:Sync:GotValidWSUrl:" + wsurl.ToString());
                if (Rooms.ContainsKey(wsToken))
                {
                    Console.WriteLine("Syncer:Sync:Room Exists for ctoken:" + wsToken);
                    Rooms[wsToken].state = RoomStates.New;
                }
                else
                {
                    Console.WriteLine("Syncer:Sync:New room for ctoken:" + wsToken);
                    Rooms[wsToken] = new Room
                    {
                        state = RoomStates.New,
                        ctoken = wsToken,
                        url = wsHostUrl
                    };
                }
                if (!mainWindow.IsVisible)
                {
                    mainWindow.Show(new WindowDetails
                    {
                        hostname = wsurl.Host,
                        ident = wsurl.ToString(),
                        wsHost = wsHostUrl,
                        wsToken = wsToken
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Syncer:Sync:UrlError:" + ex.ToString());
            }
        }

        private void Connect(string _wsHost)
        {
            wsclient.Connect(_wsHost);
        }
    }

    internal class Room
    {
        internal RoomStates state;
        internal string url;
        internal string ctoken;
    }

    internal enum RoomStates
    {
        New = 0,
        Subscribing = 1,
        Subscribed = 2,
        Disconnected = 3

    }
}
