using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;

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

            if (inAccessText != null)
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



        public static void ShowException(Exception ex, String lblComplementException = null, string textDescriptionException = null)
        {
            ExceptionMsgBoxView exceptionView = new ExceptionMsgBoxView(ex, lblComplementException, textDescriptionException);

            exceptionView.ShowDialog();

        }



        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnSentMail_Click(object sender, RoutedEventArgs e)
        {
            Process proc = new Process();



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
