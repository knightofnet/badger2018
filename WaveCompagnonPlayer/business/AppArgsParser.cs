using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using AryxDevLibrary;
using AryxDevLibrary.utils.cliParser;
using BadgerCommonLibrary.constants;
using WaveCompagnonPlayer.dto;

namespace WaveCompagnonPlayer.business
{

    public class AppArgsParser : CliParser<AppArgsDto>
    {

        private static readonly Option _modeTrtOption = new Option()
        {
            ShortOpt = "m",
            LongOpt = "mode",
            Description = "Traitement à réaliser. Modes existant : " + EnumWaveCompModeTraitement.LibelleJoined(),
            HasArgs = true,
            IsMandatory = true,
            Name = "modeTrtOption"
        };

        private static readonly Option _soundToPlayOption = new Option()
        {
            ShortOpt = "s",
            LongOpt = "sound-to-play",
            Description = String.Format("Obligatoire avec le mode {0}. Son à jouer. Sons disponibles : {1}", EnumWaveCompModeTraitement.PlayEnumWaveCompSoundMode.LaunchModeOption, EnumSonWindows.LibelleJoined()),
            HasArgs = true,
            IsMandatory = false,
            Name = "soundToPlayOption"
        };

        private static readonly Option _soundDeviceOption = new Option()
        {
            ShortOpt = "d",
            LongOpt = "sound-device",
            Description = String.Format("Avec le mode {0}. Périphérique avec lequel jouer le son. Périphérique par défaut si option omise", EnumWaveCompModeTraitement.PlayEnumWaveCompSoundMode.LaunchModeOption),
            HasArgs = true,
            IsMandatory = false,
            Name = "soundDeviceOption"
        };

        private static readonly Option _soundVolumeOption = new Option()
        {
            ShortOpt = "v",
            LongOpt = "sound-volume",
            Description = String.Format("Avec le mode {0}. Volume du son (0 à 100)", EnumWaveCompModeTraitement.PlayEnumWaveCompSoundMode.LaunchModeOption),
            HasArgs = true,
            IsMandatory = false,
            Name = "soundVolumeOption"
        };

        private static readonly Option _showDevicesTplOption = new Option()
        {
            ShortOpt = "t",
            LongOpt = "output-tpl",
            Description = String.Format("Avec le mode {0}. Template pour la sortie", EnumWaveCompModeTraitement.ShowDevicesMode.LaunchModeOption),
            HasArgs = true,
            IsMandatory = false,
            Name = "showDevicesTplOption"
        };

        public AppArgsParser()
        {
            AddOption(_modeTrtOption);
            AddOption(_showDevicesTplOption);
            AddOption(_soundToPlayOption);
            AddOption(_soundDeviceOption);
            AddOption(_soundVolumeOption);




        }


        private AppArgsDto ParseTrt(Dictionary<string, Option> dictionary)
        {
            AppArgsDto appArgsDto = new AppArgsDto();

            if (HasOption(_modeTrtOption, dictionary))
            {
                string rawValue = GetSingleOptionValue(_modeTrtOption, dictionary);
                EnumWaveCompModeTraitement eRawValue = EnumWaveCompModeTraitement.GetFromLaunchModeOption(rawValue);
                if (eRawValue == null)
                {
                    throw new CliParsingException("Le mode de traitement n'a pas été reconnu. Modes disponibles : " + EnumWaveCompModeTraitement.LibelleJoined());
                }
                appArgsDto.WaveCompModeTraitements = eRawValue;
            }

            if (appArgsDto.WaveCompModeTraitements.Equals(EnumWaveCompModeTraitement.PlayEnumWaveCompSoundMode))
            {

                // _soundToPlayOption
                if (!HasOption(_soundToPlayOption, dictionary))
                {
                    throw new CliParsingException(String.Format("L'option -{0} est obligatoire avec le mode {1}", _soundToPlayOption.ShortOpt, EnumWaveCompModeTraitement.PlayEnumWaveCompSoundMode.LaunchModeOption));
                }

                string rawValue = GetSingleOptionValue(_soundToPlayOption, dictionary);
                EnumSonWindows eRawValue = EnumSonWindows.GetFromLibelle(rawValue);
                int intIndex;
                if (eRawValue == null && Int32.TryParse(rawValue, out intIndex))
                {
                    eRawValue = EnumSonWindows.GetFromIndex(intIndex);
                }

                if (eRawValue == null)
                {
                    throw new CliParsingException("Le son n'a pas été reconnnu. sons disponibles : " + EnumSonWindows.LibelleJoined());
                }
                appArgsDto.SoundToPlay = eRawValue;


                // _soundDeviceOption
                if (HasOption(_soundDeviceOption, dictionary))
                {
                    appArgsDto.SoundDevice = GetSingleOptionValue(_soundDeviceOption, dictionary);
                }

                // _soundVolumeOption
                if (HasOption(_soundVolumeOption, dictionary))
                {
                    string volRaw = GetSingleOptionValue(_soundVolumeOption, dictionary);
                    int intvol;
                    if (Int32.TryParse(volRaw, out intvol))
                    {
                        appArgsDto.SoundVolume = intvol;
                    }
                    else
                    {
                        throw new CliParsingException(
                            "Le volume du son n'a pas une valeur valide. Il doit être entre dans l'interval [0;100]");
                    }

                }
                else
                {
                    appArgsDto.SoundVolume = 75;
                }




            }
            else if (appArgsDto.WaveCompModeTraitements.Equals(EnumWaveCompModeTraitement.ShowDevicesMode))
            {
                // _showDevicesTplOption
                if (HasOption(_showDevicesTplOption, dictionary))
                {
                    appArgsDto.GetDeviceTpl = GetSingleOptionValue(_showDevicesTplOption, dictionary); ;
                }
                else
                {
                    appArgsDto.GetDeviceTpl = "[SOUND_DEVICE]";
                }
            }

            return appArgsDto;
        }



        public override AppArgsDto ParseDirect(string[] args)
        {

            return Parse(args, ParseTrt);

        }
    }
}

