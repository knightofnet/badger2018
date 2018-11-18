using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using Badger2018.business;
using Badger2018.constants;
using Badger2018.dto;
using Badger2018.dto.bdd;
using Badger2018.services.bddLastLayer;
using Badger2018.utils;
using BadgerCommonLibrary.utils;

namespace Badger2018.services
{
    public class BadgeagesServices
    {
        private static readonly Logger _logger = Logger.LastLoggerInstance;

        public void AddBadgeageForToday(int typeBadgeage, DateTime dtTimeHeureBadgeage, String relationKey = null)
        {
            _logger.Debug("AddBadgeageForToday (typeBadgeage: {0}, dtTimeHeureBadgeage: {1}, relationKey: {2})", typeBadgeage, dtTimeHeureBadgeage, relationKey);

            DbbAccessManager dbb = DbbAccessManager.Instance;

            DateTime dateTime = AppDateUtils.DtNow().ChangeTime(dtTimeHeureBadgeage.TimeOfDay);

            BadgeageBddLayer.InsertNewBadgeage(dbb, typeBadgeage, dateTime, relationKey);
            _logger.Debug("FIN - AddBadgeageForToday(...)");
        }

        public void AddBadgeage(int typeBadgeage, DateTime date, String relationKey = null)
        {
            _logger.Debug("AddBadgeage (typeBadgeage: {0}, dtTimeHeureBadgeage: {1}, relationKey: {2})", typeBadgeage, date, relationKey);

            DbbAccessManager dbb = DbbAccessManager.Instance;


            BadgeageBddLayer.InsertNewBadgeage(dbb, typeBadgeage, date, relationKey);
            _logger.Debug("FIN - AddBadgeage(...)");

        }

        public void RemoveBadgeagesOfADay(DateTime date)
        {
            _logger.Debug("RemoveBadgeagesOfADay(date: {0})", date);
            DbbAccessManager dbb = DbbAccessManager.Instance;


            BadgeageBddLayer.RemoveBadgeagesOfAday(dbb, date);
            _logger.Debug("FIN - RemoveBadgeagesOfADay(...)");
        }

        public bool IsBadgeageExistFor(DateTime date)
        {
            _logger.Debug("IsBadgeageExistFor(date: {0})", date);

            DbbAccessManager dbb = DbbAccessManager.Instance;
            bool result = BadgeageBddLayer.IsBadgeageExistFor(dbb, date);

            _logger.Debug("FIN - IsBadgeageExistFor(...) => {0}", result);

            return result;
        }


        public string GetBadgeageOrDft(EnumBadgeageType tyBadgeage, DateTime dt)
        {
            _logger.Debug("GetBadgeageOrDft(tyBadgeage: {0}, dt: {1})", tyBadgeage.Libelle, dt);
            string day = dt.ToString("d");

            string retStr = "";

            DbbAccessManager dbb = DbbAccessManager.Instance;
            List<BadgeageEntryDto> endEntries = BadgeageBddLayer.GetListBadgeagesOf(dbb, dt, tyBadgeage.Index, null);
            if (endEntries.Count > 1)
            {
                throw new Exception("Trop d'instances retournées");
            } else if (endEntries.Count == 0)
            {
                _logger.Info("Aucun retour. Badgeage en défaut");
                retStr = day + " " + TimeSpan.Zero.ToString(Cst.TimeSpanFormat);
            } else
            {
                retStr = endEntries[0].DateTime.ToString("s");
            }

            _logger.Debug("FIN - GetBadgeageOrDft(...) => {0}", retStr);

            return retStr;
        }

        public string GetBadgeageTimeStrFor(int index, DateTime date)
        {
            _logger.Debug("GetBadgeageTimeStrFor(index: {0}, date: {1})", index, date);
            DbbAccessManager dbb = DbbAccessManager.Instance;
            string retStr = BadgeageBddLayer.GetBadgeageTimeStrFor(dbb, index, date);

            _logger.Debug("FIN - GetBadgeageTimeStrFor(...) => {0}", retStr);
            return retStr;
            
        }



