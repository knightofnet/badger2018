using System;
using AryxDevLibrary.utils.logger;
using Badger2018.business.dbb;
using Badger2018.constants;
using Badger2018.dto;
using Badger2018.dto.bdd;
using Badger2018.services.bddLastLayer;
using BadgerCommonLibrary.utils;

namespace Badger2018.services
{
    public class JoursServices
    {
        private static readonly Logger _logger = Logger.LastLoggerInstance;

        public DateTime GetFirstDayOfHistory()
        {
            _logger.Debug("GetFirstDayOfHistory()");

            DbbAccessManager dbb = DbbAccessManager.Instance;
            DateTime? result = JoursBddLayer.GetFirstDayOfHistory(dbb);
            if (!result.HasValue)
            {
                result = AppDateUtils.DtNow();
            }

            _logger.Debug("FIN - GetFirstDayOfHistory() => {0}", result);
            return result.GetValueOrDefault();

        }

        public bool IsJourExistFor(DateTime dtNow)
        {
            _logger.Debug("IsJourExistFor(dtNow : {0})", dtNow);


            DbbAccessManager dbb = DbbAccessManager.Instance;
            bool result = JoursBddLayer.IsJourExistFor(dbb, dtNow);

            _logger.Debug("FIN - IsJourExistFor(...) => {0}", result);

            return result;
        }

        public void UpdateJourWithPointageElt(DateTime date, PointageElt pointageElt)
        {
            _logger.Debug("UpdateJourWithPointageElt(date: {0}, PointageElt: {1})", date, pointageElt);
            DbbAccessManager dbb = DbbAccessManager.Instance;

            JourEntryDto j = new JourEntryDto();
            j.DateJour = date;
            j.EtatBadger = pointageElt.EtatBadger;
            j.OldEtatBadger = pointageElt.OldEtatBadger;
            j.IsComplete = pointageElt.IsComplete;
            j.TypeJour = EnumTypesJournees.GetFromIndex(pointageElt.TypeJournee);

            JoursBddLayer.UpdateJour(dbb, date, j);

            _logger.Debug("FIN - UpdateJourWithPointageElt(...)");
        }

        public void UpdateJourIsComplete(DateTime date, bool isDayComplete = true)
        {
            _logger.Debug("UpdateJourIsComplete(date: {0}, isDayComplete: {1})", date, isDayComplete);
            DbbAccessManager dbb = DbbAccessManager.Instance;

            JoursBddLayer.UpdateJourIsComplete(dbb, date, isDayComplete);

            _logger.Debug("FIN - UpdateJourIsComplete(...)");
        }

        

        public void InsertNewJour(DateTime date, PointageElt pointageElt)
        {
            _logger.Debug("InsertNewJour(date: {0}, PointageElt: {1})", date, pointageElt);
            DbbAccessManager dbb = DbbAccessManager.Instance;
            JoursBddLayer.InsertNewJour(dbb, date, pointageElt);
            _logger.Debug("FIN - InsertNewJour(...)");
        }

        public JourEntryDto GetJourData(DateTime date)
        {
            _logger.Debug("GetJourDate(date: {0})", date);

            DbbAccessManager dbb = DbbAccessManager.Instance;
            JourEntryDto jDto = JoursBddLayer.GetJourDataNext(dbb, date);

            _logger.Debug("FIN - GetJourDate(...) => {0}", date);

            return jDto;

        }

        public void UpdateTpsTravaille(DateTime dateJourToUpdate, TimeSpan tpsTravaille)
        {
            _logger.Debug("UpdateTpsTravaille (dateJourToUpdate: {0}, tpsTravaille: {1})", dateJourToUpdate, tpsTravaille);
            DbbAccessManager dbb = DbbAccessManager.Instance;

            JourEntryDto jour = JoursBddLayer.GetJourDataNext(dbb, dateJourToUpdate);
            if (jour.IsHydrated)
            {
                jour.TpsTravaille = tpsTravaille;
                JoursBddLayer.UpdateJour(dbb, jour.DateJour, jour);
            }
            //  JoursBddLayer.UpdateTpsTravaille(dbb, date, pointageElt);
            _logger.Debug("FIN - UpdateTpsTravaille (...)");
        }

        public DateTime? GetPreviousDayOf(DateTime currentShowDay)
        {
            DbbAccessManager dbb = DbbAccessManager.Instance;

            JourEntryDto jour = JoursBddLayer.GetPreviousOrNextDayOf(dbb, currentShowDay, true);

            return jour.DateJour;
        }

        public DateTime? GetNextDayOf(DateTime currentShowDay)
        {
            DbbAccessManager dbb = DbbAccessManager.Instance;

            JourEntryDto jour = JoursBddLayer.GetPreviousOrNextDayOf(dbb, currentShowDay, false);

            return jour.DateJour;
        }
    }
}
