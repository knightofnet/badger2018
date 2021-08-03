
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using AryxDevLibrary.extensions;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using Badger2018.constants;
using Badger2018.views;
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


            }
            foreach (String f in Directory.GetFiles(".").Where(r => r.Contains("pointage-201")))
            {
                File.Move(f, Cst.PointagesDir + f);
                _logger.Debug("Déplacement de {0} vers {1} réussi.", f, Cst.PointagesDirName);
            }

            if (!Directory.Exists(Cst.ScreenshotDirName))
            {
                Directory.CreateDirectory(Cst.ScreenshotDirName);
            }

            if (!Directory.Exists(Cst.LogArchiveDirName))
            {
                Directory.CreateDirectory(Cst.LogArchiveDirName);

            }
            foreach (String f in Directory.GetFiles(".").Where(r => r.Contains("log.log.2")))
            {
                File.Move(f, Cst.LogArchiveDir + f);
                _logger.Debug("Déplacement de {0} vers {1} réussi.", f, Cst.LogArchiveDirName);
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

            Delay(0).ContinueWith(delegate
            {
                _logger.Debug("RecDelayAction.Inner");
                stepAction.Invoke(null);
                numberAction--;
                if (numberAction > 0)
                {
                    Delay(timeoutStep).ContinueWith(delegate
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

        public static string TimeSpanShortStrFormat(TimeSpan ts, bool showSeconds = false)
        {
            if (ts.Hours > 0 || ts.Hours <= -1)
            {

                return String.Format("{0}{1}", ts.TotalSeconds < 0 ? "-" : "", ts.ToString(Cst.TimeSpanFormatWithH));
            }

            if (showSeconds && ts.TotalSeconds < 60)
            {
                return String.Format("{0}s", ts.TotalSeconds.ToString("##"));
            }

            return String.Format("{0}min", ts.Minutes);
        }

        public static bool TryParseAlt(string text, out TimeSpan newTboxPfAS)
        {
            if (TimeSpan.TryParse(text, out newTboxPfAS))
            {
                return true;
            }

            if (TimeSpan.TryParseExact(text, new string[] {Cst.TimeSpanFormat, Cst.TimeSpanFormatWithH},
                CultureInfo.InvariantCulture,
                TimeSpanStyles.None,
                out newTboxPfAS))
            {
                return true;
            }

            if (text.Length == 4 && text.Matches("\\d{4}"))
            {
                return TryParseAlt(text.Substring(0, 2) + ":" + text.Substring(2), out newTboxPfAS);
            }

            return false;

        }

        public static double EcartType(List<double> t)
        {
            double moyenne = Moyenne(t);
            double somme = 0.0;
            for (int i = 0; i < t.Count; i++)
            {
                double delta = t[i] - moyenne;
                somme += delta * delta;
            }
            return Math.Sqrt(somme / (t.Count - 1));
        }

        public static double Moyenne(List<double> lstDec)
        {
            double sumlstDec = lstDec.Sum(x => x);
            double moy = sumlstDec / lstDec.Count;

            return moy;
        }

        internal static Color Opacify(double v, Color color)
        {
            Color c = new Color();
            c.R = color.R;
            c.G = color.G;
            c.B = color.B;
            c.A = Convert.ToByte(v * 255);

            return c;
        }
    }
}
