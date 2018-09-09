using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Badger2018.constants;

namespace Badger2018.dto
{
    public class AppSwitchs
    {
        private EnumStatePause _pauseCurrentState;

        // public bool IsEndNotifierPlageFixeAprem { get; set; }
        //
        // public bool IsEndNotifierPlageFixeMatin { get; set; }
        //
        // public bool IsNotif1Passed { get; set; }
        // public bool IsNotif2Passed { get; set; }
        // public bool IsEndNotifEndTheo { get; set; }
        // public bool IsEndNotifierPause { get; set; }
        public bool IsAutoBadgeage { get; set; }
        public bool IsBetaUser { get; set; }

        // public bool IsNotifMaxTempsTrav { get; set; }
        public bool IsRealClose { get; set; }


        public bool PbarMainTimerActif { get; set; }
        public bool IsMoreThanTpsTheo { get; internal set; }
        public bool IsTimerStoppedByMaxTime { get; internal set; }
        public bool IsTimeRemainingNotTimeWork { get; internal set; }
        public bool CanCheckUpdate { get; set; }
        public bool IsSoundOver { get; set; }
        public bool IsShowOnStartupDone { get; internal set; }

        public EnumStatePause PauseCurrentState
        {
            get { return _pauseCurrentState; }
            set
            {
                _pauseCurrentState = value;
                if (OnPauseCurrentStateChange != null)
                {
                    OnPauseCurrentStateChange(value);
                }
            }
        }

        public event Action<EnumStatePause> OnPauseCurrentStateChange;


        public AppSwitchs()
        {
            PauseCurrentState = EnumStatePause.NONE;

        }
    }
}
