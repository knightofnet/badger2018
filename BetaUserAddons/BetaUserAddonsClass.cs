using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using Badger2018.dto;
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

        public BetaUserAddonsClass()
        {
            CurrPluginInfo = new PluginInfo("BetaUserAddons", Assembly.GetAssembly(GetType()).GetName().Version);


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
            v.TargetHookName = "BadgeBetaTest";
            v.MethodResponder = "BadgeBetaTest";

            return new MethodRecord[] { m, t, v };
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




        public void BadgeBetaTest(TimesBadgerDto Times, AppOptions PrgOptions, AppSwitchs PrgSwitch, int EtatBadger, TimeSpan RealTimeTsNow)
        {
            if (PrgSwitch.IsBetaUser
               && PrgOptions.IsAutoBadgeMeridienne
               && EtatBadger == 1
               && !PrgSwitch.IsAutoBadgeage

               // TODO RND A refaire
                //&& RealTimeTsNow.CompareTo(Times.PlageTravMatin.EndOrDft.TimeOfDay + PrgOptions.TempsMinPause + new TimeSpan(0, 0, SpecDelayMeridAutoBadgage)) >= 0)

                && RealTimeTsNow.CompareTo(Times.PlageTravMatin.EndOrDft.TimeOfDay + PrgOptions.TempsMinPause) >= 0)
            {
                PrgSwitch.IsAutoBadgeage = true;

                _logger.Debug("Spec: Autobadgeage");

                CoreAppBridge.Instance.PlayHook("ExtSetBtnBadger", new object[] { false });
                //btnBadger.IsEnabled = false;
                CoreAppBridge.Instance.PlayHook("BadgeFullAction", new object[] { true });

                CoreAppBridge.Instance.PlayHook("ExtSetBtnBadger", new object[] { false });
                //btnBadger.IsEnabled = true;

                if (PrgOptions.IsDailyDisableAutoBadgeMerid)
                {
                    PrgOptions.IsAutoBadgeMeridienne = false;

                    CoreAppBridge.Instance.PlayHook("ExtSaveOptions", new object[] { PrgOptions });
                    //OptionManager.SaveOptions(PrgOptions);

                    _logger.Debug("Spec: Autobadgeage désactivé par IsDailyDisableAutoBadgeMerid");
                }
            }
        }
    }
}
