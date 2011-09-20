using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace LaunchIISExpress
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                string pathToIIS = string.Empty;

                if(Directory.Exists(@"C:\Program Files (x86)\IIS Express\"))
                {
                    pathToIIS = Path.Combine(@"C:\Program Files (x86)\IIS Express\", "iisexpress.exe");
                }
                
                if (Directory.Exists(@"C:\Program Files\IIS Express\"))
                {
                    pathToIIS = Path.Combine(@"C:\Program Files\IIS Express\", "iisexpress.exe");
                }

                // cmd /c start /D"C:\Program Files (x86)\IIS Express\" iisexpress.exe /port:%%random%% /path:"%1" /clr:v2.0


                XmlDocument iisexpressXml = new XmlDocument();
                if (File.Exists(Path.Combine(args[0], "iisexpress.config")))
                {
                    string commandParameters = string.Format("/config:\"{0}\"", Path.Combine(args[0], "iisexpress.config"));
                    iisexpressXml.Load(Path.Combine(args[0], "iisexpress.config"));
                    Process process = Process.Start(pathToIIS, commandParameters);
                }
                else
                {



                    File.Copy(@"C:\Program Files (x86)\IIS Express\AppServer\applicationhost.config", Path.Combine(args[0], "iisexpress.config"));

                    string commandParameters = string.Format("/config:\"{0}\"", Path.Combine(args[0], "iisexpress.config"));

                    iisexpressXml = new XmlDocument();
                    iisexpressXml.Load(Path.Combine(args[0], "iisexpress.config"));

                    XmlNode sites = iisexpressXml.SelectSingleNode("configuration/system.applicationHost/sites");
                    XmlNode virtualDirectoryNode = sites.SelectSingleNode("site/application/virtualDirectory");
                    virtualDirectoryNode.Attributes["physicalPath"].Value = args[0];

                    Console.WriteLine("What application pool to use?");
                    Console.WriteLine("1: Clr4IntegratedAppPool");
                    Console.WriteLine("2: Clr4ClassicAppPool");
                    Console.WriteLine("3: Clr2IntegratedAppPool");
                    Console.WriteLine("4: Clr2ClassicAppPool");
                    Console.WriteLine("5: UnmanagedClassicAppPool");

                    XmlNode appPoolNode = sites.SelectSingleNode("applicationDefaults");
                    AppPool keyPresse;
                    ConsoleKeyInfo key = Console.ReadKey();

                    while (!AppPool.TryParse(key.KeyChar.ToString(), out keyPresse))
                    {
                        key = Console.ReadKey();
                    }

                    switch (keyPresse)
                    {
                        case AppPool.Clr4IntegratedAppPool:
                            appPoolNode.Attributes["applicationPool"].Value = "Clr4IntegratedAppPool";
                            break;

                        case AppPool.Clr4ClassicAppPool:
                            appPoolNode.Attributes["applicationPool"].Value = "Clr4ClassicAppPool";
                            break;

                        case AppPool.Clr2IntegratedAppPool:
                            appPoolNode.Attributes["applicationPool"].Value = "Clr2IntegratedAppPool";
                            break;

                        case AppPool.Clr2ClassicAppPool:
                            appPoolNode.Attributes["applicationPool"].Value = "Clr2ClassicAppPool";
                            break;

                        case AppPool.UnmanagedClassicAppPool:
                            appPoolNode.Attributes["applicationPool"].Value = "UnmanagedClassicAppPool";
                            break;
                    }

                    iisexpressXml.Save(Path.Combine(args[0], "iisexpress.config"));

                    Process process = Process.Start(pathToIIS, commandParameters);
                }
            }
            else
            {
                PrintHelp();
            }
        }


        private enum AppPool
        {
            Clr4IntegratedAppPool = 1, Clr4ClassicAppPool, Clr2IntegratedAppPool, Clr2ClassicAppPool, UnmanagedClassicAppPool
        }
        private static void PrintHelp()
        {
            Console.WriteLine("Usage LaunchIISExpress.exe [PATH to website]");
            Console.WriteLine("Looking for applicationhost.config in the website, if it dont find it, it ask what type it is, and makes the config file");

            Console.Read();
        }
    }
}
