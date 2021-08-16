using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using AryxDevLibrary.extensions;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using Badger2018.constants;
using Badger2018.utils;
using BadgerCommonLibrary.constants;
using BadgerCommonLibrary.utils;
using BadgerPluginExtender.dto;
using BadgerPluginExtender.interfaces;

namespace Badger2018.views
{


    /// <summary>
    /// Logique d'interaction pour DebugCommandView.xaml
    /// </summary>
    public partial class DebugCommandView : Window
    {
        private static Logger _logger = Logger.GetInstance(CommonCst.MainLogName);
        private bool isSyncLog;

        private String modeCmd = null;
        private MainWindow _pWinRef;

        private readonly List<String> _cmdHistory = new List<string>();
        private int _cmdHistoryIndex;

        DispatcherTimer _showTimesDispatcher = null;
        DispatcherTimer _showAppDateTimeDispatcher = null;
        private Run runForShowtime;
        private Run runForAppDateTime;


        public DebugCommandView(MainWindow pWin)
        {
            InitializeComponent();

            _pWinRef = pWin;


            richTxtBox.Document.Blocks.Clear();
            CsLog("N'oubliez pas : un grand pouvoir implique de grandes responsabilités.", Cst.SCBDarkGreen);

            Closing += (sender, args) =>
            {
                if (isSyncLog)
                {
                    _logger.OnLogging -= SyncLogging;
                }

                _pWinRef = null;
                if (_showTimesDispatcher != null)
                {
                    _showTimesDispatcher.Stop();
                    _showTimesDispatcher = null;
                }

                if (_showAppDateTimeDispatcher != null)
                {
                    _showAppDateTimeDispatcher.Stop();
                    _showAppDateTimeDispatcher = null;
                }

            };


            tbox.Focus();

        }



