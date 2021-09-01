using System;
using System.Linq;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using Badger2018.business.dbb;
using Badger2018.constants;
using Badger2018.dto;
using Badger2018.dto.bdd;
using Badger2018.services;
using BadgerCommonLibrary.utils;
using ExceptionHandlingUtils = BadgerCommonLibrary.utils.ExceptionHandlingUtils;

namespace Badger2018.business.saver
{
    class BddPointageWriterReader : ISaverTimes
    {

        private class DataBadgeageVehicle
        {
            public DateTime DayToMod { get; set; }
            public TimesBadgerDto Times { get; set; }
            public EnumTypesJournees TypeJournee { get; set; }
            public int EtatBadger { get; set; }
            public int OldEtatBadger { get; internal set; }

            public double WorkAtHomeCpt { get; internal set; }
            public TimeSpan? LastCdSeen { get; internal set; }
        }

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

        public void SaveAnotherDayTime(DateTime dayToMod, TimesBadgerDto times, EnumTypesJournees typeJournee, int etatBadger, TimeSpan cdLastSeen, double valTt)
        {
            DataBadgeageVehicle data = new DataBadgeageVehicle()
            {
                DayToMod = dayToMod,
                Times = times,
                EtatBadger = etatBadger,
                TypeJournee = typeJournee,
                OldEtatBadger = etatBadger,
                LastCdSeen = cdLastSeen,
                WorkAtHomeCpt = valTt,
            };
            SaveDayTimes(data);
        }

        public void SaveCurrentDayTimes()
        {
            DataBadgeageVehicle data = new DataBadgeageVehicle()
            {
                DayToMod = AppDateUtils.DtNow(),
                Times = _pWinRef.Times,
                EtatBadger = _pWinRef.EtatBadger,
                TypeJournee = _pWinRef.TypeJournee,
                OldEtatBadger = _pWinRef.OldEtatBadger,
                LastCdSeen = _pWinRef.PrgOptions.LastCdSeen,
                WorkAtHomeCpt =  _pWinRef.WorkAtHomeCpt,

            };
            SaveDayTimes(data);
        }

        private void SaveDayTimes(DataBadgeageVehicle data)
        {
            _logger.Debug("DEBUT : SaveCurrentDayTimes()");

            DbbAccessManager.Instance.StartTransaction();
            try
            {
                PointageElt pElt = new PointageElt
                {
                    DateDay = data.DayToMod.ToString(),
                    EtatBadger = data.EtatBadger,
                    OldEtatBadger = data.OldEtatBadger,
                    TypeJournee = data.TypeJournee.Index,
                    //IsNotif1Showed = _pWinRef.NotifManager.IsNotifShow(Cst.NotifCust1Name),
                    //IsNotif2Showed = _pWinRef.NotifManager.IsNotifShow(Cst.NotifCust2Name),
                    WorkAtHomeCpt =  data.WorkAtHomeCpt
                };

                SaveClassicDayBadgeages(data.DayToMod, data);
                SavePauseBadgeages(data.DayToMod, data);
                SaveDatasJours(pElt, data.DayToMod);

                if (data.EtatBadger == EnumBadgeageType.PLAGE_TRAV_APREM_END.Index)
                {
                    TimeSpan t = TimeSpan.Zero;
                    if (EnumTypesJournees.Complete == data.TypeJournee)
                    {
                        t += data.Times.GetTpsTravMatin();
                        t += data.Times.GetTpsTravAprem();
                    }
                    else
                    {
                        t = data.Times.PlageTravAprem.EndOrDft - data.Times.PlageTravMatin.Start;
                    }


                    _joursServices.UpdateTpsTravaille(data.DayToMod, t);
                }


                _logger.Debug("Commit !");
                DbbAccessManager.Instance.StopAndCommitTransaction();
            }
            catch (Exception ex)
            {
                _logger.Debug("EXCEPTION CATCHEE : SaveCurrentDayTimes() => {0}", ex.GetType().FullName);
                ExceptionHandlingUtils.LogAndHideException(ex);

                _logger.Debug("Rollback !");
                DbbAccessManager.Instance.StopAndRollbackTransaction();

            }
            finally
            {
                if (DbbAccessManager.Instance.CurrentTransaction != null)
                {
                    _logger.Debug("Commit in finally !");
                    DbbAccessManager.Instance.StopAndCommitTransaction();
                }

                _logger.Debug("FIN : SaveCurrentDayTimes()");
            }

        }

