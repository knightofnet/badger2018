using System;
using System.Windows.Threading;

namespace Badger2018.business
{
    class MainTimerManager
    {

        readonly DispatcherTimer _clockUpdTimer = new DispatcherTimer();
        public TimeSpan Interval { get; set; }

        public Boolean IsPaused { get; private set; }

        public Boolean IsStopped { get; private set; }
        public event Action<MainTimerManager> OnPauseToggled;

        public event EventHandler<EventArgs> OnTick;



        public MainTimerManager()
        {
            _clockUpdTimer = new DispatcherTimer();
            _clockUpdTimer.Tick += OnTickTick;

        }

        private void OnTickTick(object sender, EventArgs e)
        {
            if (!IsPaused && OnTick != null)
            {
                OnTick(sender, e);
            }
        }


        public void Start()
        {
            _clockUpdTimer.Interval = Interval;
            _clockUpdTimer.Start();

        }

        public void Pause()
        {

            IsPaused = true;
            _clockUpdTimer.IsEnabled = false;

            if (OnPauseToggled != null)
            {
                OnPauseToggled(this);
            }
        }

        public void Resume()
        {
            IsPaused = false;
            _clockUpdTimer.IsEnabled = true;

            if (OnPauseToggled != null)
            {
                OnPauseToggled(this);
            }
        }

        public void Stop()
        {
            _clockUpdTimer.Tick -= OnTickTick;
            _clockUpdTimer.Stop();
            IsStopped = true;
        }
    }
}
