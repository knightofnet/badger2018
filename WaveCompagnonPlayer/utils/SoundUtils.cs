using AryxDevLibrary.utils.logger;
using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WaveCompagnonPlayer.dto;

namespace WaveCompagnonPlayer.utils
{
    public static  class SoundUtils
    {
        private static readonly Logger _logger = Logger.LastLoggerInstance;

        public static void PlaySound(AppArgsDto prgOptions, CoreAudioController coreAudioCtrler)
        {
            bool wasMuted = false;
            bool wasDftDeviceChanged = false;
            double originalVolume = 0;
            IDevice originalDftDevice = null;


            originalDftDevice =
                coreAudioCtrler.GetPlaybackDevices()
                    .FirstOrDefault(r => r.FullName.Equals(coreAudioCtrler.DefaultPlaybackDevice.FullName));
            _logger.Debug("OrigDftDevice: {0}", originalDftDevice.FullName);

            IDevice device =
                coreAudioCtrler.GetPlaybackDevices()
                    .FirstOrDefault(r => r.FullName.Equals(prgOptions.SoundDevice)) ??
                originalDftDevice;

            _logger.Debug("DeviceChoosed: {0}", device.FullName);

            if (!device.Equals(originalDftDevice))
            {
                coreAudioCtrler.SetDefaultDevice(device);
                wasDftDeviceChanged = true;
                _logger.Debug("2-OrigDftDevice: {0}", originalDftDevice.FullName);
            }

            if (device.IsMuted)
            {
                wasMuted = true;
                device.Mute(false);
            }

            originalVolume = device.Volume;
            device.Volume = prgOptions.SoundVolume;


            if (prgOptions.SoundToPlay.IsWaveFile)
            {
                using (var audioFile = new AudioFileReader(prgOptions.SoundToPlay.WaveFileInfo.FullName))
                using (var outputDevice = new WaveOutEvent())
                {
                    outputDevice.Init(audioFile);
                    outputDevice.Play();
                    while (outputDevice.PlaybackState == PlaybackState.Playing)
                    {
                        Thread.Sleep(250);
                    }
                }
            }
            else
            {
                _logger.Debug("Action Sound");
                Task tPlay = Task.Factory.StartNew(() =>
                {
                    prgOptions.SoundToPlay.Play();
                });
                tPlay.Wait(1200);
                _logger.Debug("FIN - Action Sound");
            }

            

            device.Volume = originalVolume;
            if (wasMuted)
            {
                device.Mute(true);
            }

            if (wasDftDeviceChanged)
            {

                coreAudioCtrler.SetDefaultDevice(originalDftDevice);
                _logger.Debug("3-OrigDftDevice: {0}", originalDftDevice.FullName);
                _logger.Debug("Real DftDevice: {0}", coreAudioCtrler.DefaultPlaybackDevice.FullName);

            }
        }

    }
}
