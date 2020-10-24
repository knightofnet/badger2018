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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Badger2018.views.wizard
{
    /// <summary>
    /// Logique d'interaction pour Page3.xaml
    /// </summary>
    public partial class Page3 : Page
    {
        private const string QaBonneRep = "externe/tiers configurée pour la CNAV";
        private const string QbBonneRep = "Presque vrai selon la configuration";
        private const string QcBonneRep = "Faux";

        public bool IsFormOkRef { get; internal set; }

        public Page3()
        {
            InitializeComponent();

            cboxQa.Items.Add("Faites votre choix");
            cboxQa.Items.Add("CNAV, développée sur demande");
            cboxQa.Items.Add(QaBonneRep);
            cboxQa.SelectedIndex = 0;
            

            cboxQb.Items.Add("Faites votre choix");
            cboxQb.Items.Add("Vrai : adieu GTA");
            cboxQb.Items.Add(QbBonneRep);
            cboxQb.Items.Add("Faux : l'outil ne se substitue pas aux badgeages à faire sur GTA");
            cboxQb.SelectedIndex = 0;

            cboxQc.Items.Add("Faites votre choix");
            cboxQc.Items.Add("Vrai");
            cboxQc.Items.Add(QcBonneRep);
            cboxQc.SelectedIndex = 0;



            Loaded += (s, a) =>
            {
                cboxQa.SelectionChanged += CboxSelectionChanged;
                cboxQb.SelectionChanged += CboxSelectionChanged;
                cboxQc.SelectionChanged += CboxSelectionChanged;
            };

        }

        private void CboxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IsFormOkRef = IsChoixOk();
        }

        public bool IsChoixOk()
        {
            return cboxQa.SelectedItem.Equals(QaBonneRep) && cboxQb.SelectedItem.Equals(QbBonneRep) && cboxQc.SelectedItem.Equals(QcBonneRep);
        }
    }
}
