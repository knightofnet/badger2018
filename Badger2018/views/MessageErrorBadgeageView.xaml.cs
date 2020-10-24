using System;
using System.Windows;
using AryxDevViewLibrary.utils;
using Badger2018.constants;

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
                case 1:
                    messagePrecision = "Le navigateur permettant de badger n'a pas démarré, ou une erreur s'est produite lors de son démarrage.";
                    break;
                case 2:
                    messagePrecision = "Impossible de naviguer vers le site de badgeage. Le site n'est peut-être pas accessible (server hors-service) ou l'URL n'est pas correcte.";
                    break;
                case 3:
                case 4:
                case 5:
                    messagePrecision = "Les éléments nécessaires pour effectuer le pointage n'ont pas été trouvé. La page de badgeage est peut-être inacessible ou son contenu à changer.";
                    isConsultRecommand = true;
                    break;
                case 6:
                    isConsultRecommand = true;
                    break;

            }


            MessageErrorBadgeageView m = new MessageErrorBadgeageView();
            m.SetIsWarning(isConsultRecommand);
            m.SetErreurMessage(e.Message);
            m.SetDtErreur(dt);
            m.WindowStartupLocation = WindowStartupLocation.Manual;
            m.Top = progessWindow.Top;
            m.Left = progessWindow.Left + progessWindow.Width + 10;
            if (isConsultRecommand) { m.MarkConsultAsRecommend(); };

            m.ShowDialog();

            if (m.CodeRetour == null)
            {
                return EnumErrorCodeRetour.ANNULER;
            }
            return m.CodeRetour;

        }


    }
}
