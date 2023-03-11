using System;

namespace Assets.Scripts.Other
{
    public class TimedCommandCallerCommand
    {
        public GameConsoleCommandBase Command { get { return command; } }
        private readonly GameConsoleCommandBase command;

        public float Interval { get { return interval; } }
        private readonly float interval;

        public float StartDelay { get { return startDelay; } }
        private readonly float startDelay;

        public float DisableAfter { get { return disableAfter; } }
        private readonly float disableAfter;

        public object Action { get { return action; } }
        private readonly object action;

        public string[] Arguments { get { return arguments; } }
        private readonly string[] arguments;

        public TimedUnityAction TimedAction { get { return timedAction; } }
        private readonly TimedUnityAction timedAction;

        public DateTime CreationTime { get { return creationTime; } }
        private readonly DateTime creationTime;

        public TimedCommandCallerCommand(GameConsoleCommandBase command, float interval, float disableAfter, object action, string[] arguments = null, float startDelay = 0f)
        {
            this.command = command;
            this.interval = interval;
            this.startDelay = startDelay;
            this.disableAfter = disableAfter;
            this.action = action;
            this.arguments = arguments ?? Array.Empty<string>();

            timedAction = new TimedUnityAction(() =>
            {
                switch (this.arguments.Length)
                {
                    case 0:
                        ((Action)this.action).Invoke();
                        break;
                    case 1:
                        ((Action<string>)this.action).Invoke(this.arguments[0]);
                        break;
                    case 2:
                        ((Action<string, string>)this.action).Invoke(this.arguments[0], this.arguments[1]);
                        break;
                    case 3:
                        ((Action<string, string, string>)this.action).Invoke(this.arguments[0], this.arguments[1], this.arguments[2]);
                        break;
                }
            }, interval, startDelay: startDelay, disableAfter: disableAfter);

            creationTime = DateTime.Now;
        }
    }
}
