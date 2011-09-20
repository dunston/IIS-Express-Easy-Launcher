namespace LaunchIISExpress
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Security.Principal;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Linq;

    class Program
    {
        #region Enumerations

        private enum AppPool
        {
            Clr4IntegratedAppPool = 1, Clr4ClassicAppPool, Clr2IntegratedAppPool, Clr2ClassicAppPool, UnmanagedClassicAppPool
        }

        #endregion Enumerations

        #region Methods

        /// <exception cref="Exception">E: 102 - Could not find port</exception>
        /// <exception cref="Exception">E: 103 - Could not find host</exception>
        static void Main(string[] args)
        {
            Console.Title = "IIS Express easy launcher";
            if (args.Length > 0)
            {
                string pathToIIS = string.Empty;

                if (Directory.Exists(@"C:\Program Files (x86)\IIS Express\"))
                {
                    pathToIIS = Path.Combine(@"C:\Program Files (x86)\IIS Express\", "iisexpress.exe");
                }

                if (Directory.Exists(@"C:\Program Files\IIS Express\"))
                {
                    pathToIIS = Path.Combine(@"C:\Program Files\IIS Express\", "iisexpress.exe");
                }

                // cmd /c start /D"C:\Program Files (x86)\IIS Express\" iisexpress.exe /port:%%random%% /path:"%1" /clr:v2.0

                XmlDocument iisexpressXml;
                if (File.Exists(Path.Combine(args[0], "iisexpress.config")))
                {
                    string pathtoIssExpressConfig = Path.Combine(args[0], "iisexpress.config");
                    var configFile = XDocument.Load(pathtoIssExpressConfig);
                    XElement siteElement = configFile.Root.Element("system.applicationHost").Element("sites").Element("site");
                    XAttribute physicalPath = siteElement.Element("application").Element("virtualDirectory").
                        Attribute("physicalPath");

                    XAttribute bindingInformation = siteElement.Element("bindings").Element("binding").
                        Attribute("bindingInformation");

                    if (physicalPath != null)
                        physicalPath.SetValue(pathtoIssExpressConfig.Remove(pathtoIssExpressConfig.LastIndexOf("\\")));

                    if (bindingInformation != null)
                    {
                        string[] information = bindingInformation.Value.Split(':');
                        int port;

                        if (!int.TryParse(information[1], out port))
                            throw new Exception("E: 102 - Could not find port");
                        string host = information[2];

                        if (string.IsNullOrEmpty(host))
                            throw new Exception("E: 103 - Could not find host");

                        if (port <= 1024 || host != "localhost")
                        {
                            WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                            bool hasAdministrativeRight = pricipal.IsInRole(WindowsBuiltInRole.Administrator);

                            if (!hasAdministrativeRight)
                            {
                                if (port <= 1024)
                                {
                                    Console.WriteLine("Without administrative rights, you cant open port under 1024, please select another!");
                                    string newPort = Console.ReadLine();

                                    if (!int.TryParse(newPort.Trim(), out port) || port <= 1024)
                                    {
                                        while (true)
                                        {
                                            Console.Write("Error in parsing, or port is lower then 1024, choose another");
                                            newPort = Console.ReadLine();

                                            if (int.TryParse(newPort.Trim(), out port) && port > 1024)
                                                break;

                                        }
                                    }
                                }

                                if (host != "localhost")
                                {
                                    Console.WriteLine("Without administrative rights, you can only bind to localhost");
                                    Console.WriteLine("Want to do this ?");

                                    if (Console.ReadLine().Contains("y"))
                                        host = "localhost";

                                }
                                bindingInformation.SetValue(string.Format("{0}:{1}:{2}", information[0], port, host));
                            }
                        }
                    }
                    configFile.Save(pathtoIssExpressConfig);
                }
                else
                {

                    File.Copy(@"C:\Program Files (x86)\IIS Express\AppServer\applicationhost.config", Path.Combine(args[0], "iisexpress.config"));

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
                }
                string commandParameters = string.Format("/config:\"{0}\"", Path.Combine(args[0], "iisexpress.config"));

                //string keypress = "";
                //while (!keypress.ToLower().StartsWith("q"))
                //{

                //}
                using (Process p = new Process())
                {

                    //.Start(pathToIIS, commandParameters)
                    p.StartInfo.FileName = pathToIIS;
                    p.StartInfo.Arguments = commandParameters;
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.EnableRaisingEvents = true;
                    p.OutputDataReceived += new DataReceivedEventHandler(process_OutputDataReceived);
                    p.Start();
                    p.BeginOutputReadLine();

                    p.WaitForExit();
                }

            }
            else
            {
                PrintHelp();
            }
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Usage LaunchIISExpress.exe [PATH to website]");
            Console.WriteLine("Looking for applicationhost.config in the website, if it dont find it, it ask what type it is, and makes the config file");

            Console.Read();
        }

        private static void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        #endregion Methods
    }
}