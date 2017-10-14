using JAR.Client.Messages;
using System;
using System.Collections.Generic;
using WebSocketSharp;

namespace JAR.Client
{
    internal class WsClient
    {
        #region EventsArgs
        internal class WSHostArgs : EventArgs
        {
            public WSHostArgs(string Host)
            {
                this.Host = Host;
            }

            public string Host { get; set; }
        }
        internal class WSTokenArgs : EventArgs
        {
            public WSTokenArgs(string Token)
            {
                this.Token = Token;
            }
            public string Token { get; set; }
        }

        #endregion
        #region Events
        internal event EventHandler<WSHostArgs> OnConnect;
        internal event EventHandler<WSHostArgs> OnDisconnect;
        internal event EventHandler<WSTokenArgs> OnBrowserDisconnect;
        internal event EventHandler<WSTokenArgs> OnSubscribe;
        internal event EventHandler<WSTokenArgs> OnLogout;
        #endregion

        Dictionary<string, WebSocket> Connections = new Dictionary<string, WebSocket>();
        internal void Connect(string url)
        {
            Console.WriteLine("WSCLIENT:Connect:" + url);
            if (Connections.ContainsKey(url))
            {
                Console.WriteLine("WSCLIENT:Connect:Connection exist in Connections:" + url);
                OnConnect?.Invoke(this, new WSHostArgs(url));
            }
            else
            {
                Console.WriteLine("WSCLIENT:Connect:New connection to create:" + url);
                var ws = new WebSocket(url);
                ws.OnOpen += Ws_OnOpen;
                ws.OnClose += Ws_OnClose;
                ws.OnMessage += Ws_OnMessage;
                ws.OnError += Ws_OnError;
                ws.ConnectAsync();
            }
        }

        private void Ws_OnOpen(object sender, EventArgs e)
        {
            var ws = sender as WebSocket;
            Console.WriteLine("WSCLIENT:Handlers:OnOpen:" + ws.Url);
            Connections[ws.Url.ToString()] = ws;
            OnConnect?.Invoke(this, new WSHostArgs(ws.Url.ToString()));
        }

        internal void Subscribe(string url, string ctoken)
        {
            Console.WriteLine("WSCLIENT:Subscribe:" + url + ctoken);
            if (Connections.ContainsKey(url))
            {
                Connections[url].SendAsync(
                    new SubscribeMessage(
                        ctoken: ctoken
                    ).Serialize(),
                    (sent) =>
                        {
                            if (sent)
                                Console.WriteLine("WSCLIENT:Subscribe:Sent:" + sent);
                        }
                    );
            }

        }

        private void Ws_OnClose(object sender, CloseEventArgs e)
        {
            var ws = sender as WebSocket;
            Console.WriteLine("WSCLIENT:Handlers:OnClose:" + ws.Url);
            Connections.Remove(ws.Url.ToString());
            OnDisconnect?.Invoke(this, new WSHostArgs(ws.Url.ToString()));
        }
        private void Ws_OnMessage(object sender, MessageEventArgs e)
        {
            var ws = sender as WebSocket;
            Console.WriteLine("WSCLIENT:Handlers:OnMessage:" + ws.Url + ":" + e.Data);

            var amsg = e.Data.DeSerialize<ActionMessage>();
            Console.WriteLine("WSCLIENT:Handlers:OnMessage:amsg" + amsg);

            switch (amsg.action)
            {
                case Actions.Notice:
                    var nmsg = e.Data.DeSerialize<NoticeMessage>();
                    switch (nmsg.notice)
                    {
                        case Notices.Subscribed:
                            OnSubscribe?.Invoke(this, new WSTokenArgs(nmsg.ctoken));
                            break;
                        case Notices.BrowserDisconnected:
                            OnBrowserDisconnect?.Invoke(this, new WSTokenArgs(nmsg.ctoken));
                            break;
                        case Notices.Logout:
                            OnLogout?.Invoke(this, new WSTokenArgs(nmsg.ctoken));
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        private void Ws_OnError(object sender, ErrorEventArgs e)
        {
            Console.WriteLine("WSCLIENT:Handlers:OnError:" + e.Exception);
        }

        private class Actions
        {
            internal const string Notice = "notice";
        }

        private class Notices
        {
            internal const string Subscribed = "subscribed";
            internal const string BrowserDisconnected = "browserdisconnected";
            internal const string Logout = "logout";
        }
    }
}