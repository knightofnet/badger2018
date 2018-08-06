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

        private static readonly Option _versionTargetOption = new Option()
        {
            ShortOpt = "v",
            LongOpt = "version-cible",
            Description = "Indique vers quelle version mettre à jour l'application. Indiquer '*' pour mettre à jours vers le dernière version. '*' par défaut.",
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
            IsMandatory = false,
            Name = "configFilePath"
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
            AddOption(_versionTargetOption);
            AddOption(_configFilePathOption);
            AddOption(_forceLogDebugOption);


        }


        private AppArgsDto ParseTrt(Dictionary<string, Option> dictionary)
        {
            AppArgsDto appArgsDto = new AppArgsDto();

            if (HasOption(_versionTargetOption, dictionary))
            {


                appArgsDto.VergionTarget = GetSingleOptionValue(_versionTargetOption, dictionary);
            }





            return appArgsDto;
        }



        public override AppArgsDto ParseDirect(string[] args)
        {

            return Parse(args, ParseTrt);

        }
    }
}

