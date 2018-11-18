using AryxDevLibrary.utils.logger;
using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using BadgerCommonLibrary.constants;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WaveCompagnonPlayer.dto;

namespace WaveCompagnonPlayer.business.job
{
    public class TestAllPlayerDeviceJob : IJobInterface
    {
        private static readonly Logger _logger = Logger.LastLoggerInstance;

        public void DoJob(AppArgsDto prgOptions)
        {

            try
            {
                
                if (prgOptions.SoundToPlay == null)
                {
                    prgOptions.SoundToPlay = EnumSonWindows.Chord;
                }

                using (CoreAudioController coreAudioCtrler = new CoreAudioController())
                {


                    bool wasMuted = false;
                    bool wasDftDeviceChanged = false;
                    double originalVolume = 0;
                    IDevice originalDftDevice = null;


                    originalDftDevice =
                        coreAudioCtrler.GetPlaybackDevices()
                            .FirstOrDefault(r => r.FullName.Equals(coreAudioCtrler.DefaultPlaybackDevice.FullName));
                    _logger.Debug("OrigDftDevice: {0}", originalDftDevice.FullName);

                    foreach(IDevice device in coreAudioCtrler.GetPlaybackDevices().Where(r => r.State == DeviceState.Active))
                    {

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
            catch (Exception ex)
            {

                _logger.Error(ex.Message);
                _logger.Error(ex.StackTrace);
            }
        }
    }
}
