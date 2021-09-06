using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
using IWshRuntimeLibrary;
using File = System.IO.File;

namespace Badger2018.views
{
    /// <summary>
    /// Logique d'interaction pour CreateShortcutsView.xaml
    /// </summary>
    public partial class CreateShortcutsView : Window
    {
        private static readonly string _exePath;

        public CreateShortcutsView()
        {
            InitializeComponent();

            InitShortcutsAlreadyExists();
        }

        static CreateShortcutsView()
        {
            _exePath = Assembly.GetExecutingAssembly().Location;
        }

        private void InitShortcutsAlreadyExists()
        {
            // StartupFolder 
            chkShortcutStartFolder.IsChecked = ExistsShortcut(_exePath, Environment.SpecialFolder.Startup);

            // StartMenuFolder 
            chkShortcutStartMenu.IsChecked = ExistsShortcut(_exePath, Environment.SpecialFolder.StartMenu);

            // Desktop 
            chkShortcutBureau.IsChecked = ExistsShortcut(_exePath, Environment.SpecialFolder.Desktop);

            chkShortcutBureauNoAuto.IsChecked = ExistsShortcut(_exePath, Environment.SpecialFolder.Desktop, "-n");

        }



        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            bool isOkForStartFolder = chkShortcutStartFolder.IsChecked ?? false;
            bool isOkForStartMenu = chkShortcutStartMenu.IsChecked ?? false;
            bool isOkForBureau = chkShortcutBureau.IsChecked ?? false;
            bool isOkForBureauSpec = chkShortcutBureauNoAuto.IsChecked ?? false;

            if (!isOkForStartFolder && !isOkForStartMenu && !isOkForBureau && !isOkForBureauSpec)
            {
                var resMsgBox = MessageBox.Show(
                    "Vous n'avez coché aucune case : aucun raccourci pour démarrer Badger2018 sera créé. Si vous souhaitez démarrer l'application vous devrez vous rendre de son répertoire afin de chercher son fichier exécutable. Souhaitez-vous continuer sans créer de raccourcis ?",
                    "Avertissement", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (resMsgBox == MessageBoxResult.No || resMsgBox == MessageBoxResult.None)
                {
                    return;
                }
            }

            if (isOkForStartFolder)
            {
                CreateShortcut(_exePath, Environment.SpecialFolder.Startup,
                    "Démarre l'application Badger2018. Si l'option \"Badger au premier lancement du programme de la journée\" a été activée, alors l'application badgera le premier badgeage du jour."
                    );
            }
            else
            {
                RemoveShortcut(_exePath, Environment.SpecialFolder.Startup);
            }

            if (isOkForStartMenu)
            {
                CreateShortcut(_exePath, Environment.SpecialFolder.StartMenu,
                    "Démarre l'application Badger2018. Si l'option \"Badger au premier lancement du programme de la journée\" a été activée, alors l'application badgera le premier badgeage du jour."
                );
            }
            else
            {
                RemoveShortcut(_exePath, Environment.SpecialFolder.StartMenu);
            }

            if (isOkForBureau)
            {
                CreateShortcut(_exePath, Environment.SpecialFolder.Desktop,
                    "Démarre l'application Badger2018. Si l'option \"Badger au premier lancement du programme de la journée\" a été activée, alors l'application badgera le premier badgeage du jour."
                );
            }
            else
            {
                RemoveShortcut(_exePath, Environment.SpecialFolder.Desktop);
            }

            if (isOkForBureauSpec)
            {
                CreateShortcut(_exePath, Environment.SpecialFolder.Desktop,
                    "Démarre l'application Badger2018. Si l'option \"Badger au premier lancement du programme de la journée\" a été activée, alors l'application NE badgera PAS le premier badgeage du jour.",
                    "Badger2018 [sans autobadge]", "-n"
                );
            }
            else
            {
                RemoveShortcut(_exePath, Environment.SpecialFolder.Desktop, "-n");
            }

            Close();
        }

        private bool ExistsShortcut(string exePath, Environment.SpecialFolder folder, String args = null)
        {
            try
            {
                List<IWshShortcut> listShortcut =
                    ShortcutUtils.GetShortcutsInDirectory(Environment.GetFolderPath(folder));

                return args == null ? listShortcut.Any(r => r.TargetPath.Contains(exePath)) : listShortcut.Any(r => r.TargetPath.Contains(exePath) && r.Arguments.Equals("-n"));
            }
            catch (Exception ex)
            {
                ExceptionHandlingUtils.LogAndHideException(ex, "Test existence raccourci pour " + folder);
            }

            return false;
        }

        private void CreateShortcut(string exePath, Environment.SpecialFolder folder, String description, string title = "Badger2018", string args = null)
        {
            var directoryInfo = new DirectoryInfo(exePath).Parent;
            if (directoryInfo != null)
            {
                var newShortcut = ShortcutUtils.CreateShortcut(title,
                    Environment.GetFolderPath(folder),
                    exePath,
                    (directoryInfo.FullName)
                );

                newShortcut.Description = description;
                newShortcut.Arguments = args;

                newShortcut.Save();
            }
        }

        public void RemoveShortcut(string exePath, Environment.SpecialFolder folder, string args = null)
        {
            List<IWshShortcut> listShortcut =
                ShortcutUtils.GetShortcutsInDirectory(Environment.GetFolderPath(folder));

            if (args == null)
            {
                foreach (
                    IWshShortcut shtcut in
                    listShortcut.Where(r => r.TargetPath.Contains(exePath)))
                {
                    File.Delete(shtcut.FullName);
                }
            }
            else
            {
                foreach (
                    IWshShortcut shtcut in
                    listShortcut.Where(r => r.TargetPath.Contains(exePath) && r.Arguments.Equals(args)))
                {
                    File.Delete(shtcut.FullName);
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
