using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using Badger2018.utils;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Badger2018.dto
{
    class FfDriverSingleton
    {
        private static readonly Logger _logger = Logger.LastLoggerInstance;

        private static FfDriverSingleton _instance;

        public static FfDriverSingleton Instance {
            get
            {
                if (_instance == null)
                {
                    _instance = new FfDriverSingleton();
                }

                return _instance;
            }

            private set {
                // nothing
            }
        }

        public RemoteWebDriver FfDriver { get; private set; }

        public RemoteWebDriver GetWebDriver()
        {
            if (IsLoaded())
            {
                return FfDriver;
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool IsLoaded()
        {
            if( FfDriver != null )
            {
                try
                {
                    String no = FfDriver.Title;
                    return true;
                } catch(WebDriverException e) {

                    ExceptionHandlingUtils.Logger = _logger;
                    ExceptionHandlingUtils.LogAndHideException(e, "Erreur lors du test d'activité de FfDriver :");
               
                    FfDriver.Quit();
                    FfDriver = null;
                    return false;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Load(AppOptions appOptions)
        {
            if (!IsLoaded()) {
                FfDriver = BadgingUtils.GetWebDriver(appOptions);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        internal void Quit()
        {
            if (!IsLoaded()) return;

            FfDriver.Quit();
            
            FfDriver = null;
            
        }
    }
}
