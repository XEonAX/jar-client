using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JAR.Client
{
    public static class Installer
    {
        public static void RegisterProtocol(string protocol, string protocolName, string executablePath)
        {
            var executableFilename = Path.GetFileName(executablePath);
            RegistryKey classes = Registry.CurrentUser.OpenSubKey("Software\\Classes\\",true);
            RegistryKey protoHandler = classes.CreateSubKey(protocol);
            protoHandler.SetValue("", "URL:" + protocolName);
            protoHandler.SetValue("URL Protocol", "");
            RegistryKey defaultIcon = protoHandler.CreateSubKey("DefaultIcon");
            defaultIcon.SetValue("", executableFilename);
            RegistryKey shell = protoHandler.CreateSubKey("shell");
            RegistryKey open = shell.CreateSubKey("open");
            RegistryKey command = open.CreateSubKey("command");
            command.SetValue("", executablePath + " %1");
        }

        public static void UnregisterProtocol(string protocol)
        {
            RegistryKey classes = Registry.CurrentUser.OpenSubKey("Software\\Classes\\",true);
            classes.DeleteSubKeyTree(protocol, false);
        }

        public static void RegisterThisApplication(string protocol, string protocolName)
        {
            RegisterProtocol(protocol, protocolName, Assembly.GetCallingAssembly().Location);
        }

        public static void UnregisterThisApplication(string protocol)
        {
            UnregisterProtocol(protocol);
        }

        public static bool IsProtocolRegistered(string protocol)
        {
            return (Registry.CurrentUser.OpenSubKey("Software\\Classes\\" + protocol) != null);
        }
    }

}
