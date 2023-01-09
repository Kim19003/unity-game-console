using System;

namespace Assets.Scripts.Attributes
{
    /// <summary>
    /// Make method a game console command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class GameConsoleCommandAttribute : Attribute
    {
        public string CommandId { get { return commandId; } }
        private readonly string commandId;
        public string CommandDescription { get { return commandDescription; } }
        private readonly string commandDescription;
        public string CommandFormat { get { return commandFormat; } }
        private readonly string commandFormat;
        public string CommandExample { get { return commandExample; } }
        private readonly string commandExample;

        public GameConsoleCommandAttribute(string id, string description, string format, string example = "")
        {
            commandId = id;
            commandDescription = description;
            commandFormat = format;
            commandExample = example;
        }
    }
}
