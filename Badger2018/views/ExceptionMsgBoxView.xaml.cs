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
using BadgerCommonLibrary.constants;

namespace Badger2018.views
{
    /// <summary>
    /// Logique d'interaction pour ExceptionMsgBoxView.xaml
    /// </summary>
    public partial class ExceptionMsgBoxView : Window
    {
        public Exception ExceptionHandled { get; set; }

        public string ComplementString { get; set; }

        public ExceptionMsgBoxView(Exception ex, String complement, string inAccessText = null)
        {
            InitializeComponent();

            ExceptionHandled = ex;
            ComplementString = complement;

            if (accessText != null)
            {
                accessText.Text = inAccessText;
            }

            rchTbox.Document.Blocks.Clear();
            if (ComplementString != null)
            {
                rchTbox.Document.Blocks.Add(new Paragraph(new Run(ComplementString)));
            }
            rchTbox.Document.Blocks.Add(new Paragraph(new Run(ex.GetType().Name)));
            rchTbox.Document.Blocks.Add(new Paragraph(new Run(ex.Message)));
            rchTbox.Document.Blocks.Add(new Paragraph(new Run(ex.StackTrace)));
        }



        public static void ShowException(Exception ex, String partTrt = null, string inAccessText = null)
        {
            ExceptionMsgBoxView exceptionView = new ExceptionMsgBoxView(ex, partTrt, inAccessText);

            exceptionView.ShowDialog();

        }



        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnSentMail_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process proc = new System.Diagnostics.Process();



            proc.StartInfo.FileName = "mailto:arnaud.leblanc@cnav.fr?" +
                                      "subject=[Badger2018] Erreur lors de l'utilisation du programme" +
                                      "&body=" +
                                      "Bonjour Arnaud,%0A%0AUne erreur est survenue dans le programme Badger2018. Ci-dessous, l'erreur en question.%0A" +
                                      "%0AComplément : " + (ComplementString ?? "Aucun") +
                                      "%0AType d'erreur : " + ExceptionHandled.GetType().Name +
                                      "%0AMessage : " + ExceptionHandled.Message +
                                      "%0A" + "Stack :" + ExceptionHandled.StackTrace +
                                      "%0A%0ACordialement,%0A";
            proc.Start();
        }


    }
}
