using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.cliParser;
using Badger2018.dto;

namespace Badger2018.business
{
    public class AppArgsParser : CliParser<AppArgsDto>
    {

        private static readonly Option _exportConfigFilePath = new Option()
        {
            ShortOpt = "e",
            LongOpt = "export-config",
            Description = "Exporte la configuration dans le fichier précisé en argument de cette option.",
            HasArgs = true,
            IsMandatory = false,
            Name = "exportConfigFilePath"
        };

        private static readonly Option _importConfigFilePath = new Option()
        {
            ShortOpt = "i",
            LongOpt = "import-config",
            Description = "Importe la configuration dans le fichier précisé en argument de cette option.",
            HasArgs = true,
            IsMandatory = false,
            Name = "importConfigFilePath"
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

        private static readonly Option _loadAfterExpImpOption = new Option()
        {
            ShortOpt = "l",
            LongOpt = "loadAfterExpImp",
            Description = "Charge le programme après les tâches d'export/import",
            HasArgs = false,
            IsMandatory = false,
            Name = "_loadAfterExpImpOption"
        };


        public AppArgsParser()
        {
            AddOption(_exportConfigFilePath);
            AddOption(_importConfigFilePath);
            AddOption(_forceLogDebugOption);
            AddOption(_loadAfterExpImpOption);

        }


        private AppArgsDto ParseTrt(Dictionary<string, Option> dictionary)
        {
            AppArgsDto appArgsDto = new AppArgsDto();

            if (HasOption(_exportConfigFilePath, dictionary))
            {
                FileInfo filePath = new FileInfo(GetSingleOptionValue(_exportConfigFilePath, dictionary));
                if (filePath.Directory == null || !filePath.Directory.Exists)
                {
                    throw new CliParsingException(
                        "Le dossier parent du fichier précisé avec l'option d'exportation n'existe pas");
                }

                appArgsDto.ExportConfFilePath = filePath.FullName;
            }

            if (HasOption(_importConfigFilePath, dictionary))
            {
                FileInfo filePath = new FileInfo(GetSingleOptionValue(_importConfigFilePath, dictionary));
                if (!filePath.Exists)
                {
                    throw new CliParsingException(
                        "Le fichier précisé avec l'option d'importation n'existe pas");
                }

                appArgsDto.ImportConfFilePath = GetSingleOptionValue(_importConfigFilePath, dictionary);
            }

            if (HasOption(_forceLogDebugOption, dictionary))
            {
                appArgsDto.IsForceLogDebug = true;
            }

            if (HasOption(_loadAfterExpImpOption, dictionary))
            {
                appArgsDto.LoadAfterImportExport = true;
            }




            return appArgsDto;
        }



        public override AppArgsDto ParseDirect(string[] args)
        {

            return Parse(args, ParseTrt);

        }
    }
}
