using System;

namespace Assets.Scripts.Other
{
    public class TimedUnityAction
    {
        public Action Action { get { return action; } set { action = value; } }
        Action action = null;

        public float Interval { get { return interval; } set { if (value >= 0) interval = value; } }
        private float interval = 0f;

        public UnityTimeMode TimeMode { get { return timeMode; } set { timeMode = value; } }
        private UnityTimeMode timeMode = UnityTimeMode.Time;

        public float StartDelay { get { return startDelay; } set { if (value >= 0) startDelay = value; } }
        private float startDelay = 0f;

        public float DisableAfter { get { return disableAfter; } set { if (value >= 0) disableAfter = value; } }
        private float disableAfter = 0f;

        public bool Started { get { return started; } }
        private bool started = false;

        public bool Disabled { get { return disabled; } }
        private bool disabled = false;

        private float currentTimeElsewhereAtFirstRun = 0f;
        private bool startOver = true;
        private float nextActionCallTime = 0f;

        public TimedUnityAction(Action action = null, float interval = 0f, UnityTimeMode timeMode = UnityTimeMode.Time, float startDelay = 0f, float disableAfter = 0f)
        {
            Action = action;
            Interval = interval;
            TimeMode = timeMode;
            StartDelay = startDelay;
            DisableAfter = disableAfter;
        }

        /// <summary>
        /// Invokes {Action} every {Interval} second. Call this once in any of the Unity's Update methods.
        /// </summary>
        /// <returns><see langword="true"/> if the action was invoked.</returns>
        public bool Run()
        {
            if (disabled)
            {
                return false;
            }

            return HandleRunning(Action, Interval, TimeMode, StartDelay, DisableAfter);
        }

        /// <summary>
        /// Invokes {action} every {interval} second. Call this once in any of the Unity's Update methods.
        /// </summary>
        /// <returns><see langword="true"/> if the action was invoked.</returns>
        public bool Run(Action action, float interval, UnityTimeMode timeMode = UnityTimeMode.Time, float startDelay = 0f, float disableAfter = 0f)
        {
            if (disabled)
            {
                return false;
            }

            Action = action;
            Interval = interval;
            TimeMode = timeMode;
            StartDelay = startDelay;
            DisableAfter = disableAfter;

            return HandleRunning(Action, Interval, TimeMode, StartDelay, DisableAfter);
        }

        private bool HandleRunning(Action action, float interval, UnityTimeMode timeMode, float startDelay, float disableAfter)
        {
            if (action == null)
            {
                throw new ArgumentException("Action cannot be null.");
            }
            else if (interval < 0)
            {
                throw new ArgumentException("Interval must be greater than or equal to 0.");
            }
            else if (startDelay < 0)
            {
                throw new ArgumentException("Start delay time must be greater than or equal to 0.");
            }
            else if (disableAfter < 0)
            {
                throw new ArgumentException("Disable after time must be greater than or equal to 0.");
            }

            float currentTimeElsewhere = Helpers.GetTimeMode(timeMode);

            if (startOver)
            {
                currentTimeElsewhereAtFirstRun = currentTimeElsewhere;

                if (startDelay > 0)
                {
                    nextActionCallTime = startDelay;
                }

                startOver = false;
            }

            float currentTimeHere = currentTimeElsewhere - currentTimeElsewhereAtFirstRun;

            bool actionCalled = false;

            if (currentTimeHere >= nextActionCallTime)
            {
                started = true;

                action();
                nextActionCallTime += interval;
                actionCalled = true;
            }

            if (disableAfter > 0f && currentTimeHere >= disableAfter)
            {
                Disable();
            }

            if (actionCalled)
            {
                return true;
            }

            return false;
        }

        public void Enable()
        {
            if (disabled)
            {
                disabled = false;
            }
        }
        
        public void Disable()
        {
            if (!disabled)
            {
                started = false;
                currentTimeElsewhereAtFirstRun = 0f;
                startOver = true;
                nextActionCallTime = 0f;

                disabled = true;
            }
        }
    }
}
