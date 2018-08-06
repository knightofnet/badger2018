using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using Badger2018.business;
using Badger2018.constants;
using Badger2018.dto;
using Badger2018.utils;

namespace Badger2018.views
{
    /// <summary>
    /// Logique d'interaction pour ImportExportOptionsView.xaml
    /// </summary>
    public partial class ImportExportOptionsView : Window
    {
        private static readonly Logger _logger = Logger.LastLoggerInstance;

        private bool _isImportJob;

        /// <summary>
        /// true : isImport, false : isExport
        /// </summary>
        public bool IsImportJob
        {
            get { return _isImportJob; }
            set
            {
                _isImportJob = value;
                AdaptUi(value);
            }
        }

        private AppOptions OrigOptions { get; set; }
        public AppOptions OptionImported { get; private set; }
        public string FileName { get; private set; }

        public bool HasDoneAction { get; private set; }


        public ImportExportOptionsView(AppOptions origOptions)
        {
            InitializeComponent();

            OrigOptions = origOptions;

            radioImport.Checked += RadioIeOnChecked;
            radioExport.Checked += RadioIeOnChecked;
        }

        private void RadioIeOnChecked(object sender, RoutedEventArgs routedEventArgs)
        {
            if (radioImport.IsChecked.HasValue && radioImport.IsChecked.Value)
            {
                IsImportJob = true;
            }
            else if (radioExport.IsChecked.HasValue && radioExport.IsChecked.Value)
            {
                IsImportJob = false;
            }
        }

        private void AdaptUi(bool value)
        {

            if (value)
            {
                // Import
                btnOk.Content = "Importer";
                btnOk.ToolTip =
                    "Cliquez sur le bouton pour importer les paramètres à partir du fichier XML sélectionné";

            }
            else
            {
                //Export
                btnOk.Content = "Exporter";
                btnOk.ToolTip =
                    "Cliquez sur le bouton pour exporter les paramètres vers le fichier XML sélectionné";
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            HasDoneAction = false;
            Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (FileName == null)
                {
                    FileName = tboxFilepath.Text;
                }

                if (FileName.StartsWith("\"") && FileName.EndsWith("\""))
                {
                    FileName = FileName.Trim('"');
                }

                FileInfo fi = new FileInfo(FileName);
                _logger.Info("Fichier XML : {0}", System.IO.Path.GetFullPath(fi.FullName));

                if (IsImportJob)
                {
                    _logger.Debug("import");
                    if (!fi.Exists)
                    {
                        string msg = "Le fichier xml n'existe pas. Vérifiez le chemin avant de continuer.";
                        _logger.Warn(msg);
                        MessageBox.Show(msg, "Erreur",
                            MessageBoxButton.OK, MessageBoxImage.Error);

                        tboxFilepath.Focus();
                        return;
                    }

                    OptionImported = OptionManager.LoadFromXml(FileName);

                    HasDoneAction = OptionImported != null;
                }
                else
                {
                    _logger.Debug("export");
                    if (fi.Directory != null && !fi.Directory.Exists)
                    {
                        string msg = "Le dossier devant contenir le futur fichier xml n'existe pas. Vérifiez le chemin avant de continuer.";
                        _logger.Warn(msg);
                        MessageBox.Show(
                            msg,
                            "Erreur",
                            MessageBoxButton.OK, MessageBoxImage.Error);

                        tboxFilepath.Focus();
                        return;
                    }

                    OptionManager.SaveOptionsToXml(OrigOptions, FileName);
                    HasDoneAction = true;
                    FileUtils.ShowFileInWindowsExplorer(fi.FullName);
                }

                if (HasDoneAction)
                {
                    var msg = String.Format("L'{0} a réussi.", IsImportJob ? "importation" : "exportation");
                    _logger.Info(msg);
                    MiscAppUtils.TopMostMessageBox(msg,
                        "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Erreur lors de l'{0}.", IsImportJob ? "importation" : "exportation");
                _logger.Error(ex.Message);
                _logger.Error(ex.StackTrace);

                MessageBox.Show(
                    "Une erreur s'est produite. Consulter le journal pour plus d'informations",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Close();

        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenSaveFd();
        }

        private void tboxFilepath_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenSaveFd();
        }

        private void OpenSaveFd()
        {

            if (IsImportJob)
            {
                // Create OpenFileDialog 
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
                {
                    DefaultExt = ".xml",
                    Filter = "Fichier XML (*.xml)|*.xml"
                };

                // Display OpenFileDialog by calling ShowDialog method 
                bool? result = dlg.ShowDialog();


                // Get the selected file name and display in a TextBox 
                if (result != true) return;

                // Open document 
                FileName = dlg.FileName;
                tboxFilepath.Text = FileName;
            }
            else
            {
                // Create OpenFileDialog 
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
                {
                    DefaultExt = ".xml",
                    Filter = "Fichier XML (*.xml)|*.xml"
                };

                // Display OpenFileDialog by calling ShowDialog method 
                bool? result = dlg.ShowDialog();


                // Get the selected file name and display in a TextBox 
                if (result != true) return;

                // Open document 
                FileName = dlg.FileName;
                tboxFilepath.Text = FileName;
            }


        }
    }
}
