using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using AryxDevLibrary.utils.xml;
using Badger2018.constants;
using Badger2018.dto;
using Badger2018.utils;

namespace Badger2018.business.saver
{
    class XmlPointageWriterReader : ISaverTimes
    {
        private static readonly Logger _logger = Logger.LastLoggerInstance;

        private readonly MainWindow _pWinRef;

        public XmlPointageWriterReader(MainWindow pwin)
        {
            _pWinRef = pwin;
        }

        public void SaveCurrentDayTimes()
        {
            DbbAccessManager.Instance.StartTransaction();

            try
            {
                String sFile = Cst.PointagesDirName + "/" + MiscAppUtils.GetFileNamePointageCurrentDay(_pWinRef.Times.TimeRef);

                PointageElt pElt = new PointageElt
                {
                    DateDay = _pWinRef.RealTimeDtNow.ToString(),
                    EtatBadger = _pWinRef.EtatBadger,
                    OldEtatBadger = _pWinRef.OldEtatBadger,
                    TypeJournee = _pWinRef.TypeJournee.Index,
                    IsNotif1Showed = _pWinRef.NotifManager.IsNotifShow(Cst.NotifCust1Name),
                    IsNotif2Showed = _pWinRef.NotifManager.IsNotifShow(Cst.NotifCust2Name),

                };

                SaveBadgeages(pElt);

                _logger.Debug("Sauvegarde : {0}", pElt.ToString());

                WriteXml(pElt, sFile);

                DbbAccessManager.Instance.StopAndCommitTransaction();
            }
            catch (Exception ex)
            {
                DbbAccessManager.Instance.StopAndRollbackTransaction();

                _logger.Error("Erreur lors de la sauvegarde des horaires");
                _logger.Error(ex.Message);
                _logger.Error(ex.StackTrace);
                throw ex;
            }

        }


        public bool MustReloadIncomplete()
        {
            string sFile = Cst.PointagesDirName + "/" + MiscAppUtils.GetFileNamePointageCurrentDay(_pWinRef.Times.TimeRef);
            _logger.Debug("Test de l'existence du fichier " + sFile);
            return File.Exists(sFile);
        }

        public PointageElt LoadIncomplete()
        {
            string sFile = Cst.PointagesDirName + "/" + MiscAppUtils.GetFileNamePointageCurrentDay(_pWinRef.Times.TimeRef);

            if (File.Exists(sFile))
            {
                // PointageElt pointageElt = SerializeUtils.Deserialize<PointageElt>(sFile);
                PointageElt pointageElt = ReadXml(sFile);
                return pointageElt;
            }
            return null;

        }



        private void SaveBadgeages(PointageElt pElt)
        {

            if (_pWinRef.EtatBadger >= 0)
            {
                pElt.B0 = _pWinRef.Times.PlageTravMatin.Start.ToString();

            }
            if (_pWinRef.EtatBadger >= 1)
            {
                pElt.B1 = _pWinRef.Times.PlageTravMatin.EndOrDft.ToString();

            }
            if (_pWinRef.EtatBadger >= 2)
            {
                pElt.B2 = _pWinRef.Times.PlageTravAprem.Start.ToString();

            }
            if (_pWinRef.EtatBadger >= 3)
            {
                pElt.B3 = _pWinRef.Times.PlageTravAprem.EndOrDft.ToString();

            }

            pElt.Pauses = _pWinRef.Times.PausesHorsDelai;
        }




