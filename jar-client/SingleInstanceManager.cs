﻿using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JAR.Client
{
    public class SingleInstanceManager : WindowsFormsApplicationBase
    {
        private App _application;
        
        public SingleInstanceManager()
        {
            IsSingleInstance = true;
        }

        protected override bool OnStartup(StartupEventArgs eventArgs)
        {
            // First time _application is launched
            Console.WriteLine("SIM:FirstLaunch:" + string.Join(",",eventArgs.CommandLine));
            _application = new App();
            _application.InitializeComponent(); 
            _application.Run();
            return false;
        }

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            // Subsequent launches
            Console.WriteLine("SIM:NextLaunch:" + string.Join(",", eventArgs.CommandLine));
            base.OnStartupNextInstance(eventArgs);
            _application.OnNextStartup(eventArgs);
        }
    }
}