        private bool DoSomething(String[] args)
        {
            string textInput = args[0];

            if (StringUtils.IsNullOrWhiteSpace(textInput)) return true;

            if ("help".Equals(textInput, StringComparison.CurrentCultureIgnoreCase))
            {
                CsLog("Commandes :");
                CsLog(" archiveData : archives les captures, les journaux et les anciens pointages xml.");
                CsLog(" calcCd : calcul le c/D manuellement.");
                CsLog(" cls : efface la console.");
                CsLog(" log : ouvre le fichier journal.");
                CsLog(" folder : ouvre le dossier de l'application dans l'explorateur.");
                CsLog(" help : affiche les commandes disponibles.");
                CsLog(" isOkWebResponse : test la réponse web à une URL.");
                CsLog(" listPlugins : affiche le noms des extensions.");
                CsLog(" modeDate : bascule dans le mode de modification de dates.");
                CsLog(" modePlugin : bascule vers le mode d'une extension.");
                CsLog(" notify : affiche une notification. Le titre est facultatif. Exemples : notify \"Ceci un message\" \"Titre\".");
                CsLog(" quit : quitte Badger2018.");
                CsLog(" startSyncLog : affiche les log dans la console de debug.");
                CsLog(" stopSyncLog : affiche les log dans la console de debug.");
                CsLog(" stopTimer : arrête les timers de l'application.");
                CsLog(" showLicence : affiche la licence de l'application.");
                CsLog(" showRealTimes : affiche les temps en direct.");
                CsLog(" tick : force l'exécution d'un tick.");



            }
            else if ("modeDate".Equals(textInput, StringComparison.CurrentCultureIgnoreCase))
            {
                CsLog("Mode de modification de dates : ");
                SetCsTextForShowAppDateTime();
                modeCmd = "modDate";

            }
            else if ("modePlugin".Equals(textInput, StringComparison.CurrentCultureIgnoreCase))
            {
                if (args.Length == 1)
                {
                    CsLog("Nom de l'extension manquant. Utilisez la syntaxe suivante : modePlugin NOM_DU_PLUGIN. Voir listPlugins.", Cst.SCBDarkRed);
                    return false;
                }
                string pluginName = args[1].Trim('"');
                if (_pWinRef.PluginMgr.PluginsInstance.ContainsKey(pluginName)
                    && _pWinRef.PluginMgr.PluginsInstance[pluginName].GetMethodToRecords().Any(r => r.TargetHookName.Equals("CsDoSomething")))
                {
                    CsLog("Bascule vers le mode de l'extension " + pluginName);
                    modeCmd = "ext" + pluginName;
                }



            }
            else if ("cls".Equals(textInput, StringComparison.CurrentCultureIgnoreCase))
            {
                richTxtBox.Document.Blocks.Clear();
            }
            else if ("archiveData".Equals(textInput, StringComparison.CurrentCultureIgnoreCase))
            {
                CsLog("Archives les captures, les journaux et les anciens pointages xml");
                MiscAppUtils.CreatePaths();
                _pWinRef.ArchiveDatas();
            }
            else if ("calcCd".Equals(textInput, StringComparison.CurrentCultureIgnoreCase))
            {
                CsLog("calcul le c/D manuellement");
                CalculateCDView c = new CalculateCDView(_pWinRef.PrgOptions);
                c.ShowDialog();
            }
            else if ("log".Equals(textInput, StringComparison.CurrentCultureIgnoreCase))
            {
                CsLog("Ouverture du fichier de log (dernière instance) : " + Logger.LastLoggerInstance.FileLog.FullName);
                ProcessUtils.GoTo(Logger.LastLoggerInstance.FileLog.FullName);
            }
            else if ("folder".Equals(textInput, StringComparison.CurrentCultureIgnoreCase))
            {
                CsLog("Ouverture de l'explorateur : " + Cst.ApplicationDirectory);
                FileUtils.ShowFileInWindowsExplorer(Cst.ApplicationDirectory);
            }
            else if ("startSyncLog".Equals(textInput, StringComparison.CurrentCultureIgnoreCase))
            {
                if (isSyncLog) return true;
                _logger.OnLogging += SyncLogging;
                isSyncLog = true;
            }
            else if ("isOkWebResponse".Equals(textInput, StringComparison.CurrentCultureIgnoreCase))
            {
                String url = args.Length >= 2 ? args[1].Trim('"') : _pWinRef.PrgOptions.Uri;
                String cdRep = args.Length >= 3 ? args[2].Trim('"') : "200";

                Int32.TryParse(cdRep, out int outIntCdRep);

                CsLog("isOkWebResponse(url:"+ url+", cdRep: "+outIntCdRep+") :");
                CsLog("> "+BadgingUtils.IsValidWebResponse(url, outIntCdRep));

            }
            else if ("stopSyncLog".Equals(textInput, StringComparison.CurrentCultureIgnoreCase))
            {
                if (!isSyncLog) return true;
                _logger.OnLogging -= SyncLogging;
                isSyncLog = false;
            }
            else if ("quit".Equals(textInput, StringComparison.CurrentCultureIgnoreCase))
            {
                _logger.Info("Fin du programme demandée dans l'interface debug");
                _pWinRef.PrgSwitch.IsRealClose = true;
                _pWinRef.Close();
            }
            else if ("showRealTimes".Equals(textInput, StringComparison.CurrentCultureIgnoreCase))
            {
                CsLog("Temps à chaque tick (maj auto) :");
                SetCsTextForShowTimes();
            }
            else if ("showLicence".Equals(textInput, StringComparison.CurrentCultureIgnoreCase))
            {

                String message = String.Format("Badger2018 est activé.{0}{0}Type de licence : {1}.{0}Attribuée à : {2}{0}{0}Date d'expiration : {3}",
                        "",
                        _pWinRef.LicenceApp.TypeUser == 0 ? "ambassadeur" : "utilisateur",
                        _pWinRef.LicenceApp.NiceName.Trim(),
                        _pWinRef.LicenceApp.TypeUser == 0 ? "validitée perpétuelle" : _pWinRef.LicenceApp.DateExpiration.ToShortDateString()

                    );
                CsLog(message);

            }
            else if ("stopTimer".Equals(textInput, StringComparison.CurrentCultureIgnoreCase))
            {
                CsLog("Arrêt des timers");
                _pWinRef.StopTimers();
            }
            else if ("notify".Equals(textInput, StringComparison.CurrentCultureIgnoreCase))
            {
                String message = args.Length >= 2 ? args[1].Trim('"') : "Test notification";
                String title = args.Length >= 3 ? args[2].Trim('"') : "Badger2018";
                CsLog("Nouvelle notification :");
                CsLog(" message : " + message);
                CsLog(" titre : " + title);

                _pWinRef.NotifManager.NotifyNow(message, title);

            }
            else if ("tick".Equals(textInput, StringComparison.CurrentCultureIgnoreCase))
            {
                CsLog("Tick !");
                _pWinRef.Tick();

            }
            else if ("listPlugins".Equals(textInput, StringComparison.CurrentCultureIgnoreCase))
            {
                if (_pWinRef.PluginMgr.IsAnyPluginLoaded)
                {
                    CsLog("Extensions chargées : ");
                    foreach (KeyValuePair<string, IGenericPluginInterface> plugin in _pWinRef.PluginMgr.PluginsInstance)
                    {
                        CsLog(plugin.Value.GetPluginInfo().Name + "-" + plugin.Value.GetPluginInfo().Version);
                    }
                }
                else
                {
                    CsLog("Aucune extension chargée.");
                }


            }
            else
            {
                CsLog(String.Format("'{0}' n'est pas reconnu en tant que commande interne.", string.Join(" ", args)), Cst.SCBDarkRed);
            }


            return true;

        }



