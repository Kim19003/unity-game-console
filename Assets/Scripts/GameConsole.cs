using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Color = UnityEngine.Color;

public class GameConsole : MonoBehaviour
{
    public static KeyCode ActivateKey = KeyCode.Escape;
    
    private static readonly List<object> commands = new List<object>();
    private static readonly List<GameConsoleOutput> outputs = new List<GameConsoleOutput>();

    private bool activated;
    private string input;

    private bool canDeactivate;

    private void Awake()
    {
        commands.AddRange(new List<object>()
        {
            new GameConsoleCommand("help", "Show all available commands", "help", () =>
            {
                foreach (GameConsoleCommandBase consoleCommand in commands.Cast<GameConsoleCommandBase>())
                {
                    outputs.Add(new GameConsoleOutput($"{consoleCommand.CommandFormat} — {consoleCommand.CommandDescription}", ConsoleOutputType.Explanation));
                }
            }),
            new GameConsoleCommand<int>("test", "test", "test <int>", (value) =>
            {
                Debug.Log("I will be a cool method someday!");
            }),
            new GameConsoleCommand("clear", "Clear the console", "clear", () =>
            {
                Clear();
            }),
            new GameConsoleCommand("d2", "d2", "d2", () =>
            {
                Debug.Log("d2");
            }),
            new GameConsoleCommand("d3", "d3", "d3", () =>
            {
                Debug.Log("d3");
            }),
            new GameConsoleCommand("d4", "d4", "d4", () =>
            {
                Debug.Log("d4");
            })
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(ActivateKey))
        {
            if (!activated)
            {
                activated = true;
                StartCoroutine(CanDeactivateAfter(0.2f));
            }
        }
    }

    private Vector2 scrollPosition;
    private Rect viewRect;

    private void OnGUI()
    {
        if (!activated)
        {
            return;
        }

        HandleOutputField(0, ref viewRect, ref scrollPosition);
        HandleInputField(100, ref input);

        switch (Event.current.keyCode)
        {
            case KeyCode.Return:
                if (!string.IsNullOrWhiteSpace(input))
                {
                    HandleInput(input);
                    input = string.Empty;
                }
                break;
            case KeyCode keyCode when keyCode == ActivateKey:
                if (canDeactivate)
                {
                    input = string.Empty;
                    activated = false;
                    canDeactivate = false;
                }
                break;
        }
    }

    int previousOutputCount;

    private void HandleOutputField(float outputPositionY, ref Rect viewRect, ref Vector2 scrollPosition)
    {
        viewRect = new Rect(0, 0, Screen.width - 30, 20 * outputs.Count);

        GUI.Box(new Rect(0, outputPositionY, Screen.width, 100), string.Empty);

        if (previousOutputCount != outputs.Count)
        {
            scrollPosition.y = viewRect.height;
            previousOutputCount = outputs.Count;
        }
        
        scrollPosition = GUI.BeginScrollView(new Rect(0, outputPositionY + 5f, Screen.width, 90), scrollPosition, viewRect);

        foreach (GameConsoleOutput output in outputs)
        {
            switch (output.OutputType)
            {
                case ConsoleOutputType.Information:
                    GUI.color = Color.white;
                    break;
                case ConsoleOutputType.Explanation:
                    GUI.color = Helpers.GetCustomColor(CustomColor.LightGray);
                    break;
                case ConsoleOutputType.Warning:
                    GUI.color = Color.yellow;
                    break;
                case ConsoleOutputType.Error:
                    GUI.color = Color.red;
                    break;
            }
            GUI.Label(new Rect(0, outputPositionY, Screen.width, 20), $"{output.Text}");
            GUI.color = Color.white;

            outputPositionY += 20;
        }

        GUI.EndScrollView();
    }

    private void HandleInputField(float inputPositionY, ref string input)
    {
        GUI.Box(new Rect(0, inputPositionY, Screen.width, 30), string.Empty);
        GUI.backgroundColor = new Color(0, 0, 0, 0);
        GUI.SetNextControlName("InputField");
        input = GUI.TextField(new Rect(10, inputPositionY + 5, Screen.width - 20, 20), input);
        GUI.FocusControl("InputField");
    }

    /// <summary>
    /// Handle the input and execute the command.
    /// </summary>
    /// <returns>True, if the input didn't contain any errors.</returns>
    private bool HandleInput(string input)
    {
        string[] inputParts = input.Split(' ');

        int previousOutputsCount = outputs.Count;

        foreach (object command in commands)
        {
            GameConsoleCommandBase consoleCommandBase = (GameConsoleCommandBase)command;

            if (inputParts[0] == consoleCommandBase.CommandId)
            {
                if (command is GameConsoleCommand commandDefault)
                {
                    switch (inputParts.Length)
                    {
                        case 1:
                            outputs.Add(new GameConsoleOutput($"{string.Join(" ", inputParts)}", ConsoleOutputType.Information));
                            commandDefault.Invoke();
                            return true;
                        default:
                            outputs.Add(new GameConsoleOutput($"Wrong usage of the command '{consoleCommandBase.CommandId}', the right usage is: '{consoleCommandBase.CommandFormat}'", ConsoleOutputType.Error));
                            return false;
                    }
                }
                else if (command is GameConsoleCommand<int> commandInt)
                {
                    switch (inputParts.Length)
                    {
                        case 2:
                            try
                            {
                                outputs.Add(new GameConsoleOutput($"{string.Join(" ", inputParts)}", ConsoleOutputType.Information));
                                commandInt.Invoke(int.Parse(inputParts[1]));
                                return true;
                            }
                            catch (FormatException)
                            {
                                outputs.Add(new GameConsoleOutput($"Wrong usage of the command '{consoleCommandBase.CommandId}', the right usage is: '{consoleCommandBase.CommandFormat}'", ConsoleOutputType.Error));
                                return false;
                            }
                        default:
                            outputs.Add(new GameConsoleOutput($"Wrong usage of the command '{consoleCommandBase.CommandId}', the right usage is: '{consoleCommandBase.CommandFormat}'", ConsoleOutputType.Error));
                            return false;
                    }
                }

                break;
            }
        }

        if (previousOutputsCount == outputs.Count)
        {
            outputs.Add(new GameConsoleOutput($"Unknown command '{string.Join(" ", inputParts)}'", ConsoleOutputType.Error));
        }

        return false;
    }

    private IEnumerator CanDeactivateAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        canDeactivate = true;
    }

