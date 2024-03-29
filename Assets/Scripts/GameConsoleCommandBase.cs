﻿using Assets.Scripts.Extensions;
using System;

public class GameConsoleCommandBase
{
    public string CommandId { get { return commandId; } }
    private readonly string commandId;
    public string CommandDescription { get { return commandDescription; } }
    private readonly string commandDescription;
    public string CommandFormat { get { return commandFormat; } }
    private readonly string commandFormat;
    public string[] CommandExamples { get { return commandExamples; } }
    private readonly string[] commandExamples;

    private Func<string> inputSuggestion;

    public GameConsoleCommandBase(string id, string description, string format, string[] examples = null)
    {
        commandId = id;
        commandDescription = description;
        commandFormat = format;
        commandExamples = examples;
        inputSuggestion = () =>
        {
            return !commandExamples.IsNullOrEmpty() ? commandExamples.GetRandomElement() : commandFormat;
        };
    }

    public void SetInputSuggestion(Func<string> inputSuggestion)
    {
        if (inputSuggestion != null)
        {
            this.inputSuggestion = inputSuggestion;
        }
    }

    public string GetInputSuggestion()
    {
        string suggestion = inputSuggestion();

        return $"{(!suggestion.Contains(CommandId) ? CommandId + " " : string.Empty)}{inputSuggestion()}";
    }
}

public class GameConsoleCommand : GameConsoleCommandBase
{
    public Action Action { get; set; }

    public GameConsoleCommand(string id, string description, string format, string[] examples = null, Action action = null) : base(id, description, format, examples)
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

    public GameConsoleCommand(string id, string description, string format, string[] examples = null, Action<T1> action = null) : base(id, description, format, examples)
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

    public GameConsoleCommand(string id, string description, string format, string[] examples = null, Action<T1, T2> action = null) : base(id, description, format, examples)
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

    public GameConsoleCommand(string id, string description, string format, string[] examples = null, Action<T1, T2, T3> action = null) : base(id, description, format, examples)
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