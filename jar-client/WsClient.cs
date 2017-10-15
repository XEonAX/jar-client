using JAR.Client.Messages;
using System;
using System.Collections.Generic;
using WebSocketSharp;

namespace JAR.Client
{
    internal class WsClient
    {
        Dictionary<string, WebSocket> Connections = new Dictionary<string, WebSocket>();

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

        #region WsClientEvents
        internal event EventHandler<WSHostArgs> OnConnect;
        internal event EventHandler<WSHostArgs> OnDisconnect;
        internal event EventHandler<WSTokenArgs> OnBrowserConnect;
        internal event EventHandler<WSTokenArgs> OnBrowserDisconnect;
        internal event EventHandler<WSTokenArgs> OnSubscribe;
        internal event EventHandler<WSTokenArgs> OnLogout;
        #endregion

        #region WsClient Methods

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

        internal void Tick(Dictionary<string, List<string>> servers, TickMessage tickMsg)
        {
            Console.WriteLine("WSCLIENT:Tick:servers.count:" +servers.Count );
            foreach (var url in servers.Keys)
            {
                var ctokens = servers[url];
                tickMsg.ctokens = ctokens.ToArray();
                Connections[url].SendAsync(tickMsg.Serialize(), (sent) =>
                {
                    if (sent)
                        Console.WriteLine("WSCLIENT:Tick:sent:" + sent);
                });
            }
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

        #endregion

        #region Websocket Event Handlers

        private void Ws_OnOpen(object sender, EventArgs e)
        {
            var ws = sender as WebSocket;
            Console.WriteLine("WSCLIENT:Handlers:OnOpen:" + ws.Url);
            Connections[ws.Url.ToString()] = ws;
            OnConnect?.Invoke(this, new WSHostArgs(ws.Url.ToString()));
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
            Console.WriteLine("WSCLIENT:Handlers:Ws_OnMessage:" + ws.Url + ":" + e.Data);

            var actionMsg = e.Data.DeSerialize<ActionMessage>();
            Console.WriteLine("WSCLIENT:Handlers:Ws_OnMessage:actionMsg.action:" + actionMsg.action);

            switch (actionMsg.action)
            {
                case Actions.Notice:
                    var noticeMsg = e.Data.DeSerialize<NoticeMessage>();
                    Console.WriteLine("WSCLIENT:Handlers:Ws_OnMessage:noticeMsg.notice:" + noticeMsg.notice);
                    Console.WriteLine("WSCLIENT:Handlers:Ws_OnMessage:noticeMsg.ctoken:" + noticeMsg.ctoken);

                    switch (noticeMsg.notice)
                    {
                        case Notices.Subscribed:
                            OnSubscribe?.Invoke(this, new WSTokenArgs(noticeMsg.ctoken));
                            break;
                        case Notices.BrowserConnected:
                            OnBrowserConnect?.Invoke(this, new WSTokenArgs(noticeMsg.ctoken));
                            break;
                        case Notices.BrowserDisconnected:
                            OnBrowserDisconnect?.Invoke(this, new WSTokenArgs(noticeMsg.ctoken));
                            break;
                        case Notices.Logout:
                            OnLogout?.Invoke(this, new WSTokenArgs(noticeMsg.ctoken));
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
            Console.WriteLine("WSCLIENT:Handlers:Ws_OnError:" + e.Message + ":Ex:" + e.Exception);
        }

        #endregion

    }
}