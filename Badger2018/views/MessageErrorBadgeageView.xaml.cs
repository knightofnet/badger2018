using System;
using System.Windows;
using System.Windows.Shell;
using AryxDevViewLibrary.utils;
using Badger2018.constants;
using Badger2018.dto;

namespace Badger2018.views
{
    /// <summary>
    /// Logique d'interaction pour MessageErrorBadgeageView.xaml
    /// </summary>
    public partial class MessageErrorBadgeageView : Window
    {
        public EnumErrorCodeRetour CodeRetour { get; set; }

        public string ErreurMessage { get; set; }

        private MessageErrorBadgeageView()
        {
            InitializeComponent();

            lienConsult.Click += (sender, args) =>
            {
                CodeRetour = EnumErrorCodeRetour.CONSULTER_POINTAGE;
                Close();
            };

            lienManuBadge.Click += (sender, args) =>
            {
                CodeRetour = EnumErrorCodeRetour.OPEN_BADGE_PAGE;
                Close();
            };

            lienRetry.Click += (sender, args) =>
            {
                CodeRetour = EnumErrorCodeRetour.RETRY;
                Close();
            };

            lienSirrhius.Click += (sender, args) =>
            {
                CodeRetour = EnumErrorCodeRetour.OPEN_SIRHIUS;
                Close();
            };

            btnAnnuler.Click += (sender, args) =>
            {
                CodeRetour = EnumErrorCodeRetour.ANNULER;
                Close();
            };

            TaskbarItemInfo = new TaskbarItemInfo() { ProgressState = TaskbarItemProgressState.Normal };
            Loaded += delegate(object sender, RoutedEventArgs args)
            {
                TaskbarItemInfo.ProgressValue = 0.5;
                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Error;
            };
            
            FocusableChanged += (sender, args) =>
            {
              
            };

        }

        public void MarkConsultAsRecommend()
        {

            lblLienConsult.FontWeight = FontWeights.Bold;
            lienConsult.Inlines.Clear();
            lienConsult.Inlines.Add("Consulter mes pointages (recommandé)");

        }

        private void SetErreurMessage(string message)
        {
            lblErreurLbl.Content = message;
        }

        private void SetDtErreur(DateTime dt)
        {
            Title += " - " + dt.ToShortTimeString();
        }

        private void SetMessage(string message)
        {
            atErreur.Text = message;

        }

        private void SetIsWarning(bool isWarning)
        {
            if (isWarning)
            {
                imgA.Source = PresentationImageUtils.DoGetImageSourceFromResource(GetType().Assembly.GetName().Name, "sign-warning-icon.png");
            }
            else
            {
                imgA.Source = PresentationImageUtils.DoGetImageSourceFromResource(GetType().Assembly.GetName().Name, "sign-error-icon.png");
            }
        }

        public static EnumErrorCodeRetour ShowMessageError(Exception e, DateTime dt, Window progessWindow, int etapeBadgage)
        {
            String messagePrecision = "";
            bool isConsultRecommand = false;
            switch (etapeBadgage)
            {
                case 0:
                    messagePrecision = "Le navigateur permettant de badger n'a pas démarré, ou une erreur s'est produite lors de son démarrage.";
                    break;
                case 1:
                    messagePrecision = "Impossible de naviguer vers le site de badgeage. Le site n'est peut-être pas accessible (serveur hors-service), l'URL n'est pas correcte ou le site n'a pas répondu assez vite.";
                    break;
                case 2:
                case 3:
                case 4:
                    messagePrecision = "Les éléments nécessaires pour effectuer le pointage n'ont pas été trouvé. La page de badgeage est peut-être inacessible ou son contenu à changer.";
                    isConsultRecommand = true;
                    break;
                case 5:
                    isConsultRecommand = true;
                    break;

            }


            MessageErrorBadgeageView m = new MessageErrorBadgeageView();
            m.SetIsWarning(isConsultRecommand);
            m.SetErreurMessage(e.Message);
            m.SetDtErreur(dt);
            m.SetMessage(messagePrecision);
            m.WindowStartupLocation = WindowStartupLocation.Manual;
            m.Top = progessWindow.Top;
            m.Left = progessWindow.Left + progessWindow.Width + 10;
            if (isConsultRecommand) { m.MarkConsultAsRecommend(); };

            System.Media.SystemSounds.Beep.Play();

            m.ShowDialog();

            if (m.CodeRetour == null)
            {
                return EnumErrorCodeRetour.ANNULER;
            }
            return m.CodeRetour;

        }


    }
}
