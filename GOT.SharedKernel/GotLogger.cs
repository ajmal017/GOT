using NLog;
using NLog.Config;
using NLog.Targets;

namespace GOT.SharedKernel
{
    public class GotLogger : IGotLogger
    {
        private const string FILE_NAME = "\\GOT_${shortdate}";
        private const string EXTENSION = ".log";
        private static readonly Logger Log = LogManager.GetLogger("Got logger");

        public GotLogger()
        {
            Initialize();
        }

        public void AddLog(string message, int type = 1)
        {
            switch (type) {
                case 1: 
                    Log.Info(message);
                    break;
                case 2: 
                    Log.Warn(message);
                    break;
                case 3: 
                    Log.Error(message);
                    break;
                case 4: 
                    Log.Fatal(message);
                    break;
            }
        }
        
        private void Initialize()
        {
            var logPath = FolderBuilder.GetLogFolderPath() + FILE_NAME + EXTENSION;
            var config = new LoggingConfiguration();
            var logfile = new FileTarget("logfile") {FileName = logPath};
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);
            LogManager.AutoShutdown = true;
            LogManager.Configuration = config;

            AddLog("=============  Started Logging  =============");
        }
    }
}