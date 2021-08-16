using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Badger2018.utils;

namespace Badger2018.views.usercontrols
{
    /// <summary>
    /// Logique d'interaction pour FriseDayControl.xaml
    /// </summary>
    public partial class FriseDayControl : UserControl
    {
        private readonly MoreDetailsView.IMoreDetailViewPresenter _presenter;

        public event Action<DateTime> OnClickBtnSeeScreenshot;

        public DateTime DtTimeIn { get; set; }

        public String Title { get; set; }
        public String SubTitle { get; set; }

        public String LTag { get; set; }
        public Color Color { get; set; }
        public int EtatBadgeageAssociated { get; internal set; }

        public String PauseRef { get; set; }

        public FriseDayControl()
        {
            InitializeComponent();
            mainGrid.Background = null;
            EtatBadgeageAssociated = -2;


        }
        public FriseDayControl(DateTime hour, string title, string subtitle, MoreDetailsView.IMoreDetailViewPresenter presenter )
            : this()
        {

            DtTimeIn = hour;
            Title = title;
            SubTitle = subtitle;

            _presenter = presenter;

            RefreshUi();
        }

        public void RefreshUi()
        {
            lblHours.ContentShortTime(DtTimeIn);
            lblName.Content = Title;
            lblMoreStr.Content = SubTitle;
            rectColor.Fill = new SolidColorBrush(Color);
            rectSep.Fill = new SolidColorBrush(Color);
            rectEndSepBottom.Stroke = new SolidColorBrush(Color);
            rectEndSepBottom.Fill = new SolidColorBrush(Color);
        }

        public void SetBtnScreenshotVisible(bool isVisible)
        {
            lblBtnSeeScreenShot.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void btnSeeScreenShot_Click(object sender, RoutedEventArgs e)
        {
            if (OnClickBtnSeeScreenshot != null)
            {
                OnClickBtnSeeScreenshot(DtTimeIn);
            }
        }

        public void SetEndBloc(bool value)
        {
            rectEndSepBottom.Visibility = value ? Visibility.Visible : Visibility.Hidden;
        }

        public void ActiveLinkModifyLastBadgeage()
        {
            lblMoreStr.Content = null;
            Hyperlink hl = new Hyperlink(new Run("Faire de ce badgage le dernier de la journée"));
            lblMoreStr.Content = hl;

            hl.Click += (sender, args) =>
            {

                _presenter.ChangeEtatBadgeageAndSetComplete(DtTimeIn, EtatBadgeageAssociated, 3);
            };
        }
    }
}
