using System;
using UnityEngine;

namespace Assets.Scripts.Other
{
    public class TimedUnityAction
    {
        public Action Action
        {
            get
            {
                return action;
            }
            set
            {
                if (value != null)
                {
                    action = value;
                }
            }
        }
        Action action = null;

        public float Interval
        {
            get
            {
                return interval;
            }
            private set
            {
                if (value > 0)
                {
                    interval = value;
                }
            }
        }
        private float interval = 0f;

        float nextActionTime = 0f;

        bool started = false;

        public TimedUnityAction()
        {

        }

        public TimedUnityAction(Action action, float interval)
        {
            if (action == null)
            {
                throw new ArgumentException("Action cannot be null.");
            }
            else if (interval <= 0)
            {
                throw new ArgumentException("Interval must be greater than 0.");
            }

            Action = action;
            Interval = interval;
        }

        /// <summary>
        /// Runs {Action} every {Interval} second. Call this in Unity's Update method.
        /// </summary>
        public void Run(float startDelay = 0f)
        {
            if (Action == null)
            {
                throw new ArgumentException("Action cannot be null.");
            }
            else if (Interval <= 0)
            {
                throw new ArgumentException("Interval must be greater than 0.");
            }
            else if (startDelay < 0)
            {
                throw new ArgumentException("Start delay must be greater than or equal to 0.");
            }

            if (Time.timeSinceLevelLoad > (startDelay > 0 && !started ? startDelay : nextActionTime))
            {
                Action();

                nextActionTime += Interval;

                started = true;
            }
        }

        /// <summary>
        /// Runs {action} every {interval} second. Call this in Unity's Update method.
        /// </summary>
        public void Run(Action action, float interval, float startDelay = 0f)
        {
            if (action == null)
            {
                throw new ArgumentException("Action cannot be null.");
            }
            else if (interval <= 0)
            {
                throw new ArgumentException("Interval must be greater than 0.");
            }
            else if (startDelay < 0)
            {
                throw new ArgumentException("Start delay must be greater than or equal to 0.");
            }

            if (Action == null)
            {
                Action = action;
            }
            else if (Action != action)
            {
                throw new ArgumentException("The action has already been initialized, and cannot be changed here. Use the SetAction method to change the action.");
            }

            if (Interval != interval)
            {
                Interval = interval;
            }

            if (Time.timeSinceLevelLoad > (startDelay > 0 && !started ? startDelay : nextActionTime))
            {
                Action();

                nextActionTime += Interval;

                started = true;
            }
        }

        public void SetAction(Action action)
        {
            Action = action;
        }

        public void SetInterval(float interval)
        {
            Interval = interval;
        }
    }
}
