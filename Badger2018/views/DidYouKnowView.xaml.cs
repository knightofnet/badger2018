using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using AryxDevLibrary.extensions;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using AryxDevLibrary.utils.xml;

namespace Badger2018.views
{
    /// <summary>
    /// Logique d'interaction pour DidYouKnowView.xaml
    /// </summary>
    public partial class DidYouKnowView : Window
    {

        private static readonly Logger _logger = Logger.LastLoggerInstance;
        private bool _showTipsAtStart;

        public XmlFile XmlSource { get; private set; }

        public int LastIntTips { get; set; }

        public bool ShowTipsAtStart
        {
            get { return _showTipsAtStart; }
            set
            {
                _showTipsAtStart = value;
                chkShowInStart.IsChecked = value;
            }
        }

        public DidYouKnowView()
        {
            InitializeComponent();

            XmlSource = XmlFile.InitXmlFileByString(Properties.Resources.didyouknow);

            rchTbox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;



            chkShowInStart.Click += (sender, args) =>
            {
                bool val = chkShowInStart.IsChecked.HasValue && chkShowInStart.IsChecked.Value;
                ShowTipsAtStart = val;
            };

            NextTips(isRnd: true);
        }

        public void NextTips(bool isNext = true, bool isRnd = false)
        {
            if (isRnd)
            {
                Random rnd = new Random();
                int newInt = rnd.Next(0, XmlSource.Root.ChildNodes.Count) + 1;
                while (newInt == LastIntTips && XmlSource.Root.ChildNodes.Count > 1)
                {
                    newInt = rnd.Next(0, XmlSource.Root.ChildNodes.Count) + 1;
                }
                LastIntTips = newInt;
            }
            else
            {
                if (isNext)
                {
                    LastIntTips++;
                }
                else
                {
                    LastIntTips--;
                }

            }

            if (LastIntTips > XmlSource.Root.ChildNodes.Count)
            {
                LastIntTips = 1;
            }
            if (LastIntTips < 1)
            {
                LastIntTips = XmlSource.Root.ChildNodes.Count;
            }

            string tips = XmlUtils.GetValueXpathOrDefault(XmlSource.Root, "//elt[" + LastIntTips + "]", null);
            if (tips == null) return;

            Block firstBlock = rchTbox.Document.Blocks.FirstBlock;
            rchTbox.Document.Blocks.Clear();

            bool isBlockTitle = true;
            rchTbox.Document.Blocks.Add(firstBlock);
            foreach (string paragraphe in tips.SplitByStr("\r\n"))
            {
                string text = paragraphe.Trim('\t').Trim();
                if (!StringUtils.IsNullOrWhiteSpace(text))
                {
                    Run newRun = new Run(text);
                    Paragraph newParagraph = new Paragraph(newRun);
                    if (isBlockTitle)
                    {
                        isBlockTitle = false;
                        newRun.FontWeight = FontWeights.Bold;
                        newRun.FontSize = 18;
                        newRun.Foreground = new SolidColorBrush(Colors.DodgerBlue);

                        newParagraph.Margin = new Thickness(newParagraph.Margin.Left, 0, newParagraph.Margin.Right, newParagraph.Margin.Bottom);

                    }


                    rchTbox.Document.Blocks.Add(newParagraph);
                }
            }

            lblIndex.Content = String.Format("astuce {0}/{1}", LastIntTips, XmlSource.Root.ChildNodes.Count);
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                NextTips(isNext: false);
            }
            else
            {
                NextTips();
            }


        }
    }
}
