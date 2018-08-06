using System;
using System.Collections.Generic;
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
using Badger2018.constants;
using Badger2018.utils;

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
                imgA.Source = MiscAppUtils.DoGetImageSourceFromResource(GetType().Assembly.GetName().Name, "sign-warning-icon.png");
            } else
            {
                imgA.Source = MiscAppUtils.DoGetImageSourceFromResource(GetType().Assembly.GetName().Name, "sign-error-icon.png");
            }
        }

        public static EnumErrorCodeRetour ShowMessageError(Exception e, DateTime dt, Window progessWindow, bool isConsultRecommand=false )
        {
            MessageErrorBadgeageView m = new MessageErrorBadgeageView();
            m.SetIsWarning(isConsultRecommand);
            m.SetErreurMessage(e.Message);
            m.SetDtErreur(dt);
            m.WindowStartupLocation = WindowStartupLocation.Manual;
            m.Top = progessWindow.Top;
            m.Left = progessWindow.Left + progessWindow.Width + 10;
            if(isConsultRecommand) { m.MarkConsultAsRecommend(); };

            m.ShowDialog();            

            if (m.CodeRetour == null)
            {
                return EnumErrorCodeRetour.ANNULER;
            }
            return m.CodeRetour;

        }

     
    }
}
