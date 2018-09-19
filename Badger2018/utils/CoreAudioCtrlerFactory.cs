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
using BadgerCommonLibrary.utils;

namespace Badger2018.utils
{
    public class CoreAudioCtrlerFactory : IDisposable
    {



        private static readonly Logger _logger = Logger.LastLoggerInstance;
        private CoreAudioController _lastCoreAudioController;
        private CoreAudioController _coreAudioController;
        private bool _isDisposed;
        public IList<string> ListOfSoundDevices { get; set; }

        private DispatcherTimer finPauseMidiTimer = null;

        private BackgroundWorker _bckgWorker = null;

        public CoreAudioCtrlerFactory()
        {
            ListOfSoundDevices = new List<string>();
        }

        public CoreAudioController CoreAudioCtrler
        {
            get
            {
                _logger.Debug("CoreAudioCtrler.get()");

                if (_coreAudioController == null || IsDisposed)
                {
                    _logger.Debug("CoreAudioCtrler.get() : IsDisposed or null");

                    WaitLastIsDisposed(_lastCoreAudioController);

                    _coreAudioController = TaskGetNew(3);
                    // IObserver<DeviceChangedArgs> AudioDvcMgrOnChange = AudioDeviceManager_AudioDeviceChanged();
                    /*
                    TimeSpan tsFinPause = AppDateUtils.DtNow().TimeOfDay + new TimeSpan(0, 10, 0);
                    if (finPauseMidiTimer != null && finPauseMidiTimer.IsEnabled)
                    {
                        finPauseMidiTimer.Stop();
                    }
                    finPauseMidiTimer = new DispatcherTimer();
                    finPauseMidiTimer.Interval = new TimeSpan(0, 0, 10);
                    finPauseMidiTimer.Tick += (sender, args) =>
                    {
                        TimeSpan remainingTimer = tsFinPause - AppDateUtils.DtNow().TimeOfDay;
                        if (remainingTimer < TimeSpan.Zero)
                        {
                            finPauseMidiTimer.Stop();
                            _coreAudioController.Dispose();
                            WaitLastIsDisposed(_coreAudioController);
                        }
                    };
                    finPauseMidiTimer.Start();
                    */
                    IsDisposed = false;
                }
                _logger.Debug("CoreAudioCtrler.get() : return");
                return _coreAudioController;
            }
            private set
            {
                _coreAudioController = value;
            }
        }

        private CoreAudioController TaskGetNew(int nbTentative = 3, int timeout = 3000)
        {

            var task = Task.Factory.StartNew(() =>
            {
                return new CoreAudioController();
            });

            bool isCompletedSuccessfully = task.Wait(TimeSpan.FromMilliseconds(timeout));

            if (isCompletedSuccessfully)
            {
                return task.Result;
            }

            if (nbTentative >= 0)
            {
                _logger.Warn("Erreur... on retente {0}", nbTentative);

                return TaskGetNew(--nbTentative, (int)(timeout * 1.2));
            }


            return null;

        }

        private void WaitLastIsDisposed(CoreAudioController lastCoreAudioController)
        {
            try
            {
                int i = 10;
                while (lastCoreAudioController != null && i > 0 && !((AsyncBroadcaster<DeviceChangedArgs>)lastCoreAudioController.AudioDeviceChanged).IsDisposed)
                {
                    _logger.Debug("Wait last to be disposed...");
                    System.Threading.Thread.Sleep(150);
                    i--;
                }
                if (i == 0)
                {
                    _logger.Debug("Stop waiting because waited too long");
                }
            }
            catch (Exception ex)
            {

            }

        }

        public bool IsDisposed
        {
            get
            {
                return _isDisposed;
            }
            set
            {
                _logger.Debug("IsDisposed.get()");
                _isDisposed = value;
                /*
                if (_coreAudioController != null && value)
                {
                    _coreAudioController.Dispose();

                    GC.Collect();
                    GC.SuppressFinalize(_coreAudioController);
                    GC.WaitForPendingFinalizers();

                    _lastCoreAudioController = _coreAudioController;
                    _coreAudioController = null;
                    _logger.Debug("IsDisposed.get() : dispose");

                }
                 * */
                _logger.Debug("IsDisposed.get() : FIN");

            }
        }

        public void Dispose()
        {
            IsDisposed = true;

        }



        public void AsyncLoadListOfSoundDevice(Action<IList<string>> actionAfterBckger, Action actionAfterBckgerFailAction, CoreAudioCtrlerFactory coreAudioFactory)
        {
            GetListOfSoundDevicesBckder bckder = new GetListOfSoundDevicesBckder();
            bckder.CoreAudioFactory = coreAudioFactory;
            _bckgWorker = new BackgroundWorker();
            _bckgWorker.DoWork += bckder.DoWork;

            _bckgWorker.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs args)
            {

                if (args.Error == null)
                {
                    _logger.Debug("AsyncLoadListOfSoundDevice::Success");
                    actionAfterBckger.Invoke(bckder.ListDevices);
                    coreAudioFactory.ListOfSoundDevices = bckder.ListDevices;

                }
                else
                {
                    _logger.Debug("AsyncLoadListOfSoundDevice::Error");
                    actionAfterBckgerFailAction.Invoke();
                }


            };

            _bckgWorker.RunWorkerAsync();
        }



    }
}
