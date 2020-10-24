using System;
using System.Windows;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using BadgerCommonLibrary.utils;

namespace Badger2018.views
{
    /// <summary>
    /// Logique d'interaction pour DebugCommandView.xaml
    /// </summary>
    public partial class DebugCommandView : Window
    {
        public DebugCommandView()
        {
            InitializeComponent();

            
           
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            String textInput = tbox.Text;
            DateTime newTboxTmin = new DateTime();

            if (StringUtils.IsNullOrWhiteSpace(textInput)) return;

            if ("log".Equals(textInput, StringComparison.CurrentCultureIgnoreCase))
            {
                ProcessUtils.GoTo(Logger.LastLoggerInstance.FileLog.FullName);
            } else if ("folder".Equals(textInput, StringComparison.CurrentCultureIgnoreCase))
            {
                FileUtils.ShowFileInWindowsExplorer(Logger.LastLoggerInstance.FileLog.DirectoryName);
            }
            else if ("modDate".Equals(textInput, StringComparison.CurrentCultureIgnoreCase))
            {
                tbox.Text = AppDateUtils.DtNow().ToString();
            }
            else if ("cleardate".Equals(textInput, StringComparison.CurrentCultureIgnoreCase))
            {
                AppDateUtils.ForceDtNow(null);
            }
            else if (DateTime.TryParse(tbox.Text, out newTboxTmin))
            {
                AppDateUtils.ForceDtNow(newTboxTmin);

            }
            
        }
    }
}
