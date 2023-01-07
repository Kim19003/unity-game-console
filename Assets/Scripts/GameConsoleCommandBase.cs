﻿using System;

public class GameConsoleCommandBase
{
    public string CommandId { get { return commandId; } }
    private readonly string commandId;
    public string CommandDescription { get { return commandDescription; } }
    private readonly string commandDescription;
    public string CommandFormat { get { return commandFormat; } }
    private readonly string commandFormat;
    public string CommandExample { get { return commandExample; } }
    private readonly string commandExample;

    public GameConsoleCommandBase(string id, string description, string format, string example = "")
    {
        commandId = id;
        commandDescription = description;
        commandFormat = format;
        commandExample = example;
    }
}

public class GameConsoleCommand : GameConsoleCommandBase
{
    public Action Action { get; set; }

    public GameConsoleCommand(string id, string description, string format, string example = "", Action action = null) : base(id, description, format, example)
    {
        Action = action;
    }
    
    public void Invoke()
    {
        if (Action == null)
        {
            throw new NullReferenceException("Action is null.");
        }

        Action.Invoke();
    }
}

public class GameConsoleCommand<T1> : GameConsoleCommandBase
{
    public Action<T1> Action { get; set; }

    public GameConsoleCommand(string id, string description, string format, string example = "", Action<T1> action = null) : base(id, description, format, example)
    {
        Action = action;
    }

    public void Invoke(T1 value)
    {
        if (Action == null)
        {
            throw new NullReferenceException("Action is null.");
        }

        Action.Invoke(value);
    }
}

public class GameConsoleCommand<T1, T2> : GameConsoleCommandBase
{
    public Action<T1, T2> Action { get; set; }

    public GameConsoleCommand(string id, string description, string format, string example = "", Action<T1, T2> action = null) : base(id, description, format, example)
    {
        Action = action;
    }

    public void Invoke(T1 firstValue, T2 secondValue)
    {
        if (Action == null)
        {
            throw new NullReferenceException("Action is null.");
        }

        Action.Invoke(firstValue, secondValue);
    }
}

public class GameConsoleCommand<T1, T2, T3> : GameConsoleCommandBase
{
    public Action<T1, T2, T3> Action { get; set; }

    public GameConsoleCommand(string id, string description, string format, string example = "", Action<T1, T2, T3> action = null) : base(id, description, format, example)
    {
        Action = action;
    }

    public void Invoke(T1 firstValue, T2 secondValue, T3 thirdValue)
    {
        if (Action == null)
        {
            throw new NullReferenceException("Action is null.");
        }

        Action.Invoke(firstValue, secondValue, thirdValue);
    }
}