        private bool DoSomethingModDate(String[] args)
        {
            DateTime newDateTime = new DateTime();

            string textInput = args[0];

            if ("help".Equals(textInput, StringComparison.CurrentCultureIgnoreCase))
            {
                CsLog("Commandes :");
                CsLog(" back : bascule dans le mode normal de la console.");
                CsLog(" current : place la date de l'application dans l'invite de la console.");
                CsLog(" set : change l'heure et la date de l'application.");
                CsLog(" reset : l'heure et la date de l'application est celle du système.");


            }
            else if ("cls".Equals(textInput, StringComparison.CurrentCultureIgnoreCase))
            {
                richTxtBox.Document.Blocks.Clear();
            }
            else if ("current".Equals(textInput, StringComparison.CurrentCultureIgnoreCase))
            {
                string dateStr = AppDateUtils.DtNow().ToString("g");
                CsLog("Date de l'application :");
                CsLog(" " + dateStr);
                tbox.Text = dateStr;
                return false;
            }
            else if ("reset".Equals(textInput, StringComparison.CurrentCultureIgnoreCase))
            {

                AppDateUtils.ForceDtNow(null);
            }
            else if ("set".Equals(textInput, StringComparison.CurrentCultureIgnoreCase))
            {
                if (DateTime.TryParse(args[1].Trim('"'), out newDateTime))
                {
                    AppDateUtils.ForceDtNow(newDateTime);
                    CsLog("Date de l'application forcée à : " + newDateTime.ToString("g"));
                    return false;
                }
                CsLog("Format de date non reconnu", Cst.SCBDarkRed);
            }

            else
            {
                CsLog(String.Format("'{0}' n'est pas reconnu en tant que commande interne.", string.Join(" ", args)), Cst.SCBDarkRed);
            }


            return true;
        }

