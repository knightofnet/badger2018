
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
        public static bool CsvStringContains(string toSearch, string csvSepString)
        {
            string[] splitted = csvSepString.Split(';');
            return splitted.Any<string>(r => r.Equals(toSearch));
        }


        public static ImageSource DoGetImageSourceFromResource(string psAssemblyName, string psResourceName)
        {
            Uri oUri = new Uri(@"pack://application:,,,/" + psAssemblyName + ";component/Resources/" + psResourceName, UriKind.RelativeOrAbsolute);

            return new BitmapImage(oUri);
        }

        public static Icon DoGetIconSourceFromResource(string psAssemblyName, string psResourceName)
        {
            Uri oUri = new Uri(@"pack://application:,,,/" + psAssemblyName + ";component/Resources/" + psResourceName, UriKind.RelativeOrAbsolute);

            return new Icon(Application.GetResourceStream(oUri).Stream);
        }

        public static string GetFileNamePointageCurrentDay()
        {
            return "pointage-" + AppDateUtils.DtNow().ToString("yyyy-MM-dd") + ".xml";
        }

        public static string GetFileNameScreenshot(String more = null)
        {

            return "screenshot-" + (more ?? "") + "-" + AppDateUtils.DtNow().ToString("yyyy-MM-dd-HH-mm-ss") + ".png";
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

        public static MessageBoxResult TopMostMessageBox(String msg, String title, MessageBoxButton buttonParam,
            MessageBoxImage iconParam)
        {
            MessageBoxButtons button = MessageBoxButtons.OK;
            switch (buttonParam)
            {
                case MessageBoxButton.OK:
                    button = MessageBoxButtons.OK;
                    break;
                case MessageBoxButton.OKCancel:
                    button = MessageBoxButtons.OKCancel;
                    break;
                case MessageBoxButton.YesNo:
                    button = MessageBoxButtons.YesNo;
                    break;
                case MessageBoxButton.YesNoCancel:
                    button = MessageBoxButtons.YesNoCancel;
                    break;
            }

            MessageBoxIcon icon = MessageBoxIcon.None;
            switch (iconParam)
            {
                case MessageBoxImage.Asterisk:
                    icon = MessageBoxIcon.Asterisk;
                    break;
                case MessageBoxImage.Error:
                    icon = MessageBoxIcon.Error;
                    break;
                /* case MessageBoxImage.Exclamation:
                     icon = MessageBoxIcon.Exclamation;
                     break;*/
                /*case MessageBoxImage.Hand:
                    icon = MessageBoxIcon.Hand;
                    break;*/
                /* case MessageBoxImage.Information:
                     icon = MessageBoxIcon.Information;
                     break;*/
                case MessageBoxImage.None:
                    icon = MessageBoxIcon.None;
                    break;
                case MessageBoxImage.Question:
                    icon = MessageBoxIcon.Question;
                    break;
                /*case MessageBoxImage.Stop:
                    icon = MessageBoxIcon.Stop;
                    break;*/
                case MessageBoxImage.Warning:
                    icon = MessageBoxIcon.Warning;
                    break;
                default:
                    icon = MessageBoxIcon.None;
                    break;

            }

            return TopMostMessageBox(msg, title, button, icon);

        }

        [Obsolete("Not recommended direct usage.")]
        public static MessageBoxResult TopMostMessageBox(String msg, String title, MessageBoxButtons button, MessageBoxIcon icon)
        {
            var r = MessageBox.Show(new Form { TopMost = true },
                    msg,
                    title,
                    button,
                    icon
                    );

            switch (r)
            {
                case DialogResult.Cancel:
                    return MessageBoxResult.Cancel;

                case DialogResult.No:
                    return MessageBoxResult.No;

                case DialogResult.OK:
                    return MessageBoxResult.OK;

                case DialogResult.None:
                    return MessageBoxResult.None;

                case DialogResult.Yes:
                    return MessageBoxResult.Yes;
            }

            return MessageBoxResult.None;
        }

        public static Task Delay(int milliseconds)
        {
            var tcs = new TaskCompletionSource<object>();
            new Timer(_ => tcs.SetResult(null)).Change(milliseconds, -1);
            return tcs.Task;
        }

        public static void RecDelayAction(Action<Task> stepAction, int numberAction, int timeoutStep, Action<Task> finalAction)
        {
            MiscAppUtils.Delay(0).ContinueWith(delegate
            {
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
            }
                );

        }
    }
}
