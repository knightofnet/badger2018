using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Badger2018.business;
using Badger2018.constants;
using Badger2018.dto;
using Badger2018.dto.bdd;
using Badger2018.services.bddLastLayer;

namespace Badger2018.services
{
    class JoursServices
    {

        public bool IsJourExistFor(DateTime dtNow)
        {
            DbbAccessManager dbb = DbbAccessManager.Instance;


            return JoursBddLayer.IsJourExistFor(dbb, dtNow);
        }

        public void UpdateJourWithPointageElt(DateTime date, PointageElt pointageElt)
        {
            DbbAccessManager dbb = DbbAccessManager.Instance;

            JourEntryDto j = new JourEntryDto();
            j.DateJour = date;
            j.EtatBadger = pointageElt.EtatBadger;
            j.OldEtatBadger = pointageElt.OldEtatBadger;
            j.IsComplete = pointageElt.IsComplete;
            j.TypeJour = EnumTypesJournees.GetFromIndex(pointageElt.TypeJournee);

            JoursBddLayer.UpdateJour(dbb, date, j);
        }

        public void InsertNewJour(DateTime date, PointageElt pointageElt)
        {
            DbbAccessManager dbb = DbbAccessManager.Instance;
            JoursBddLayer.InsertNewJour(dbb, date, pointageElt);
        }

        public JourEntryDto GetJourData(DateTime date)
        {
            DbbAccessManager dbb = DbbAccessManager.Instance;
            return JoursBddLayer.GetJourDataNext(dbb, date);
        }

        public void UpdateTpsTravaille(DateTime dateJourToUpdate, TimeSpan tpsTravaille)
        {
            DbbAccessManager dbb = DbbAccessManager.Instance;

            JourEntryDto jour = JoursBddLayer.GetJourDataNext(dbb, dateJourToUpdate);
            if (jour.IsHydrated)
            {
                jour.TpsTravaille = tpsTravaille;
                JoursBddLayer.UpdateJour(dbb, jour.DateJour, jour);
            }
            //  JoursBddLayer.UpdateTpsTravaille(dbb, date, pointageElt);
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