        public void SaveDatasJours(PointageElt pElt, DateTime dateRef)
        {
            if (_joursServices.IsJourExistFor(dateRef))
            {
                _logger.Debug("Sauvegarde en BDD du jour. Le jour existe déjà, on le mets à jour");
                _joursServices.UpdateJourWithPointageElt(dateRef, pElt);
            }
            else
            {
                _logger.Debug("Sauvegarde en BDD du jour. Nouvelle journée");
                _joursServices.InsertNewJour(dateRef, pElt);
            }
        }


        private void SaveClassicDayBadgeages(DateTime dtRef, DataBadgeageVehicle data)
        {
            _badgeageService.RemoveBadgeagesOfADay(dtRef);

            if (data.EtatBadger >= EnumBadgeageType.PLAGE_TRAV_MATIN_START.Index)
            {
                _logger.Debug("Sauvegarde en BDD du badgage du matin ({0})", data.Times.PlageTravMatin.Start);
                _badgeageService.AddBadgeage(EnumBadgeageType.PLAGE_TRAV_MATIN_START.Index, data.Times.PlageTravMatin.Start, cdToSave: data.LastCdSeen);
            }
            if (data.EtatBadger >= EnumBadgeageType.PLAGE_TRAV_MATIN_END.Index)
            {
                _logger.Debug("Sauvegarde en BDD du badgage de fin de matinée ({0})", data.Times.PlageTravMatin.EndOrDft);
                _badgeageService.AddBadgeage(EnumBadgeageType.PLAGE_TRAV_MATIN_END.Index, data.Times.PlageTravMatin.EndOrDft, cdToSave: data.LastCdSeen);
            }
            if (data.EtatBadger >= EnumBadgeageType.PLAGE_TRAV_APREM_START.Index)
            {
                _logger.Debug("Sauvegarde en BDD du badgage du début d'après-midi ({0})", data.Times.PlageTravAprem.Start);
                _badgeageService.AddBadgeage(EnumBadgeageType.PLAGE_TRAV_APREM_START.Index, data.Times.PlageTravAprem.Start, cdToSave: data.LastCdSeen);
            }
            if (data.EtatBadger >= EnumBadgeageType.PLAGE_TRAV_APREM_END.Index)
            {
                _logger.Debug("Sauvegarde en BDD du badgage de fin d'après-midi ({0})", data.Times.PlageTravAprem.EndOrDft);
                _badgeageService.AddBadgeage(EnumBadgeageType.PLAGE_TRAV_APREM_END.Index, data.Times.PlageTravAprem.EndOrDft, cdToSave: data.LastCdSeen);
            }

        }

        private void SavePauseBadgeages(DateTime dtRef, DataBadgeageVehicle data)
        {
            if (!data.Times.PausesHorsDelai.Any())
            {
                return;
            }

            foreach (IntervalTemps pause in data.Times.PausesHorsDelai)
            {
                string rndStr = StringUtils.RandomString(16, ensureUnique: true);
                while (_badgeageService.IsExistPauseWithThisRelationId(rndStr))
                {
                    rndStr = StringUtils.RandomString(16, ensureUnique: true);
                }

                _logger.Debug("Sauvegarde en BDD d'un pause (complete? {2}). Début : {0}, Fin : {1} ", pause.Start, pause.EndOrDft, pause.IsIntervalComplet());
                _badgeageService.AddBadgeage(EnumBadgeageType.PLAGE_START.Index, pause.Start, rndStr, cdToSave: data.LastCdSeen);
                if (pause.IsIntervalComplet())
                {
                    _badgeageService.AddBadgeage(EnumBadgeageType.PLAGE_END.Index, pause.EndOrDft, rndStr, cdToSave: data.LastCdSeen);
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
            pElt.WorkAtHomeCpt = jour.WorkAtHomeCpt;
        }


    }
}