        private void SetCsTextForShowTimes()
        {
            if (_showTimesDispatcher != null)
            {
                _showTimesDispatcher.Stop();
                runForShowtime = null;
            }

            _showTimesDispatcher = new DispatcherTimer();
            _showTimesDispatcher.Tick += (sender, eventArgs) =>
            {
                const string tpl = "Heure actuelle : {0}\n" +
                             "Temps travaillé : {1}\n" +
                             "Temps travaillé matin : {2}\n" +
                             "Temps réel (heure) : {3}\n" +
                             "Tps trav restant min : {4}\n" +
                             "Tps trav restant max : {5}";
                if (runForShowtime == null)
                {
                    runForShowtime = new Run { FontFamily = new FontFamily("Consolas") };

                    Paragraph txtBlock = new Paragraph(runForShowtime) { Margin = new Thickness(0, 0, 0, 0) };

                    richTxtBox.Document.Blocks.Add(txtBlock);
                }
                runForShowtime.Text = String.Format(tpl,
                    _pWinRef.RealTimes.RealTimeDtNow.ToString("g"),
                    _pWinRef.RealTimes.RealTimeTempsTravaille.ToString("g"),
                    _pWinRef.RealTimes.RealTimeTempsTravailleMatin.ToString("g"),
                    _pWinRef.RealTimes.RealTimeTsNow.ToString(Cst.TimeSpanFormat),
                    _pWinRef.RealTimes.RealTimeMinTpsTravRestant.ToString(Cst.TimeSpanFormat),
                    _pWinRef.RealTimes.RealTimeMaxTpsTravRestant.ToString(Cst.TimeSpanFormat)
                    );
            };
            _showTimesDispatcher.Interval = new TimeSpan(0, 0, 1);
            _showTimesDispatcher.Start();
        }

        private void SetCsTextForShowAppDateTime()
        {
            if (_showAppDateTimeDispatcher != null)
            {
                _showAppDateTimeDispatcher.Stop();
                runForAppDateTime = null;
            }

            _showAppDateTimeDispatcher = new DispatcherTimer();
            _showAppDateTimeDispatcher.Tick += (sender, eventArgs) =>
            {


                const string tpl = " Date système     : {0}\n" +
                                   " Date application : {1}\n";
                if (runForAppDateTime == null)
                {
                    runForAppDateTime = new Run { FontFamily = new FontFamily("Consolas") };

                    Paragraph txtBlock = new Paragraph(runForAppDateTime) { Margin = new Thickness(0, 0, 0, 0) };

                    richTxtBox.Document.Blocks.Add(txtBlock);
                }
                runForAppDateTime.Text = String.Format(tpl,
                    DateTime.Now.ToString("g"),
                    AppDateUtils.DtNow().ToString("g")
                    );
            };
            _showAppDateTimeDispatcher.Interval = new TimeSpan(0, 0, 1);
            _showAppDateTimeDispatcher.Start();
        }

        private void SyncLogging(string lines, string levelLbl, string lineFormatted)
        {
            if (levelLbl.Trim().Equals("INFO"))
            {
                Dispatcher.BeginInvoke(new Action(() => { CsLog(lineFormatted, new SolidColorBrush(Colors.Aqua)); }));

            }
            else if (levelLbl.Trim().Equals("WARN"))
            {
                Dispatcher.BeginInvoke(new Action(() => { CsLog(lineFormatted, new SolidColorBrush(Colors.DarkOrange)); }));
            }
            else if (levelLbl.Trim().Equals("ERROR"))
            {
                Dispatcher.BeginInvoke(new Action(() => { CsLog(lineFormatted, new SolidColorBrush(Colors.DarkRed)); }));
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(() => { CsLog(lineFormatted); }));
            }
        }

        private void tbox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                String textInput = tbox.Text == null ? "" : tbox.Text.Trim();


                _cmdHistory.Add(textInput);
                _cmdHistoryIndex = _cmdHistory.Count;

                CsLog(">" + textInput);

                String[] args = CutString(textInput);

