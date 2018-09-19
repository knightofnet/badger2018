using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using WaveCompagnonPlayer.dto;

namespace WaveCompagnonPlayer.business.job
{
    internal class ShowSoundDevicesJob : IJobInterface
    {
        public void DoJob(AppArgsDto prgOptions)
        {
            string tpl = prgOptions.GetDeviceTpl.Replace("[SOUND_DEVICE]", "{0}");

            using (CoreAudioController coreAudioController = new CoreAudioController())
            {
                foreach (string devices in coreAudioController.GetPlaybackDevices().Where(r => r.State == DeviceState.Active).Select(r => r.Name))
                {

                    Console.WriteLine(tpl, devices);
                }
            }
        }
    }
}
