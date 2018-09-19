using AryxDevLibrary.utils.logger;
using AudioSwitcher.AudioApi.CoreAudio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Badger2018.business;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.Observables;
using Badger2018.constants;
using BadgerCommonLibrary.constants;
using BadgerCommonLibrary.utils;

namespace Badger2018.utils
{
    public class CoreAudioCtrlerFactory
    {



        private static readonly Logger _logger = Logger.LastLoggerInstance;


        public IList<string> ListOfSoundDevices { get; set; }


        private BackgroundWorker _bckgWorker = null;

        public CoreAudioCtrlerFactory()
        {
            ListOfSoundDevices = new List<string>();
        }



        public void AsyncPlaySound(EnumSonWindows sound, string deviceFullName, int volume, Action actionAfterBckger, Action<Exception> actionAfterBckgerFailAction)
        {
            SoundWorkBckder bckder = new SoundWorkBckder();
            bckder.CoreAudioFactory = this;
            bckder.Sound = sound;
            bckder.Device = deviceFullName;
            bckder.Volume = volume;

            _bckgWorker = new BackgroundWorker();
            _bckgWorker.DoWork += bckder.DoWorkPlaySound;

            _bckgWorker.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs args)
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

            _bckgWorker.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs args)
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


    }
}
