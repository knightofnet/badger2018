using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Windows;
using System.Xml;
using AryxDevLibrary.extensions;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using AryxDevLibrary.utils.xml;
using Badger2018.constants;
using Badger2018.dto;
using Badger2018.Properties;
using Badger2018.views;
using BadgerCommonLibrary.utils;

namespace Badger2018.business
{
    class OptionManager
    {
        private static readonly Logger _logger = Logger.LastLoggerInstance;

        public static AppOptions LoadOptions()
        {
            AppOptions opt = null;

            try
            {

                if (!IsOptionsExists())
                {
                    InitSettingsOptions();
                }

                opt = LoadFromSettings();

            }
            catch (Exception e)
            {
                ExceptionHandlingUtils.LogAndRethrows(e, "Une erreur est survenue lors de la récupération des options.");

            }

            return opt;

        }

        /// <summary>
        /// Charge les paramètres à partir de la config utilisateur
        /// </summary>
        /// <returns></returns>
        private static AppOptions LoadFromSettings()
        {
            AppOptions opt = new AppOptions();
            String eOptCurrent = null;

            try
            {

                foreach (EnumAppOptions eOpt in EnumAppOptions.Values)
                {
                    eOptCurrent = eOpt.Name;

                    PropertyInfo propertyInfo = opt.GetType().GetProperty(eOpt.Name);
                    if (propertyInfo == null)
                    {
                        throw new Exception("La propriété " + eOptCurrent + " n'existe pas dans " + typeof(AppOptions).Name);
                    }

                    if (propertyInfo.PropertyType == typeof(TimeSpan))
                    {
                        propertyInfo.SetValue(opt, GetTimeSpanSettingsOpt(eOpt.Name), null);
                    }
                    else if (propertyInfo.PropertyType == typeof(String))
                    {
                        propertyInfo.SetValue(opt, GetStringSettingsOpt(eOpt.Name), null);
                    }
                    else if (propertyInfo.PropertyType == typeof(bool))
                    {
                        propertyInfo.SetValue(opt, GetBooleanSettingsOpt(eOpt.Name), null);
                    }
                    else if (propertyInfo.PropertyType == typeof(int))
                    {
                        propertyInfo.SetValue(opt, GetIntSettingsOpt(eOpt.Name), null);
                    }
                    else if (
                        propertyInfo.PropertyType.GetInterfaces()
                            .Any(r => r.Name == typeof(IEnumSerializableWithIndex<>).Name))
                    {
                        var value = EnumAppOptions.GetEnumFromIndex(GetIntSettingsOpt(eOpt.Name),
                            propertyInfo.PropertyType);
                        propertyInfo.SetValue(opt, value, null);
                    }

                }

            }
            catch (Exception e)
            {
                MessageBox.Show(
    "Une erreur s'est produite lors du chargement des paramètres. Le programme ne peut pas démarrer. Consulter le journal pour plus d'informations",
    "Erreur avec  l'option: " + eOptCurrent, MessageBoxButton.OK, MessageBoxImage.Error);

                ExceptionHandlingUtils.LogAndRethrows(e, "Une erreur s'est produite lors du chargement des paramètres. Le programme ne peut pas démarrer. Consulter le journal pour plus d'informations");

            }

            return opt;
        }

        private static void InitSettingsOptions()
        {
            AppOptions optD = GetDefaultOptionObj();

            DtoToSettings(optD);

            Settings.Default["First"] = "ok";

            Settings.Default.Save();

        }

        /// <summary>
        /// Sauvegarde les paramètres dans la config utilisateur
        /// </summary>
        /// <param name="optD"></param>
        private static void DtoToSettings(AppOptions optD)
        {

            foreach (EnumAppOptions eOpt in EnumAppOptions.Values)
            {
                PropertyInfo propertyInfo = optD.GetType().GetProperty(eOpt.Name);

                object value = propertyInfo.GetValue(optD, null);

                if (
                    propertyInfo.PropertyType.GetInterfaces()
                        .Any(r => r.Name == typeof(IEnumSerializableWithIndex<>).Name))
                {
                    value = EnumAppOptions.GetIndexFromEnum(value);
                }

                SetOptSetting(eOpt.Name, value);

            }


        }

        /// <summary>
        /// Sauvegarde les paramètres dans un fichier XML
        /// </summary>
        /// <param name="optD"></param>
        /// <param name="fileXml"></param>
        private static void DtoToSerializableDto(AppOptions optD, String fileXml)
        {
            XmlFile x = XmlFile.NewFromEmpty(fileXml, Cst.XmlRootName);

            foreach (EnumAppOptions eOpt in EnumAppOptions.Values.Where(r => !r.IsSpec))
            {
                PropertyInfo propertyInfo = optD.GetType().GetProperty(eOpt.Name);

                object value = propertyInfo.GetValue(optD, null);

                if (
                    propertyInfo.PropertyType.GetInterfaces()
                        .Any(r => r.Name == typeof(IEnumSerializableWithIndex<>).Name))
                {
                    value = EnumAppOptions.GetIndexFromEnum(value);
                }

                SetXmlSetting(eOpt.Name, value, x);

            }

            x.Save();
        }

