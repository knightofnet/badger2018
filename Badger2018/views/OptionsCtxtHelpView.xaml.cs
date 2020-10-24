using System;
using System.Windows;

namespace Badger2018.views
{
    /// <summary>
    /// Logique d'interaction pour OptionsCtxtHelpView.xaml
    /// </summary>
    public partial class OptionsCtxtHelpView : Window
    {

        public string HelpFileUrl { get; private set; }

        public OptionsCtxtHelpView(string fileHtmlHelpPath)
        {
            InitializeComponent();

            HelpFileUrl = String.Format("file:///{0}", fileHtmlHelpPath.Replace(@"\", "/"));






            webView.Navigate(new Uri(HelpFileUrl));
        }

        public void GoToAnchor(string fullAnchorName)
        {
            webView.Navigate(new Uri(String.Format("{0}#{1}", HelpFileUrl, fullAnchorName)));

        }

        public void GoToAnchor(string viewName, string anchorName)
        {
            GoToAnchor(viewName + "_" + anchorName);

        }

    }
}
