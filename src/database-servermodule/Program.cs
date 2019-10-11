using System;
using custom_message_based_implementation.consts;
using custom_message_based_implementation.encoding;
using custom_message_based_implementation.model;
using message_based_communication.model;
using NLog;

namespace database_servermodule
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public const bool IsLocalhost = true;
        public const bool IsTesting = false;

        public static void Main(string[] args)
        {
            SetupNLog();
            WriteLineAndLog("Database servermodule is starting...");
            try
            {
                // var portToListenForRegistration = new Port() { ThePort = 5533 };

                var serverModuleConnectionInformation = new ConnectionInformation()
                {
                    IP = new IP() { TheIP = IsLocalhost ? "127.0.0.1" : "Fill in IP" },
                    Port = new Port() { ThePort = 5522 }
                };

                var databaseServerModuleConnInfo = new ConnectionInformation()
                {
                    IP = new IP() { TheIP = (IsLocalhost) ? "127.0.0.1" : "Fill in IP" },
                    Port = new Port() { ThePort = 5535 }
                };

                var database = new DatabaseServermodule(new ModuleType() { TypeID = ModuleTypeConst.MODULE_TYPE_DATABASE });
                database.Setup(serverModuleConnectionInformation, new Port() { ThePort = 5523 }, databaseServerModuleConnInfo, new CustomEncoder());

                WriteLineAndLog("Database servermodule has started successfully with self IP: " + databaseServerModuleConnInfo.IP.TheIP + " and server IP: " + serverModuleConnectionInformation.IP.TheIP);
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

            Console.ReadKey();
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
