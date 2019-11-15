using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using custom_message_based_implementation.consts;
using custom_message_based_implementation.encoding;
using custom_message_based_implementation.model;
using message_based_communication.model;
using NLog;
using File = System.IO.File;

namespace database_servermodule
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        private const string SELF_IP = "sip";
        private const string SELF_COMM_PORT = "scp";
        private const string ROUTER_IP = "rip";
        private const string ROUTER_COMM_PORT = "rcp";
        private const string ROUTER_REG_PORT = "rrp";
        private const string IS_LOCALHOST = "isLocal";

        public static bool IsLocalhost = true;
        public const bool IsTesting = false;

        private const string SQL_SERVR_PATH = "/opt/mssql/bin/sqlservr";
        private const int MILISECONDS_PER_SECOND = 1000;
        public static void Main(string[] args)
        {
            SetupNLog();
            WriteLineAndLog("Database servermodule is starting...");
            try
            {

                //start sql server if following path exists
                if (File.Exists(SQL_SERVR_PATH))
                {
                    Logger.Debug("The file for the sql - server exists");
                    var processInfo = new ProcessStartInfo(SQL_SERVR_PATH);
                    Process.Start(processInfo);
                    Logger.Debug("Successfully started the sql - server process. Will wait 15 seconds for it to start");
                    Thread.Sleep(15 * MILISECONDS_PER_SECOND);
                    Logger.Debug("finished waiting");
                }



                Port portToRegisterOn = new Port() { ThePort = 5523 };

                var router_conn_info = new ConnectionInformation()
                {
                    IP = new IP() { TheIP = null },
                    Port = new Port() { ThePort = 5522 } // is set after the reading of the system args because isLocalhost might change
                };

                var self_conn_info = new ConnectionInformation()
                {
                    IP = new IP() { TheIP = null }, // is set after the reading of the system args because isLocalhost might change
                    Port = new Port() { ThePort = 5582 } // todo port stuff
                };


                //setting network infromation with sys args
                foreach (var arg in args)
                {
                    var split = arg.Split(":");
                    if (2 != split.Length)
                    {
                        throw new ArgumentException("Got badly formatted system arguments");
                    }

                    if (split[0].Equals(SELF_IP)) // set self ip
                    {
                        self_conn_info.IP.TheIP = split[1];
                        Console.WriteLine("Overriding self ip with: " + split[1]);
                    }
                    else if (split[0].Equals(SELF_COMM_PORT)) // set self communication port
                    {
                        self_conn_info.Port.ThePort = Convert.ToInt32(split[1]);
                        Console.WriteLine("Overriding self communication port with: " + split[1]);
                    }
                    else if (split[0].Equals(ROUTER_IP)) // set router ip
                    {
                        router_conn_info.IP.TheIP = split[1];
                        Console.WriteLine("Overriding router ip with: " + split[1]);
                    }
                    else if (split[0].Equals(ROUTER_COMM_PORT)) // set router communication port
                    {
                        router_conn_info.Port.ThePort = Convert.ToInt32(split[1]);
                        Console.WriteLine("Overriding router communication port with: " + split[1]);
                    }
                    else if (split[0].Equals(ROUTER_REG_PORT)) // set router registration port
                    {
                        portToRegisterOn.ThePort = Convert.ToInt32(split[1]);
                        Console.WriteLine("Overriding router registration port with: " + split[1]);
                    }else if (split[0].Equals(IS_LOCALHOST))
                    {
                        if (split[1].Equals("true", StringComparison.InvariantCultureIgnoreCase))
                        {
                            IsLocalhost = true;
                        }else if (split[1].Equals("false", StringComparison.InvariantCultureIgnoreCase))
                        {
                            IsLocalhost = false;
                        }
                        else
                        {
                            throw  new Exception("ERROR IN SYSTEM ARGUMENTS");
                        }
                    }
                }
                Console.WriteLine("Using Localhost:" + IsLocalhost);

                if (null == self_conn_info.IP.TheIP)
                {
                    self_conn_info.IP.TheIP = IsLocalhost ? "127.0.0.1" : DatabaseServermodule.GetIP();
                }
                if (null == router_conn_info.IP.TheIP)
                {
                    self_conn_info.IP.TheIP = IsLocalhost ? "127.0.0.1" : "Fill in IP";
                }

                Console.WriteLine(
                    "\n\n Using the following network parameters: \n self network info: \n{ IP: " + self_conn_info.IP.TheIP + ", comm_port: " + self_conn_info.Port.ThePort + " }"
                    + "\n and router network infromation: \n{ IP: " + router_conn_info.IP.TheIP + ", comm_port: " + router_conn_info.Port.ThePort + ", reg_port: " + portToRegisterOn.ThePort + "}"
                    + "\n\n");

                var database = new DatabaseServermodule(new ModuleType() { TypeID = ModuleTypeConst.MODULE_TYPE_DATABASE });
                database.Setup(router_conn_info, portToRegisterOn , self_conn_info, new CustomEncoder());

                WriteLineAndLog("Database servermodule has started successfully with self IP: " + self_conn_info.IP.TheIP + " and server IP: " + router_conn_info.IP.TheIP);
                if(IsTesting) {
                    var pk = database.Login(new Email{TheEmail = "admin@ccfeu.com"}, new Password{ThePassword = "pass"});
                    Console.WriteLine("Primary key: " + pk.TheKey);
                    database.CreateAccount(new Email{TheEmail = "new@user.com"}, new Password{ThePassword = "password"});
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Database servermodule encountered an exception while in the setup process: " + ex);
                Logger.Debug(ex);
            }

            Console.WriteLine("Putting main thread to sleep in a loop");
            while (true)
            {
                Thread.Sleep(500);
            }
        }

        private static void SetupNLog()
        {
            var config = new NLog.Config.LoggingConfiguration();
            var logFile = "database-servermodule-log.txt";

            /*
            var rootFolder = System.AppDomain.CurrentDomain.BaseDirectory;
            if (File.Exists(Path.Combine(rootFolder, logFile)))
            {  
                File.Delete(Path.Combine(rootFolder, logFile));
            }
            */

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = logFile };

            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            // Apply config           
            LogManager.Configuration = config;
        }

        public static void WriteLineAndLog(string message)
        {
            Console.WriteLine(message);
            Logger.Info(message);
        }
    }
}
