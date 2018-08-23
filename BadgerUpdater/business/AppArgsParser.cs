using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AryxDevLibrary.utils.cliParser;
using BadgerUpdater.dto;

namespace BadgerUpdater.business
{

    public class AppArgsParser : CliParser<AppArgsDto>
    {

        private static readonly Option _launchAppIfSucessOption = new Option()
        {
            ShortOpt = "l",
            LongOpt = "launch-app",
            Description = "Lance Badger2018 si le traitement réussi",
            HasArgs = false,
            IsMandatory = false,
            Name = "launchAppIfSucess"
        };

        private static readonly Option _versionTargetOption = new Option()
        {
            ShortOpt = "v",
            LongOpt = "version-cible",
            Description = "Indique vers quelle version mettre à jour l'application. Indiquer '*' pour mettre à jour vers le dernière version. '*' par défaut.",
            HasArgs = true,
            IsMandatory = false,
            Name = "versionTarget"
        };

        private static readonly Option _configFilePathOption = new Option()
        {
            ShortOpt = "f",
            LongOpt = "config-filepath",
            Description = "Chemin vers le fichier XML de mise à jour",
            HasArgs = true,
            IsMandatory = true,
            Name = "configFilePath"
        };

        private static readonly Option _appFilePathOption = new Option()
        {
            ShortOpt = "a",
            LongOpt = "app-filepath",
            Description = "Chemin vers le fichier exe ",
            HasArgs = true,
            IsMandatory = true,
            Name = "appFilePath"
        };

        private static readonly Option _forceLogDebugOption = new Option()
        {
            ShortOpt = "d",
            LongOpt = "force-log-debug",
            Description = "Force la journalisation des log débug.",
            HasArgs = false,
            IsMandatory = false,
            Name = "_forceLogDebugOption"
        };



        public AppArgsParser()
        {
            AddOption(_launchAppIfSucessOption);
            AddOption(_versionTargetOption);
            AddOption(_configFilePathOption);
            AddOption(_forceLogDebugOption);
            AddOption(_appFilePathOption);


        }


        private AppArgsDto ParseTrt(Dictionary<string, Option> dictionary)
        {
            AppArgsDto appArgsDto = new AppArgsDto();

            if (HasOption(_versionTargetOption, dictionary))
            {
                appArgsDto.VergionTarget = GetSingleOptionValue(_versionTargetOption, dictionary);
            }
            else
            {
                appArgsDto.VergionTarget = "*";
            }

            if (HasOption(_configFilePathOption, dictionary))
            {
                string configFilePah = GetSingleOptionValue(_configFilePathOption, dictionary);
                if (!File.Exists(configFilePah))
                {
                    throw new CliParsingException(String.Format("Le fichier indiqué avec le paramètre -{0} n'existe pas. ({1})", _configFilePathOption.ShortOpt.ToString(), configFilePah));
                }
                appArgsDto.XmlUpdateFile = configFilePah;
            }

            if (HasOption(_appFilePathOption, dictionary))
            {
                string exeFilePath = GetSingleOptionValue(_appFilePathOption, dictionary);
                if (!File.Exists(exeFilePath))
                {
                    throw new CliParsingException(String.Format("Le fichier indiqué avec le paramètre -{0} n'existe pas. ({1})", _appFilePathOption.ShortOpt.ToString(), exeFilePath));
                }
                appArgsDto.BadgerAppExe = exeFilePath;


            }

            appArgsDto.LaunchAppIfSucess = HasOption(_launchAppIfSucessOption, dictionary);



            return appArgsDto;
        }



        public override AppArgsDto ParseDirect(string[] args)
        {

            return Parse(args, ParseTrt);

        }
    }
}

