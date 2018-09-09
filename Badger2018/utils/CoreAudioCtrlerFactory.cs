using AudioSwitcher.AudioApi.CoreAudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Badger2018.utils
{
    public class CoreAudioCtrlerFactory : IDisposable
    {

        private CoreAudioController _coreAudioController;
        private bool _isDisposed;

        public CoreAudioController CoreAudioCtrler
        {
            get
            {
                if (_coreAudioController == null || IsDisposed)
                {
                    _coreAudioController = new CoreAudioController();
                    IsDisposed = false;
                }
                return _coreAudioController;
            }
            private set
            {
                _coreAudioController = value;
            }
        }

        public bool IsDisposed {
            get
            {
                return _isDisposed;
            }
            set {
                _isDisposed = value;
                if (_coreAudioController != null && value)
                {
                    _coreAudioController.Dispose();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }

            } }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
