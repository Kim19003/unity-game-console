using System;

public class GameConsoleCommandBase
{
    public string CommandId { get { return commandId; } }
    private readonly string commandId;
    public string CommandDescription { get { return commandDescription; } }
    private readonly string commandDescription;
    public string CommandFormat { get { return commandFormat; } }
    private readonly string commandFormat;
    
    public GameConsoleCommandBase(string id, string description, string format)
    {
        commandId = id;
        commandDescription = description;
        commandFormat = format;
    }
}

public class GameConsoleCommand : GameConsoleCommandBase
{
    private readonly Action command;

    public GameConsoleCommand(string id, string description, string format, Action command) : base(id, description, format)
    {
        this.command = command;
    }

    public void Invoke()
    {
        command.Invoke();
    }
}

public class GameConsoleCommand<T1> : GameConsoleCommandBase
{
    private readonly Action<T1> command;

    public GameConsoleCommand(string id, string description, string format, Action<T1> command) : base(id, description, format)
    {
        this.command = command;
    }

    public void Invoke(T1 value)
    {
        command.Invoke(value);
    }
}