        private bool GetXmlBooleanValue(XmlNode root, string iscomplete)
        {
            try
            {
                string rawValue = XmlUtils.GetValueXpathOrDefault(root, iscomplete, Boolean.FalseString);
                return Boolean.Parse(rawValue);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private int GetXmlIntValue(XmlNode root, string iscomplete)
        {
            try
            {
                string rawValue = XmlUtils.GetValueXpathOrDefault(root, iscomplete, "0");
                return Int16.Parse(rawValue);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private DateTime GetXmlDateValue(XmlNode root, string iscomplete)
        {
            try
            {
                string rawValue = XmlUtils.GetValueXpathOrDefault(root, iscomplete, null);
                if (StringUtils.IsNullOrWhiteSpace(rawValue))
                {
                    return (DateTime)ReflexionUtils.GetDefaultValue(typeof(DateTime));
                }
                return DateTime.Parse(rawValue);
            }
            catch (Exception)
            {
                throw new FormatException("Erreur lors de la lecture d'un DateTime à partir d'une string");
            }
        }

        private DateTime? GetXmlDateValueNullable(XmlNode root, string iscomplete)
        {
            try
            {
                string rawValue = XmlUtils.GetValueXpathOrDefault(root, iscomplete, null);
                if (StringUtils.IsNullOrWhiteSpace(rawValue))
                {
                    return null;
                }
                return DateTime.Parse(rawValue);
            }
            catch (Exception)
            {
                throw new FormatException("Erreur lors de la lecture d'un DateTime à partir d'une string");
            }
        }

        private static void WriteXml(PointageElt pElt, string sFile)
        {
            /*
            String a = SerializeUtils.Serialize(pElt);
            using (StreamWriter sw = new StreamWriter(sFile))
            {
                sw.Write(XmlUtils.FormatXmlString(a));
            }
             */

            XmlFile xmlFile = XmlFile.NewFromEmpty(sFile, pElt.GetType().Name);

            XmlUtils.CreateSetValueAndAppendTo("IsComplete", pElt.IsComplete.ToString(), xmlFile.Doc, xmlFile.Root);
            if (pElt.DateDay != null)
            {
                XmlUtils.CreateSetValueAndAppendTo("DateDay", pElt.DateDay.ToString(), xmlFile.Doc, xmlFile.Root);
            }

            XmlUtils.CreateSetValueAndAppendTo("EtatBadger", pElt.EtatBadger.ToString(), xmlFile.Doc, xmlFile.Root);
            XmlUtils.CreateSetValueAndAppendTo("OldEtatBadger", pElt.OldEtatBadger.ToString(), xmlFile.Doc, xmlFile.Root);

            if (pElt.B0 != null)
            {
                XmlUtils.CreateSetValueAndAppendTo("B0", pElt.B0, xmlFile.Doc, xmlFile.Root);
            }
            if (pElt.B1 != null)
            {
                XmlUtils.CreateSetValueAndAppendTo("B1", pElt.B1, xmlFile.Doc, xmlFile.Root);
            }
            if (pElt.B2 != null)
            {
                XmlUtils.CreateSetValueAndAppendTo("B2", pElt.B2, xmlFile.Doc, xmlFile.Root);
            }
            if (pElt.B3 != null)
            {
                XmlUtils.CreateSetValueAndAppendTo("B3", pElt.B3, xmlFile.Doc, xmlFile.Root);
            }
            XmlUtils.CreateSetValueAndAppendTo("TypeJournee", pElt.TypeJournee.ToString(), xmlFile.Doc, xmlFile.Root);
            XmlUtils.CreateSetValueAndAppendTo("IsNotif1Showed", pElt.IsNotif1Showed.ToString(), xmlFile.Doc, xmlFile.Root);
            XmlUtils.CreateSetValueAndAppendTo("IsNotif2Showed", pElt.IsNotif2Showed.ToString(), xmlFile.Doc, xmlFile.Root);

            if (pElt.Pauses != null && pElt.Pauses.Any())
            {
                XmlElement EltListPauses = xmlFile.Doc.CreateElement("Pauses");

                foreach (IntervalTemps pause in pElt.Pauses)
                {
                    XmlElement EltPause = xmlFile.Doc.CreateElement("Pause");
                    XmlUtils.CreateSetValueAndAppendTo("Start", pause.Start.ToString(), xmlFile.Doc, EltPause);
                    if (pause.End.HasValue)
                    {
                        XmlUtils.CreateSetValueAndAppendTo("End", pause.End.ToString(), xmlFile.Doc, EltPause);
                    }

                    EltListPauses.AppendChild(EltPause);
                }

                xmlFile.Root.AppendChild(EltListPauses);
            }

            xmlFile.Save();

        }

        public PointageElt ReadXml(string sFile)
        {
            PointageElt retPointageElt = new PointageElt();

            XmlFile xmlFile = XmlFile.InitXmlFile(sFile);


            retPointageElt.IsComplete = GetXmlBooleanValue(xmlFile.Root, "./IsComplete");
            retPointageElt.DateDay = XmlUtils.GetValueXpathOrDefault(xmlFile.Root, "./DateDay", null);
            retPointageElt.EtatBadger = GetXmlIntValue(xmlFile.Root, "./EtatBadger");
            retPointageElt.OldEtatBadger = GetXmlIntValue(xmlFile.Root, "./OldEtatBadger");
            retPointageElt.B0 = XmlUtils.GetValueXpathOrDefault(xmlFile.Root, "./B0", null);
            retPointageElt.B1 = XmlUtils.GetValueXpathOrDefault(xmlFile.Root, "./B1", null);
            retPointageElt.B2 = XmlUtils.GetValueXpathOrDefault(xmlFile.Root, "./B2", null);
            retPointageElt.B3 = XmlUtils.GetValueXpathOrDefault(xmlFile.Root, "./B3", null);
            retPointageElt.TypeJournee = GetXmlIntValue(xmlFile.Root, "./TypeJournee");
            retPointageElt.IsNotif1Showed = GetXmlBooleanValue(xmlFile.Root, "./IsNotif1Showed");
            retPointageElt.IsNotif2Showed = GetXmlBooleanValue(xmlFile.Root, "./IsNotif2Showed");

            XmlNodeList xmlPausesElt = XmlUtils.GetNodesXpath(xmlFile.Root, "./Pauses/Pause");

            if (xmlPausesElt.Count > 0)
            {
                retPointageElt.Pauses = new List<IntervalTemps>();

                foreach (XmlElement xmlPause in xmlPausesElt)
                {
                    IntervalTemps ivlTemps = new IntervalTemps();

                    if (XmlUtils.GetValueXpathOrDefault(xmlPause, "./Start", null) == null)
                    {
                        throw new Exception(
                            "Erreur lors du parsing du fichier xml des pointage : la balise Start d'une pause est absente");
                    }
                    ivlTemps.Start = GetXmlDateValue(xmlPause, "./Start");
                    ivlTemps.End = GetXmlDateValueNullable(xmlPause, "./End");

                    retPointageElt.Pauses.Add(ivlTemps);
                }

            }

            return retPointageElt;

        }

    }
}
