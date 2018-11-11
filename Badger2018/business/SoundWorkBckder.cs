using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using AryxDevLibrary.extensions;
using AryxDevLibrary.utils.logger;
using Badger2018.dto;
using Badger2018.utils;
using BadgerCommonLibrary.constants;
using BadgerCommonLibrary.utils;

namespace Badger2018.business
{
    class SoundWorkBckder
    {
        private static readonly Logger _logger = Logger.LastLoggerInstance;
        private static string _delimiter = "###";
        public IList<string> ListDevices { get; set; }
        public CoreAudioCtrlerFactory CoreAudioFactory { get; set; }
        public AppOptions PrgOptions { get; set; }
        public EnumSonWindows Sound { get; set; }
        public string Device { get; set; }
        public int Volume { get; set; }

        public void DoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            BackgroundWorker bkg = sender as BackgroundWorker;
            ListDevices = new List<string>(1);

            Process compiler = new Process();
            try
            {
                compiler.StartInfo.FileName = "WaveCompagnonPlayer.exe";
                compiler.StartInfo.Arguments = String.Format("-m {0} -t \"{1}[SOUND_DEVICE]{1}\"", EnumWaveCompModeTraitement.ShowDevicesMode.LaunchModeOption, _delimiter);
                compiler.StartInfo.UseShellExecute = false;
                compiler.StartInfo.RedirectStandardOutput = true;
                compiler.StartInfo.CreateNoWindow = true;
                compiler.Start();
                compiler.PriorityClass = ProcessPriorityClass.High;

                string output = compiler.StandardOutput.ReadToEnd();
                bool hasMoreLineThanNormal = false;
                foreach (string line in output.SplitByStr("\r\n"))
                {
                    if (line.IsEmpty())
                    {
                        continue;
                    }


                    if (line.StartsWith(_delimiter) && line.EndsWith(_delimiter))
                    {
                        ListDevices.Add(line.Replace(_delimiter, ""));
                    }
                    else
                    {
                        hasMoreLineThanNormal = true;
                    }
                }

                if (hasMoreLineThanNormal)
                {
                    _logger.Error(output);
                }

                compiler.WaitForExit();

                if (compiler.HasExited && compiler.ExitCode > EnumExitCodes.OK.ExitCodeInt)
                {
                    throw new Exception("Une erreur est survenue lors de la lecture des périphériques sons");
                }

            }
            catch (Exception ex)
            {
                ExceptionHandlingUtils.LogAndRethrows(ex);
            }
            finally
            {
                if (!compiler.HasExited)
                {
                    compiler.WaitForExit();
                }
            }




        }

        public void DoWorkPlaySound(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bkg = sender as BackgroundWorker;
            ListDevices = new List<string>(1);

            if (Sound == null || Volume < 0 || Volume > 100 || Device == null)
            {
                _logger.Error("Impossible de jouer le son. Un paramétre est absent");
                return;
            }

            Process compiler = new Process();
            try
            {
                compiler.StartInfo.FileName = "WaveCompagnonPlayer.exe";
                compiler.StartInfo.Arguments = String.Format("-m {0} -s {1} -v {2} -d \"{3}\"",
                    EnumWaveCompModeTraitement.PlayEnumWaveCompSoundMode.LaunchModeOption,
                    Sound.Index,
                    Volume,
                    Device
                    );
                compiler.StartInfo.UseShellExecute = false;
                compiler.StartInfo.RedirectStandardOutput = true;
                compiler.StartInfo.CreateNoWindow = true;
                compiler.Start();
                compiler.PriorityClass = ProcessPriorityClass.High;

                compiler.WaitForExit();

                if (compiler.HasExited && compiler.ExitCode > EnumExitCodes.OK.ExitCodeInt)
                {
                    throw new Exception("Une erreur est survenue lors de la lecture du son");
                }


            }
            catch (Exception ex)
            {
                ExceptionHandlingUtils.LogAndRethrows(ex);
            }
            finally
            {
                if (!compiler.HasExited)
                {
                    compiler.WaitForExit();
                }
            }

        }
    }
}
