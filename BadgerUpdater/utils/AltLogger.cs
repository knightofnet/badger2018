using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using BadgerCommonLibrary.constants;

namespace BadgerUpdater.utils
{
    public class AltLogger : Logger
    {

        private static Regex escapeRegex = new Regex(@"<\*(!|)\w+\*>(?<text>.*?)<\*\/(!|)\*>", RegexOptions.Compiled);

        public AltLogger(string path)
            : base(path)
        {
        }

        public AltLogger(string path, LogLvl consoleOutLvl, LogLvl fileOutLvl)
            : base(path, consoleOutLvl, fileOutLvl)
        {
        }

        public AltLogger(string path, LogLvl consoleOutLvl, LogLvl fileOutLvl, string maxLogFileSize)
            : base(path, consoleOutLvl, fileOutLvl, maxLogFileSize)
        {
        }

        public bool ShowDtAndLogLevel { get; set; }


        protected override void WriteLineToConsole(string line, string levelLbl, string lineFormatted)
        {

            if (!ShowDtAndLogLevel || _logLevelFile > LogLvl.DEBUG)
            {
                lineFormatted = String.Format("{0}", line);
            }
            else
            {
                if ("ERROR".Equals(levelLbl))
                {
                    levelLbl = "<*red*>ERROR<*/*>";
                }
                else if ("WARN ".Equals(levelLbl))
                {
                    levelLbl = "<*orange*>WARN <*/*>";
                } if ("INFO ".Equals(levelLbl))
                {
                    levelLbl = "<*cyan*>INFO <*/*>";
                }

                lineFormatted = String.Format("{0} {1} : [{3}] : {2}", (DateTime.Now).ToShortDateString(),
                (DateTime.Now).ToLongTimeString(), line, levelLbl);
            }

            if (escapeRegex.IsMatch(lineFormatted))
            {
                ConsoleUtils.WriteLineColor(lineFormatted);
            }
            else
            {
                base.WriteLineToConsole(line, levelLbl, lineFormatted);
            }
        }

        protected override void WriteLineToLogFile(string line, string levelLbl, string lineFormatted)
        {
            if (escapeRegex.IsMatch(lineFormatted))
            {
                lineFormatted = escapeRegex.Replace(lineFormatted, @"$3");
            }

            base.WriteLineToLogFile(line, levelLbl, lineFormatted);
        }
    }
}