                bool isClearInput = true;
                try
                {
                    if (modeCmd == null)
                    {
                        isClearInput = DoSomething(args);
                    }
                    else if ("back".Equals(textInput, StringComparison.CurrentCultureIgnoreCase))
                    {
                        modeCmd = null;
                    }
                    else if ("modDate".Equals(modeCmd))
                    {
                        isClearInput = DoSomethingModDate(args);
                    }
                    else if (modeCmd.StartsWith("ext"))
                    {
                        isClearInput = ExtDoSomething(args);
                    }
                }
                catch (Exception ex)
                {
                    CsLog(ex.Message, Cst.SCBDarkRed);
                    CsLog(ex.StackTrace, Cst.SCBDarkRed);
                    BadgerCommonLibrary.utils.ExceptionHandlingUtils.LogAndHideException(ex);
                }

                if (isClearInput)
                    tbox.Text = String.Empty;
            }
            else if (e.Key == Key.Up)
            {
                _cmdHistoryIndex -= 1;
                if (_cmdHistoryIndex < 0)
                {
                    _cmdHistoryIndex = 0;
                }
                tbox.Text = _cmdHistory[_cmdHistoryIndex];
                tbox.CaretIndex = tbox.Text.Length;
            }
            else if (e.Key == Key.Down)
            {
                _cmdHistoryIndex += 1;
                if (_cmdHistoryIndex >= _cmdHistory.Count)
                {
                    _cmdHistoryIndex = _cmdHistory.Count - 1;
                }
                tbox.Text = _cmdHistory[_cmdHistoryIndex];
                tbox.CaretIndex = tbox.Text.Length;
            }
            else if (e.Key == Key.Escape)
            {
                _cmdHistoryIndex = _cmdHistory.Count - 1;
                tbox.Text = String.Empty;

            }
        }

        private bool ExtDoSomething(string[] args)
        {

            string extName = modeCmd.Substring(3);
            ObservableCollection<Dictionary<String, object>> obsCsExt = new ObservableCollection<Dictionary<String, object>>();
            obsCsExt.CollectionChanged += (o, eventArgs) =>
            {
                if (eventArgs.Action == NotifyCollectionChangedAction.Add)
                {
                    Dictionary<String, object> item = eventArgs.NewItems[0] as Dictionary<String, object>;
                    if (item == null || !item.ContainsKey("message")) return;

                    string message = item["message"] as string;
                    SolidColorBrush color = item["color"] as SolidColorBrush;

                    CsLog(message, color);
                }
            };
            HookReturns retHook = _pWinRef.PluginMgr.PlayHookPluginAndReturn(extName, "CsDoSomething",
                new object[] { args, obsCsExt }, typeof(bool));
            bool isClearInput = (bool)retHook.ReturnFirstOrDefaultResultObject();
            return isClearInput;
        }

        private string[] CutString(string textInput)
        {
            List<String> retList = new List<string>();
            StringBuilder sb = new StringBuilder();
            bool isQuote = false;
            foreach (char c in textInput)
            {
                if (c == '"')
                {
                    sb.Append(c);
                    isQuote = !isQuote;
                }
                else if (c == ' ' && !isQuote)
                {
                    retList.Add(sb.ToString());
                    sb.Clear();
                }
                else
                {
                    sb.Append(c);
                }
            }
            retList.Add(sb.ToString());

            return retList.ToArray();
        }


        private void CsLog(string text, SolidColorBrush color = null, bool isAddtoStack = true)
        {
            Run runP = new Run(text);

            Paragraph txtBlock = new Paragraph(runP);
            runP.FontFamily = new FontFamily("Consolas");
            txtBlock.Margin = new Thickness(0, 0, 0, 0);

            SolidColorBrush locColor = Cst.SCBBlack;
            if (color != null)
            {
                locColor = color;
            }
            txtBlock.Foreground = locColor;

            if (isAddtoStack)
            {
                richTxtBox.Document.Blocks.Add(txtBlock);
                richTxtBox.ScrollToEnd();
            }
        }
    }
}
