using System;

namespace JAR.Client
{
    public class EntryPoint
    {
        /// <summary>
        /// The Main Entry Point of the Application
        /// </summary>
        /// <param name="args">command line arguments</param>
        [STAThread]
        public static void Main(string[] args)
        {
            SingleInstanceManager manager = new SingleInstanceManager();
            manager.Run(args);
        }
    }
}
