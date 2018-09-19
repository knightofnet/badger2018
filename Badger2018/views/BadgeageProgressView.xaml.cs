using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Badger2018.views
{
    /// <summary>
    /// Logique d'interaction pour BadgeageProgressView.xaml
    /// </summary>
    public partial class BadgeageProgressView : Window
    {
        private readonly List<Image> _lstImage;
        private readonly List<Label> _lstLabel;
        public BackgroundWorker BackgrounderRef { get; internal set; }

        public BadgeageProgressView()
        {
            InitializeComponent();

            _lstLabel = new List<Label>(6) { lblEt1, lblEt2, lblEt3, lblEt4, lblEt5, lblEt6 };
            _lstImage = new List<Image>(6) { tickEt1, tickEt2, tickEt3, tickEt4, tickEt5, tickEt6 };

            RaZProgress();

        }



        private void RaZProgress()
        {
            foreach (Image image in _lstImage)
            {
                image.Visibility = Visibility.Hidden;
            }

            foreach (Label label in _lstLabel)
            {
                label.Foreground = Cst.SCBGrey;
            }
        }

        public void EnterStep(int i)
        {
            if (i >= _lstImage.Count || i > _lstLabel.Count || i < 0)
            {
                return;
            }

            _lstImage[i].Visibility = Visibility.Hidden;
            _lstLabel[i].Foreground = Cst.SCBBlack;

        }

        public void ValidStep(int i)
        {
            if (i >= _lstImage.Count || i > _lstLabel.Count || i < 0)
            {
                return;
            }

            _lstImage[i].Visibility = Visibility.Visible;
            _lstLabel[i].Foreground = Cst.SCBDarkGreen;

        }

        public void ErrorStep(int i)
        {
            if (i >= _lstImage.Count || i > _lstLabel.Count || i < 0)
            {
                return;
            }

            _lstImage[i].Visibility = Visibility.Hidden;
            _lstLabel[i].Foreground = Cst.SCBDarkRed;
        }

        public void ToogleBtnCancel ()
        {
            btnCancelBadgeage.Visibility = btnCancelBadgeage.Visibility == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;
        }

        private void btnCancelBadgeage_Click(object sender, RoutedEventArgs e)
        {
            if (BackgrounderRef != null && !BackgrounderRef.CancellationPending)
            {
                BackgrounderRef.CancelAsync();
            }
        }
    }
}