        /// <summary>
        /// Charge les paramètres à partir d'un fichier xml
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static AppOptions LoadFromXml(string fileName)
        {
            XmlFile x = XmlFile.InitXmlFile(fileName);

            if (!x.Root.Name.Equals(Cst.XmlRootName))
            {

                ExceptionHandlingUtils.LogAndNewException(String.Format("Erreur lors de l'importation des paramètres à partir du fichier XML : la racine du fichier xml n'est pas {0}", Cst.XmlRootName));
            }

            AppOptions opt = new AppOptions();
            String pName = null;

            foreach (EnumAppOptions eOpt in EnumAppOptions.Values)
            {
                pName = eOpt.Name;

                try
                {
                    PropertyInfo propertyInfo = opt.GetType().GetProperty(eOpt.Name);

                    String xmlValue = XmlUtils.GetValueXpath(x.Root, String.Format("//{0}", pName));
                    if (xmlValue == null)
                    {
                        continue;
                    }

                    if (propertyInfo.PropertyType == typeof(TimeSpan))
                    {
                        propertyInfo.SetValue(opt, TimeSpan.Parse(xmlValue), null);
                    }
                    else if (propertyInfo.PropertyType == typeof(String))
                    {
                        propertyInfo.SetValue(opt, xmlValue, null);
                    }
                    else if (propertyInfo.PropertyType == typeof(bool))
                    {
                        propertyInfo.SetValue(opt, Boolean.Parse(xmlValue), null);
                    }
                    else if (propertyInfo.PropertyType == typeof(int))
                    {
                        propertyInfo.SetValue(opt, Int16.Parse(xmlValue), null);
                    }
                    else if (
                        propertyInfo.PropertyType.GetInterfaces()
                            .Any(r => r.Name == typeof(IEnumSerializableWithIndex<>).Name))
                    {
                        var value = EnumAppOptions.GetEnumFromIndex(Int16.Parse(xmlValue), propertyInfo.PropertyType);
                        propertyInfo.SetValue(opt, value, null);
                    }

                }
                catch (Exception ex)
                {
                    ExceptionHandlingUtils.LogAndNewException(
                        String.Format("Erreur lors de l'importation des paramètres à partir du fichier XML (paramètre lu: {0}.)", pName),
                        ex);


                }

            }


            return opt;
        }




        private static void SetXmlSetting(string key, object value, XmlFile xmlFile)
        {
            XmlElement elt = xmlFile.Doc.CreateElement(key);
            elt.InnerText = value.ToString();
            xmlFile.Root.AppendChild(elt);
        }

        public static AppOptions GetDefaultOptionObj()
        {

            AppOptions opt = new AppOptions();

            String eOptCurrent = "";
            try
            {

                foreach (EnumAppOptions eOpt in EnumAppOptions.Values)
                {
                    eOptCurrent = eOpt.Name;
                    PropertyInfo propertyInfo = opt.GetType().GetProperty(eOpt.Name);

                    object value = eOpt.GetObjDefaultValue();


                    propertyInfo.SetValue(opt, value, null);
                }
            }
            catch (Exception e)
            {
                ExceptionHandlingUtils.LogAndHideException(e, String.Format("Erreur lors de la RaZ des paramètres (paramètre en cours de traitement: {0})", eOptCurrent));

                MessageBox.Show(
                    "Une erreur s'est produite lors de la remise à zéro des paramètres. L'action est annulée. Consulter le journal pour plus d'informations",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);

                return null;

            }

            return opt;

        }

        private static bool IsOptionsExists()
        {
            String a = (String)Settings.Default["First"];
            return !a.IsEmpty();
        }

        private static void SetOptSetting(String key, object value)
        {
            Settings.Default[key] = value;
        }

        private static TimeSpan GetTimeSpanSettingsOpt(String optName)
        {
            return (TimeSpan)Settings.Default[optName];

        }

        private static bool GetBooleanSettingsOpt(String optName)
        {
            return (bool)Settings.Default[optName];
        }

        private static String GetStringSettingsOpt(String optName)
        {
            return (String)Settings.Default[optName];
        }

        private static int GetIntSettingsOpt(String optName)
        {
            return (int)Settings.Default[optName];
        }

        public static void SaveOptions(AppOptions appOptions)
        {
            DtoToSettings(appOptions);
            Settings.Default.Save();
        }

        public static void SaveOptionsToXml(AppOptions appOptions, String fileName)
        {
            DtoToSerializableDto(appOptions, fileName);
        }

        public static void ChangeOptions(AppOptions newOptions, AppOptions oldOptions, string fileName)
        {
            String optName = null;
            try
            {
                XmlFile x = XmlFile.InitXmlFile(fileName);


                foreach (EnumAppOptions eOpt in EnumAppOptions.Values)
                {
                    optName = eOpt.Name;

                    if (!XmlUtils.HasElement(x.Root, String.Format("//{0}", eOpt.Name)))
                    {
                        continue;
                    }


                    PropertyInfo propertyInfoA = newOptions.GetType().GetProperty(eOpt.Name);
                    PropertyInfo propertyInfoB = oldOptions.GetType().GetProperty(eOpt.Name);

                    object valueA = propertyInfoA.GetValue(newOptions, null);
                    object valueB = propertyInfoB.GetValue(oldOptions, null);

                    if (!valueA.Equals(valueB))
                    {
                        _logger.Debug("Modification de l'option {0}. Ancienne valeur {1}, nouvelle valeur {2}", optName, valueB.ToString(), valueA.ToString());
                        propertyInfoB.SetValue(oldOptions, valueA, null);
                    }


                }
            }
            catch (Exception e)
            {
                _logger.Error("Erreur lors du test pour {0}", optName);

                throw e;
            }
        }


    }
}
