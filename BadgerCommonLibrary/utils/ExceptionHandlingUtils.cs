using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using AryxDevLibrary.utils.logger;

namespace BadgerCommonLibrary.utils
{
    public class ExceptionHandlingUtils
    {

        private static readonly Logger _logger = Logger.LastLoggerInstance;

        public static bool ShowStackTrace = true;

        public static void LogAndRethrows(Exception ex, string moreMsg = null)
        {
            _logger.Error("Erreur :");
            if (moreMsg != null)
            {
                _logger.Error(moreMsg);
            }
            _logger.Error("Type d'exception : {0}, Message : {1}", ex.GetType().Name, ex.Message);

            if (ShowStackTrace)
            {
                _logger.Error("Stack Trace : {0}", ex.StackTrace);
            }
            throw ex;
        }




        public static void LogAndHideException(Exception ex, string moreMsg = null, bool isWarnMsgAndDebugStack = false)
        {
            if (!isWarnMsgAndDebugStack)
            {
                _logger.Error("Erreur :");
            }
            else
            {
                _logger.Warn("Erreur :");
            }

            if (moreMsg != null)
            {
                if (!isWarnMsgAndDebugStack)
                {
                    _logger.Error(moreMsg);
                }
                else
                {
                    _logger.Warn(moreMsg);
                }
            }

            if (!isWarnMsgAndDebugStack)
            {

                _logger.Error("Type d'exception : {0}, Message : {1}", ex.GetType().Name, ex.Message);
                if (ShowStackTrace)
                {
                    _logger.Error("Stack Trace : {0}", ex.StackTrace);
                }
            }
            else
            {
                _logger.Debug("Type d'exception : {0}, Message : {1}", ex.GetType().Name, ex.Message);
                _logger.Debug("Stack Trace : {0}", ex.StackTrace);
            }




        }

        public static void LogAndEndsProgram(Exception ex, int exitCode, string moreMsg = null)
        {
            _logger.Error("Erreur fatale :");
            if (moreMsg != null)
            {
                _logger.Error(moreMsg);
            }
            _logger.Error("Type d'exception : {0}, Message : {1}", ex.GetType().Name, ex.Message);
            if (ShowStackTrace)
            {
                _logger.Error("Stack Trace : {0}", ex.StackTrace);
            }

            Environment.Exit(exitCode);
        }

        public static void LogAndNewException(string moreMsg, Exception innerException = null)
        {
            _logger.Error("Erreur :");
            _logger.Error(moreMsg);
            throw new Exception(moreMsg, innerException);

        }
    }
}
