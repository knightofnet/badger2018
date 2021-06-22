using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AryxDevLibrary.utils.logger;

namespace BadgerCommonLibrary.constants
{
    public class CommonCst
    {
        public const string UpdaterLogFile = "logUpd.log";
        public const string AppLogFile = "log.log";
        public const string WavePlayerLogFile = "logWave.log";

        public const string MainLogName = "main";


#if DEBUG
        public const Logger.LogLvl ConsoleLogLvl = Logger.LogLvl.DEBUG;
        public const Logger.LogLvl FileLogLvl = Logger.LogLvl.DEBUG;
#else
        public const Logger.LogLvl ConsoleLogLvl = Logger.LogLvl.INFO;
        public const Logger.LogLvl FileLogLvl = Logger.LogLvl.INFO;
#endif

        public const string OneShotUpdateConfFilename = "oneShotUpdateConf.xml";

        public const string SqliteAppSalt = "Z0kp5dlDeIOC6frMv7KEweN4Jdkw2lxL";

    }
}
