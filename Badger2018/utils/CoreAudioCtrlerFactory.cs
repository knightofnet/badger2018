using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using AryxDevLibrary.utils.logger;
using Badger2018.business;
using BadgerCommonLibrary.constants;
using BadgerCommonLibrary.utils;

namespace Badger2018.utils
{
    public class CoreAudioCtrlerFactory
    {



        private static readonly Logger _logger = Logger.LastLoggerInstance;


        private Process WaveCompProcess;

        public IList<string> ListOfSoundDevices { get; set; }


        private BackgroundWorker _bckgWorker = null;

        public CoreAudioCtrlerFactory()
        {
            ListOfSoundDevices = new List<string>();
        }



        public void AsyncPlaySound(EnumSonWindows sound, string deviceFullName, int volume, Action actionAfterBckger, Action<Exception> actionAfterBckgerFailAction)
        {


            Process[] pWaveCompProcs = Process.GetProcessesByName("WaveCompagnonPlayer");

                SoundWorkBckder bckder = new SoundWorkBckder();
                bckder.CoreAudioFactory = this;
                bckder.Sound = sound;
                bckder.Device = deviceFullName;
                bckder.Volume = volume;
                bckder.UseTcpRequest = pWaveCompProcs.Length == 1;

                _bckgWorker = new BackgroundWorker();
                _bckgWorker.DoWork += bckder.DoWorkPlaySound;

                _bckgWorker.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs args)
                {

                    if (args.Error == null)
                    {
                        _logger.Debug("AsyncPlaySound::Success");
                        if (actionAfterBckger != null)
                        {
                            actionAfterBckger.Invoke();
                        }
                    }
                    else
                    {
                        _logger.Debug("AsyncPlaySound::Error");
                        if (actionAfterBckgerFailAction != null)
                        {
                            actionAfterBckgerFailAction.Invoke(args.Error);
                        }
                    }


                };

                _bckgWorker.RunWorkerAsync();
            
        }


        public void AsyncLoadListOfSoundDevice(Action<IList<string>> actionAfterBckger, Action<Exception> actionAfterBckgerFailAction)
        {
            SoundWorkBckder bckder = new SoundWorkBckder();
            bckder.CoreAudioFactory = this;
            _bckgWorker = new BackgroundWorker();
            _bckgWorker.DoWork += bckder.DoWork;

            _bckgWorker.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs args)
            {

                if (args.Error == null)
                {
                    _logger.Debug("AsyncLoadListOfSoundDevice::Success");
                    if (actionAfterBckger != null)
                    {
                        actionAfterBckger.Invoke(bckder.ListDevices);
                    }
                    this.ListOfSoundDevices = bckder.ListDevices;

                }
                else
                {
                    _logger.Debug("AsyncLoadListOfSoundDevice::Error");
                    if (actionAfterBckgerFailAction != null)
                    {
                        actionAfterBckgerFailAction.Invoke(args.Error);
                    }
                }


            };

            _bckgWorker.RunWorkerAsync();
        }

        internal void InitProcess()
        {
            WaveCompProcess = new Process();
            WaveCompProcess.StartInfo.FileName = "WaveCompagnonPlayer.exe";
            WaveCompProcess.StartInfo.Arguments = String.Format("-m {0}",
                EnumWaveCompModeTraitement.DaemonWaitingOrders.LaunchModeOption);
            WaveCompProcess.StartInfo.UseShellExecute = false;
            WaveCompProcess.StartInfo.RedirectStandardOutput = true;
            WaveCompProcess.StartInfo.CreateNoWindow = true;
            WaveCompProcess.Start();
            WaveCompProcess.PriorityClass = ProcessPriorityClass.Normal;
        }

        internal void CloseProcess()
        {
            try
            {

                TcpRequestsStore.CloseWaveCompagnon();
            }
            catch (Exception ex)
            {
                if (WaveCompProcess != null && !WaveCompProcess.HasExited)
                {
                    WaveCompProcess.Close();
                    
                }

            } finally
            {
                if (WaveCompProcess != null)
                {
                    try
                    {
                        WaveCompProcess.Kill();
                    } catch (Exception ex)
                    {
                        ExceptionHandlingUtils.LogAndHideException(ex, "Erreur lors de la fermeture de WaveCompagnon");
                    }
                }
            }

        }
    }
}