    /// <summary>
    /// Get the commands mirrored.
    /// </summary>
    /// <returns>An array of the mirrored commands.</returns>
    public static GameConsoleCommandBase[] GetMirroredCommands()
    {
        List<GameConsoleCommandBase> newCommands = new List<GameConsoleCommandBase>();

        foreach (GameConsoleCommandBase command in commands.Cast<GameConsoleCommandBase>())
        {
            newCommands.Add(new GameConsoleCommandBase(command.CommandId, command.CommandDescription, command.CommandFormat));
        }

        return newCommands.ToArray();
    }

    /// <summary>
    /// Get the outputs mirrored.
    /// </summary>
    /// <returns>An array of the mirrored outputs.</returns>
    public static GameConsoleOutput[] GetMirroredOutputs()
    {
        List<GameConsoleOutput> newOutputs = new List<GameConsoleOutput>();

        foreach (GameConsoleOutput output in outputs)
        {
            newOutputs.Add(new GameConsoleOutput(output.Text, output.OutputType));
        }

        return newOutputs.ToArray();
    }

    /// <summary>
    /// Print text to the console.
    /// </summary>
    public static void Put(string text, ConsoleOutputType outputType = ConsoleOutputType.Information)
    {
        outputs.Add(new GameConsoleOutput(text, outputType));
    }

    /// <summary>
    /// Clear the console.
    /// </summary>
    public static void Clear()
    {
        outputs.Clear();
    }
}
