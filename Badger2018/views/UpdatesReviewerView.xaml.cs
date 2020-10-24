using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using AryxDevViewLibrary.utils;
using BadgerCommonLibrary.dto;

namespace Badger2018.views
{
    /// <summary>
    /// Logique d'interaction pour UpdatesReviewerView.xaml
    /// </summary>
    public partial class UpdatesReviewerView : Window
    {
        public bool IsDoUpdate { get; private set; }

        public List<UpdateInfoDto> ListVersions { get; private set; }

        public bool IsBtnClose { get; private set; }

        private DgColumnExtended _colAppName;
        private DgColumnExtended _colVersionName;

        public UpdatesReviewerView(List<UpdateInfoDto> listUpdateInfoDtos)
        {
            InitializeComponent();
            IsBtnClose = false;

            ListVersions = listUpdateInfoDtos;


            dgVersions.CanUserAddRows = false;
            dgVersions.CanUserDeleteRows = false;
            dgVersions.AutoGenerateColumns = false;
            dgVersions.IsReadOnly = true;
            dgVersions.ItemsSource = ListVersions;

            InitDatagrid();

            dgVersions.UpdateLayout();
            dgVersions.Items.Refresh();

            dgVersions.SelectionChanged += DgVersionsOnSelectionChanged;

            dgVersions.SelectedIndex = ListVersions.Any() ? 0 : -1;

            Closing += (sender, args) =>
            {
                if (!IsBtnClose)
                {
                    IsDoUpdate = false;
                }
            };
        }



        private void InitDatagrid()
        {
            dgVersions.Columns.Clear();

            _colAppName = new DgColumnExtended();
            _colAppName.Header = "Titre";
            _colAppName.BasePropName = "Title";
            _colAppName.Binding = new Binding(_colAppName.BasePropName);
            dgVersions.Columns.Add(_colAppName);

            _colVersionName = new DgColumnExtended();
            _colVersionName.Header = "Version";
            _colVersionName.BasePropName = "Version";
            _colVersionName.Binding = new Binding(_colVersionName.BasePropName);
            _colVersionName.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            dgVersions.Columns.Add(_colVersionName);

            return;

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            IsDoUpdate = false;
            IsBtnClose = true;
            Close();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            IsDoUpdate = true;
            IsBtnClose = true;
            Close();
        }


        private void DgVersionsOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            UpdateInfoDto updSel = dgVersions.SelectedItem as UpdateInfoDto;
            if (updSel == null) return;

            lblName.Content = updSel.Title;
            lblVersion.Content = updSel.Version;
            lblAuthor.Content = updSel.Authors;

            rtbDescription.Document.Blocks.Clear();
            rtbDescription.Document.Blocks.Add(new Paragraph(new Run(updSel.Description)));

        }

    }
}
