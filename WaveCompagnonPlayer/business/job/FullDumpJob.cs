using AryxDevLibrary.utils.logger;
using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WaveCompagnonPlayer.dto;

namespace WaveCompagnonPlayer.business.job
{
    public class FullDumpJob : IJobInterface
    {
        private static readonly Logger _logger = Logger.LastLoggerInstance;

        public void DoJob(AppArgsDto prgOptions)
        {
            try
            {
                using (CoreAudioController coreAudioController = new CoreAudioController())
                {
                    foreach (CoreAudioDevice device in coreAudioController.GetPlaybackDevices().Where(r => r.State == DeviceState.Active))
                    {

                        Console.WriteLine("Name : " + device.Name);
                        Console.WriteLine("  Fullname : " + device.FullName);
                        Console.WriteLine("  IsDftDev : " + device.IsDefaultDevice);
                    }
                }
            }
            catch (Exception ex)
            {

                _logger.Error(ex.Message);
                _logger.Error(ex.StackTrace);
            }
        }
    }
}
