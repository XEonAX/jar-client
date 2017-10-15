using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JAR.Client
{

    static class Constants
    {
        /// <summary>
        /// Title of the application
        /// </summary>
        internal const string Title = "JAR - Client";

        /// <summary>
        /// Protocol to use for Protocol Handler
        /// </summary>
        internal const string Protocol = "jarclient";
    }

    /// <summary>
    /// JSON message Actions
    /// </summary>
    static class Actions
    {
        internal const string Notice = "notice";
    }


    /// <summary>
    /// JSON Message with `Action` as `notice` will have Notice of this types
    /// </summary>
    static class Notices
    {
        internal const string Subscribed = "subscribed";
        internal const string BrowserConnected = "browserconnected";
        internal const string BrowserDisconnected = "browserdisconnected";
        internal const string Logout = "logout";
    }
    
    internal enum RoomStates
    {
        New = 0,
        Subscribing = 1,
        Subscribed = 2,
        Disconnected = 3
    }
}