        public List<IntervalTemps> GetPauses(DateTime date)
        {
            _logger.Debug("GetPauses(date: {0})", date);
            List<IntervalTemps> retList = new List<IntervalTemps>();

            DbbAccessManager dbb = DbbAccessManager.Instance;

            List<BadgeageEntryDto> listStartPause = BadgeageBddLayer.GetListBadgeagesOf(dbb, date, EnumBadgeageType.PLAGE_START.Index, null);
            if (!listStartPause.Any()) return retList;

            foreach (BadgeageEntryDto pauseEntry in listStartPause)
            {
                IntervalTemps ivl = new IntervalTemps();
                ivl.Start = pauseEntry.DateTime;

                if (BadgeageBddLayer.IsBadgeageExistFor(dbb, date, EnumBadgeageType.PLAGE_END.Index, pauseEntry.RelationKey))
                {
                    List<BadgeageEntryDto> endEntries = BadgeageBddLayer.GetListBadgeagesOf(dbb, date,
                        EnumBadgeageType.PLAGE_END.Index, pauseEntry.RelationKey);
                    if (endEntries.Count > 1)
                    {
                        throw new Exception("Trop d'instances retournées");
                    }
                    ivl.End = endEntries[0].DateTime;
                }
                else
                {
                    ivl.End = null;
                }

                retList.Add(ivl);
            }

            _logger.Debug("FIN - GetPauses(...) => {0}", date);

            return retList;


        }

        public TimeSpan GetBadgeMedianneTime(EnumBadgeageType tyBadgeage, int nbJours)
        {
            _logger.Debug("");
            DbbAccessManager dbb = DbbAccessManager.Instance;
            List<BadgeageEntryDto> listBadgeage = BadgeageBddLayer.GetBadgeMedianneTime(dbb, tyBadgeage.Index,
                nbJours);

            List<double> lstDec = new List<double>(listBadgeage.Count);
            lstDec.AddRange(listBadgeage.Select(badgeageEntryDto => badgeageEntryDto.DateTime.TimeOfDay.TotalSeconds));

            lstDec = lstDec.OrderBy(x => x).ToList();
            double mid = (lstDec.Count - 1) / 2.0;

            double median = (lstDec[(int)mid] + lstDec[(int)(mid + 0.5)]) / 2;



            return TimeSpan.FromSeconds(median);
        }

        public TimeSpan? GetBadgeMoyenneTime(EnumBadgeageType tyBadgeage, int nbJours)
        {
            _logger.Debug("GetBadgeMoyenneTime(tyBadgeage: {0}, nbJour: {1})", tyBadgeage.Libelle,nbJours);
            DbbAccessManager dbb = DbbAccessManager.Instance;
            List<BadgeageEntryDto> listBadgeage = BadgeageBddLayer.GetBadgeMedianneTime(dbb, tyBadgeage.Index,
                nbJours);

            if (!listBadgeage.Any())
            {
                return null;
            }

            List<double> lstDec = new List<double>(listBadgeage.Count);
            
            lstDec.AddRange(listBadgeage.Select(badgeageEntryDto => badgeageEntryDto.DateTime.TimeOfDay.TotalSeconds));


            double ec = MiscAppUtils.EcartType(lstDec);
            

            double sumlstDec = lstDec.Sum(x => x);
            double moy = sumlstDec / lstDec.Count;
            const double V = 1.645;
            int c = lstDec.Count(r => r >= (moy - V * ec) && r <= (moy + V * ec));
            if (c > 1)
            {
                lstDec = lstDec.Where(r => r >= (moy - V * ec) && r <= (moy + V * ec)).ToList();
                moy = MiscAppUtils.Moyenne(lstDec);
            }

            TimeSpan tsRet = TimeSpan.FromSeconds(moy);

            _logger.Debug("FIN - GetBadgeMoyenneTime(...) => {0}", tsRet);

            return tsRet;


        }
    }
}
