﻿using Badger2018.dto;
using Badger2018.utils;
using BadgerCommonLibrary.utils;
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
using static Badger2018.views.usercontrols.semaine.JourSemaineControl;

namespace Badger2018.views.usercontrols.semaine
{
    /// <summary>
    /// Logique d'interaction pour HoverPeriodePage.xaml
    /// </summary>
    public partial class HoverPeriodePage : Page
    {
        public HoverPeriodePage()
        {
            InitializeComponent();
        }

        public void Loads(PeriodeG pG, AppOptions appOptions)
        {
            lblNamePer.Content = pG.Name;

            lblHdeb.ContentShortTime(pG.StartTs);
            lblHfin.ContentShortTime(pG.EndTs);


            rectA.Fill = pG.PerRectangle.Fill;
            rectB.Fill = pG.PerRectangle.Fill;

            lblTpsTravPer.ContentShortTime(pG.EndTs - pG.StartTs);


            bool isMaxDepassed = false;
            lblTpsTravTot.ContentShortTime(TimesUtils.GetTempsTravaille(AppDateUtils.DtNow(), pG.InfosDay.EtatBadger, pG.InfosDay.Times, appOptions, pG.InfosDay.TypesJournees, false, ref isMaxDepassed));

        }
    }
}