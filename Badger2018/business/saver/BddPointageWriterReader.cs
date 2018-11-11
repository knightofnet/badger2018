using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using Badger2018.constants;
using Badger2018.dto;
using Badger2018.dto.bdd;
using Badger2018.services;
using BadgerCommonLibrary.utils;

namespace Badger2018.business.saver
{
    class BddPointageWriterReader : ISaverTimes
    {
        private static readonly Logger _logger = Logger.LastLoggerInstance;
        private readonly MainWindow _pWinRef;
        private readonly BadgeagesServices _badgeageService;
        private readonly JoursServices _joursServices;

        public BddPointageWriterReader(MainWindow pwin)
        {

            _badgeageService = ServicesMgr.Instance.BadgeagesServices;
            _joursServices = ServicesMgr.Instance.JoursServices;
            _pWinRef = pwin;
        }

        public void SaveCurrentDayTimes()
        {
            DbbAccessManager.Instance.StartTransaction();
            try
            {
                PointageElt pElt = new PointageElt
                {
                    DateDay = _pWinRef.RealTimeDtNow.ToString(),
                    EtatBadger = _pWinRef.EtatBadger,
                    OldEtatBadger = _pWinRef.OldEtatBadger,
                    TypeJournee = _pWinRef.TypeJournee.Index,
                    IsNotif1Showed = _pWinRef.NotifManager.IsNotifShow(Cst.NotifCust1Name),
                    IsNotif2Showed = _pWinRef.NotifManager.IsNotifShow(Cst.NotifCust2Name),

                };

                SaveClassicDayBadgeages();
                SavePauseBadgeages();
                SaveDatasJours(pElt, AppDateUtils.DtNow());

                if (_pWinRef.EtatBadger == EnumBadgeageType.PLAGE_TRAV_APREM_END.Index)
                {
                    TimeSpan t = TimeSpan.Zero;
                    if (EnumTypesJournees.Complete == _pWinRef.TypeJournee || EnumTypesJournees.Matin == _pWinRef.TypeJournee)
                    {
                        t += _pWinRef.Times.GetTpsTravMatin();
                    }
                    if (EnumTypesJournees.Complete == _pWinRef.TypeJournee || EnumTypesJournees.ApresMidi == _pWinRef.TypeJournee)
                    {
                        t += _pWinRef.Times.GetTpsTravAprem();
                    }

                    _joursServices.UpdateTpsTravaille(_pWinRef.RealTimeDtNow, t);
                }



                DbbAccessManager.Instance.StopAndCommitTransaction();
            }
            catch (Exception ex)
            {
                ExceptionHandlingUtils.LogAndHideException(ex);

                DbbAccessManager.Instance.StopAndRollbackTransaction();
            }



        }

        public void SaveDatasJours(PointageElt pElt, DateTime dateRef)
        {
            if (_joursServices.IsJourExistFor(dateRef))
            {
                _joursServices.UpdateJourWithPointageElt(dateRef, pElt);
            }
            else
            {
                _joursServices.InsertNewJour(dateRef, pElt);
            }
        }


        private void SaveClassicDayBadgeages()
        {
            _badgeageService.RemoveBadgeagesOfToday();

            if (_pWinRef.EtatBadger >= EnumBadgeageType.PLAGE_TRAV_MATIN_START.Index)
            {
                //TODO LOG
                _badgeageService.AddBadgeageForToday(EnumBadgeageType.PLAGE_TRAV_MATIN_START.Index, _pWinRef.Times.PlageTravMatin.Start);
            }
            if (_pWinRef.EtatBadger >= EnumBadgeageType.PLAGE_TRAV_MATIN_END.Index)
            {
                //TODO LOG
                _badgeageService.AddBadgeageForToday(EnumBadgeageType.PLAGE_TRAV_MATIN_END.Index, _pWinRef.Times.PlageTravMatin.EndOrDft);
            }
            if (_pWinRef.EtatBadger >= EnumBadgeageType.PLAGE_TRAV_APREM_START.Index)
            {
                //TODO LOG
                _badgeageService.AddBadgeageForToday(EnumBadgeageType.PLAGE_TRAV_APREM_START.Index, _pWinRef.Times.PlageTravAprem.Start);
            }
            if (_pWinRef.EtatBadger >= EnumBadgeageType.PLAGE_TRAV_APREM_END.Index)
            {
                //TODO LOG
                _badgeageService.AddBadgeageForToday(EnumBadgeageType.PLAGE_TRAV_APREM_END.Index, _pWinRef.Times.PlageTravAprem.EndOrDft);
            }

        }

        private void SavePauseBadgeages()
        {
            if (!_pWinRef.Times.PausesHorsDelai.Any())
            {
                return;
            }

            foreach (IntervalTemps pause in _pWinRef.Times.PausesHorsDelai)
            {
                string rndStr = StringUtils.RandomString(16);
                //TODO LOG
                _badgeageService.AddBadgeageForToday(EnumBadgeageType.PLAGE_START.Index, pause.Start, rndStr);
                if (pause.IsIntervalComplet())
                {
                    _badgeageService.AddBadgeageForToday(EnumBadgeageType.PLAGE_END.Index, pause.EndOrDft, rndStr);
                }

            }
        }

        public bool MustReloadIncomplete()
        {
            return _joursServices.IsJourExistFor(AppDateUtils.DtNow());
        }

        public PointageElt LoadIncomplete()
        {
            PointageElt pElt = new PointageElt();

            LoadDatasJours(pElt);
            LoadClassicDayBadgeages(pElt);
            LoadPauseBadgeages(pElt);

            return pElt;
        }

        private void LoadClassicDayBadgeages(PointageElt pElt)
        {

            pElt.B0 = _badgeageService.GetBadgeageOrDft(EnumBadgeageType.PLAGE_TRAV_MATIN_START, AppDateUtils.DtNow());
            pElt.B1 = _badgeageService.GetBadgeageOrDft(EnumBadgeageType.PLAGE_TRAV_MATIN_END, AppDateUtils.DtNow());
            pElt.B2 = _badgeageService.GetBadgeageOrDft(EnumBadgeageType.PLAGE_TRAV_APREM_START, AppDateUtils.DtNow());
            pElt.B3 = _badgeageService.GetBadgeageOrDft(EnumBadgeageType.PLAGE_TRAV_APREM_END, AppDateUtils.DtNow());
        }



        private void LoadPauseBadgeages(PointageElt pElt)
        {
            pElt.Pauses = _badgeageService.GetPauses(AppDateUtils.DtNow());
        }

        private void LoadDatasJours(PointageElt pElt)
        {
            pElt.DateDay = AppDateUtils.DtNow().ToString();
            JourEntryDto jour = _joursServices.GetJourData(AppDateUtils.DtNow());

            pElt.EtatBadger = jour.EtatBadger;
            pElt.OldEtatBadger = jour.OldEtatBadger;
            pElt.IsComplete = jour.IsComplete;
            pElt.TypeJournee = jour.TypeJour.Index;
        }
    }
}
