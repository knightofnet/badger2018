using System;
using System.IO;
using System.Linq;
using System.Xml;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.xml;
using Badger2018.business.saver;
using Badger2018.constants;
using Badger2018.dto;
using Badger2018.services;

namespace Badger2018.business
{
    public class XmlToBddPointageConverter
    {

        public DirectoryInfo PointageDir { get; private set; }

        private readonly BadgeagesServices _badgeageService;
        private readonly JoursServices _joursServices;


        public XmlToBddPointageConverter(string pointagesDir)
        {
            if (!Directory.Exists(pointagesDir))
            {
                throw new DirectoryNotFoundException("Le dossier des pointages n'existe pas");
            }

            _badgeageService = new BadgeagesServices();
            _joursServices = new JoursServices();

            PointageDir = new DirectoryInfo(pointagesDir);
        }

        public void Convert()
        {
            XmlPointageWriterReader xmlReader = new XmlPointageWriterReader(null);
            BddPointageWriterReader bddWriter = new BddPointageWriterReader(null);
            foreach (FileInfo file in PointageDir.GetFiles("*.xml"))
            {

                PointageElt pElt = xmlReader.ReadXml(file.FullName);

                bddWriter.SaveDatasJours(pElt, DateTime.Parse(pElt.DateDay));
                SaveClassicDayBadgeages(pElt);
                SavePauseBadgeages(pElt);
            }
        }

        private void SaveClassicDayBadgeages(PointageElt pElt)
        {
            int etatBadgeage = pElt.EtatBadger;
            _badgeageService.RemoveBadgeagesOfADay(DateTime.Parse(pElt.DateDay));

            if (etatBadgeage >= EnumBadgeageType.PLAGE_TRAV_MATIN_START.Index)
            {
                //TODO LOG
                _badgeageService.AddBadgeage(EnumBadgeageType.PLAGE_TRAV_MATIN_START.Index, DateTime.Parse(pElt.B0));
            }
            if (etatBadgeage >= EnumBadgeageType.PLAGE_TRAV_MATIN_END.Index)
            {
                //TODO LOG
                _badgeageService.AddBadgeage(EnumBadgeageType.PLAGE_TRAV_MATIN_END.Index, DateTime.Parse(pElt.B1));
            }
            if (etatBadgeage >= EnumBadgeageType.PLAGE_TRAV_APREM_START.Index)
            {
                //TODO LOG
                _badgeageService.AddBadgeage(EnumBadgeageType.PLAGE_TRAV_APREM_START.Index, DateTime.Parse(pElt.B2));
            }
            if (etatBadgeage >= EnumBadgeageType.PLAGE_TRAV_APREM_END.Index)
            {
                //TODO LOG
                _badgeageService.AddBadgeage(EnumBadgeageType.PLAGE_TRAV_APREM_END.Index, DateTime.Parse(pElt.B3));
            }

        }

        private void SavePauseBadgeages(PointageElt pElt)
        {
            if (pElt.Pauses == null || !pElt.Pauses.Any())
            {
                return;
            }

            foreach (IntervalTemps pause in pElt.Pauses)
            {
                string rndStr = StringUtils.RandomString(16);

                _badgeageService.AddBadgeage(EnumBadgeageType.PLAGE_START.Index, pause.Start, rndStr);
                if (pause.IsIntervalComplet())
                {
                    _badgeageService.AddBadgeage(EnumBadgeageType.PLAGE_END.Index, pause.EndOrDft, rndStr);
                }

            }
        }
    }
}
