
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using Badger2018.constants;
using Badger2018.views;
using BadgerCommonLibrary.utils;
using OpenQA.Selenium.Remote;
using Application = System.Windows.Application;
using Color = System.Drawing.Color;
using MessageBox = System.Windows.Forms.MessageBox;
using Timer = System.Threading.Timer;

namespace Badger2018.utils
{


    public class MiscAppUtils
    {
        private static readonly Logger _logger = Logger.LastLoggerInstance;





        public static string GetFileNamePointageCurrentDay(DateTime refDtTime)
        {
            return "pointage-" + refDtTime.ToString("yyyy-MM-dd") + ".xml";
        }

        public static string GetFileNameScreenshot(DateTime refDtTime, String more = null)
        {

            return "screenshot-" + (more ?? "") + "-" + refDtTime.ToString("yyyy-MM-dd-HH-mm-ss") + ".png";
        }



        public static void ShowNotificationBaloon(NotifyIcon notifyIcon, string title, string text, EventHandler actionHandler, int timeoutMs = 1000, ToolTipIcon? nIcon = null, bool useAlternate = false)
        {

            if (useAlternate)
            {
                ModerNotifView.ShowNotif(title, text, timeoutMs * 2);
                return;
            }


            notifyIcon.BalloonTipIcon = nIcon ?? ToolTipIcon.None;
            notifyIcon.BalloonTipTitle = title;
            notifyIcon.BalloonTipText = text;

            if (actionHandler != null)
            {
                notifyIcon.BalloonTipClicked += actionHandler;
            }


            notifyIcon.ShowBalloonTip(timeoutMs);


#if DEBUG
            SystemSounds.Beep.Play();
            SystemSounds.Exclamation.Play();
            SystemSounds.Beep.Play();

            Console.WriteLine(title + " ::: " + text);
#endif
        }

        public static void GoTo(string text)
        {
            if (!StringUtils.IsNullOrWhiteSpace(text))
            {
                Process.Start(text);
            }
        }




        public static void CreatePaths()
        {
            if (!Directory.Exists(Cst.PointagesDirName))
            {
                Directory.CreateDirectory(Cst.PointagesDirName);

                foreach (String f in Directory.GetFiles(".").Where(r => r.Contains("pointage-201")))
                {
                    File.Move(f, Cst.PointagesDir + f);
                    _logger.Debug("Déplacement de {0} vers {1} réussi.", f, Cst.PointagesDirName);
                }
            }

            if (!Directory.Exists(Cst.ScreenshotDirName))
            {
                Directory.CreateDirectory(Cst.ScreenshotDirName);
            }
        }



        public static Task Delay(int milliseconds)
        {
            var tcs = new TaskCompletionSource<object>();
            new Timer(_ => tcs.SetResult(null)).Change(milliseconds, -1);
            return tcs.Task;
        }

        public static void RecDelayAction(Action<Task> stepAction, int numberAction, int timeoutStep, Action<Task> finalAction)
        {
            _logger.Debug("RecDelayAction");

            MiscAppUtils.Delay(0).ContinueWith(delegate
            {
                _logger.Debug("RecDelayAction.Inner");
                stepAction.Invoke(null);
                numberAction--;
                if (numberAction > 0)
                {
                    MiscAppUtils.Delay(timeoutStep).ContinueWith(delegate
                    {
                        RecDelayAction(stepAction, numberAction, timeoutStep, finalAction);
                    });
                }
                else
                {
                    finalAction.Invoke(null);
                }
                _logger.Debug("FIN - RecDelayAction.Inner");
            }
                );

            _logger.Debug("FIN - RecDelayAction");
        }
    }
}
