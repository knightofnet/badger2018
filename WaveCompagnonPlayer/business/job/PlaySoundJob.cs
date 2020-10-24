using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AryxDevLibrary.utils.logger;
using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using NAudio.Wave;
using WaveCompagnonPlayer.dto;
using WaveCompagnonPlayer.utils;

namespace WaveCompagnonPlayer.business.job
{
    class PlaySoundJob : IJobInterface
    {

        private static readonly Logger _logger = Logger.LastLoggerInstance;

        public void DoJob(AppArgsDto prgOptions)
        {

            try
            {

                using (CoreAudioController coreAudioCtrler = new CoreAudioController())
                {
                    SoundUtils.PlaySound(prgOptions, coreAudioCtrler);

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
