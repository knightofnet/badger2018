using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using Badger2018.constants;
using Badger2018.dto;
using Badger2018.views;
using BadgerPluginExtender;
using BadgerPluginExtender.dto;
using BadgerPluginExtender.interfaces;
using BetaUserAddons.Properties;

namespace BetaUserAddons
{
    public class BetaUserAddonsClass : IGenericPluginInterface
    {
        private static readonly Logger _logger = Logger.LastLoggerInstance;
        public PluginInfo CurrPluginInfo { get; private set; }

        private bool _isloaded = false;

        public BetaUserAddonsClass()
        {
            CurrPluginInfo = new PluginInfo("BetaUserAddons", Assembly.GetAssembly(GetType()).GetName().Version);

            _isloaded = true;
        }



        public PluginInfo GetPluginInfo()
        {
        
            return CurrPluginInfo;

        }

        public MethodRecord[] GetMethodToRecords()
        {
            MethodRecord m = new MethodRecord();
            m.TargetHookName = "TestHooName";
            m.MethodResponder = "TestMethod";

            MethodRecord t = new MethodRecord();
            t.TargetHookName = "IsBetaUser";
            t.MethodResponder = "IsBetaUser";

            MethodRecord v = new MethodRecord();
            v.TargetHookName = "FullOnClockUpdTimerOnOnTick";
            v.MethodResponder = "BadgeBetaTest";

            return new MethodRecord[] { m, t, v };
        }

        public void OnOptionsViewInitHandler(OptionsView optView)
        {
            // optView.
        }


        public void TestMethod(bool iss)
        {
            Console.WriteLine("TEST");
            Console.WriteLine("TEST");
            Console.WriteLine("TEST");
            Console.WriteLine("TEST");
            Console.WriteLine("TEST");
            Console.WriteLine("TEST");
            Console.WriteLine("TEST");
        }

        public bool IsBetaUser()
        {
            return StringUtils.CsvStringContains(Environment.UserName.ToUpper(),
                ((string)Settings.Default["specUser"]).ToUpper());


        }




        public void BadgeBetaTest(TimesBadgerDto times, AppOptions prgOptions, AppSwitchs prgSwitch, int etatBadger, RealTimesObj realTimeObj, EnumTypesJournees typeJournees)
        {
            if (prgSwitch.IsBetaUser
               && prgOptions.IsAutoBadgeMeridienne
               && etatBadger == 1
               && !prgSwitch.IsAutoBadgeage
               && realTimeObj.RealTimeTsNow.CompareTo(times.PlageTravMatin.EndOrDft.TimeOfDay + prgOptions.TempsMinPause + new TimeSpan(0, 0, prgOptions.DeltaAutoBadgeageMinute)) >= 0)
            {
                prgSwitch.IsAutoBadgeage = true;

                _logger.Debug("Spec: Autobadgeage");

                CoreAppBridge.Instance.PlayHook("ExtSetBtnBadger", new object[] { false });
                //btnBadger.IsEnabled = false;
                CoreAppBridge.Instance.PlayHook("BadgeFullAction", new object[] { true, true });

                CoreAppBridge.Instance.PlayHook("ExtSetBtnBadger", new object[] { false });
                //btnBadger.IsEnabled = true;

                if (prgOptions.IsDailyDisableAutoBadgeMerid)
                {
                    prgOptions.IsAutoBadgeMeridienne = false;

                    CoreAppBridge.Instance.PlayHook("ExtSaveOptions", new object[] { prgOptions });
                    //OptionManager.SaveOptions(PrgOptions);

                    _logger.Debug("Spec: Autobadgeage désactivé par IsDailyDisableAutoBadgeMerid");
                }
            }
        }
    }
}
