using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using AryxDevLibrary.utils;
using Badger2018.business;
using Badger2018.constants;
using Badger2018.dto;
using Badger2018.dto.bdd;
using Badger2018.services.bddLastLayer;
using BadgerCommonLibrary.utils;

namespace Badger2018.services
{
    public class BadgeagesServices
    {

        public void AddBadgeageForToday(int typeBadgeage, DateTime dtTimeHeureBadgeage, String relationKey = null)
        {
            DbbAccessManager dbb = DbbAccessManager.Instance;

            DateTime dateTime = AppDateUtils.DtNow().ChangeTime(dtTimeHeureBadgeage.TimeOfDay);

            BadgeageBddLayer.InsertNewBadgeage(dbb, typeBadgeage, dateTime, relationKey);

        }

        public void AddBadgeage(int typeBadgeage, DateTime date, String relationKey = null)
        {
            DbbAccessManager dbb = DbbAccessManager.Instance;



            BadgeageBddLayer.InsertNewBadgeage(dbb, typeBadgeage, date, relationKey);

        }

        public void RemoveBadgeagesOfToday()
        {
            DbbAccessManager dbb = DbbAccessManager.Instance;

            DateTime dateTime = AppDateUtils.DtNow();

            BadgeageBddLayer.RemoveBadgeagesOfAday(dbb, dateTime);
        }

        public bool IsBadgeageExistFor(DateTime date)
        {
            DbbAccessManager dbb = DbbAccessManager.Instance;
            return BadgeageBddLayer.IsBadgeageExistFor(dbb, date);
        }


        public string GetBadgeageOrDft(EnumBadgeageType tyBadgeage, DateTime dt)
        {
            string day = dt.ToString("d");
            string badgeage = GetBadgeageTimeStrFor(tyBadgeage.Index, dt);
            if (StringUtils.IsNullOrWhiteSpace(badgeage))
            {
                return day + " " + TimeSpan.Zero.ToString(Cst.TimeSpanFormat);
            }

            return day + " " + badgeage;
        }

        public string GetBadgeageTimeStrFor(int index, DateTime date)
        {
            DbbAccessManager dbb = DbbAccessManager.Instance;
            return BadgeageBddLayer.GetBadgeageTimeStrFor(dbb, index, date);
        }



        public List<IntervalTemps> GetPauses(DateTime date)
        {
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

            return retList;


        }

        public TimeSpan GetBadgeMedianneTime(EnumBadgeageType tyBadgeage, int nbJours)
        {
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
            DbbAccessManager dbb = DbbAccessManager.Instance;
            List<BadgeageEntryDto> listBadgeage = BadgeageBddLayer.GetBadgeMedianneTime(dbb, tyBadgeage.Index,
                nbJours);

            if (!listBadgeage.Any())
            {
                return null;
            }

            List<double> lstDec = new List<double>(listBadgeage.Count);
            lstDec.AddRange(listBadgeage.Select(badgeageEntryDto => badgeageEntryDto.DateTime.TimeOfDay.TotalSeconds));




            double sumlstDec = lstDec.Sum(x => x);
            double moy = sumlstDec / lstDec.Count;


            return TimeSpan.FromSeconds(moy);


        }
    }
}
