using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Color = UnityEngine.Color;

public class GameConsole : MonoBehaviour
{
    public int fontSize = 16;
    public float height = 200;
    public ConsoleTextColor defaultTextColor = ConsoleTextColor.White;
    public ConsoleTextColor inputSuggestionTextColor = ConsoleTextColor.Grey;
    public ConsoleTextColor outputExplanationTextColor = ConsoleTextColor.Grey;
    public ConsoleTextColor outputWarningTextColor = ConsoleTextColor.Yellow;
    public ConsoleTextColor outputErrorTextColor = ConsoleTextColor.Red;
    public ConsoleTextColor outputBoxBackgroundColor = ConsoleTextColor.Black;
    public float outputBoxBackgroundColorAlpha = 0.5f;
    public ConsoleTextColor inputBoxBackgroundColor = ConsoleTextColor.Black;
    public float inputBoxBackgroundColorAlpha = 0.6f;

    public static KeyCode ActivateKey = KeyCode.Escape;

    private static readonly List<object> commands = new List<object>();
    private static readonly List<GameConsoleOutput> outputs = new List<GameConsoleOutput>();
    private static readonly List<string> inputs = new List<string>();

    GUIStyle outputBoxStyle, inputBoxStyle, outputLabelStyle, inputTextFieldStyle;

    private string input = string.Empty;
    private readonly float canScrollSuggestionsAfterTime = 0.2f;

    private bool activated;
    private bool canDeactivate;

    private void Awake()
    {
        Color _outputBoxBackgroundColor = Helpers.GetConsoleTextColor(outputBoxBackgroundColor);
        Color _inputBoxBackgroundColor = Helpers.GetConsoleTextColor(inputBoxBackgroundColor);

        outputBoxStyle = new GUIStyle()
        {
            normal = new GUIStyleState()
            {
                background = Helpers.MakeTexture(1, 1, new Color(_outputBoxBackgroundColor.r, _outputBoxBackgroundColor.g, _outputBoxBackgroundColor.b, outputBoxBackgroundColorAlpha))
            }
        };

        inputBoxStyle = new GUIStyle()
        {
            normal = new GUIStyleState()
            {
                background = Helpers.MakeTexture(1, 1, new Color(_inputBoxBackgroundColor.r, _inputBoxBackgroundColor.g, _inputBoxBackgroundColor.b, inputBoxBackgroundColorAlpha))
            }
        };

        outputLabelStyle = new GUIStyle()
        {
            fontSize = fontSize,
            richText = true,
            normal = new GUIStyleState()
            {
                textColor = Helpers.GetConsoleTextColor(defaultTextColor)
            }
        };

        inputTextFieldStyle = new GUIStyle()
        {
            fontSize = fontSize,
            richText = true,
            normal = new GUIStyleState()
            {
                textColor = Helpers.GetConsoleTextColor(defaultTextColor)
            }
        };

        // Add commands here
        commands.AddRange(new List<object>()
        {
            new GameConsoleCommand("help", "Show all available commands", "help", () =>
            {
                foreach (GameConsoleCommandBase consoleCommand in commands.Cast<GameConsoleCommandBase>())
                {
                    outputs.Add(new GameConsoleOutput($"{consoleCommand.CommandFormat} — {consoleCommand.CommandDescription}", ConsoleOutputType.Explanation));
                }
            }),
            new GameConsoleCommand("clear", "Clear the console", "clear", () =>
            {
                Clear();
            }),
            new GameConsoleCommand<string>("print", "Print text to the console", "print <string>", (value) =>
            {
                Print(value, ConsoleOutputType.Explanation);
            }),
            new GameConsoleCommand<int>("cool_method", "Method, that will be cool someday", "cool_method <int>", (value) =>
            {
                Print("Cool method: I will be a cool method someday!", ConsoleOutputType.Explanation);
            }),
            new GameConsoleCommand<int>("clear_something", "Method, that clears something... or does it?", "clear_something", (value) =>
            {
                Print("Clear something: Tried to clear something... but I can't do that, I'm just a test method!", ConsoleOutputType.Explanation);
            }),
        });
        // -----
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
        HandleInputField(height, ref input);

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

    readonly int outputLabelHeight = 20;
    readonly int outputLabelMarginBottom = 2;
    int previousOutputCount = -1;

    private void HandleOutputField(float outputPositionY, ref Rect viewRect, ref Vector2 scrollPosition)
    {
        viewRect = new Rect(0, 0, Screen.width - 30, (outputLabelHeight + outputLabelMarginBottom) * outputs.Count);

        GUI.Box(new Rect(0, outputPositionY, Screen.width, height), string.Empty, outputBoxStyle);

        if (previousOutputCount != outputs.Count)
        {
            scrollPosition.y = viewRect.height;
            previousOutputCount = outputs.Count;
        }
        
        scrollPosition = GUI.BeginScrollView(new Rect(0, outputPositionY + 5f, Screen.width, height - 10), scrollPosition, viewRect);

        foreach (GameConsoleOutput output in outputs)
        {
            string newOutputText = string.Empty;

            switch (output.OutputType)
            {
                case ConsoleOutputType.Explanation:
                    newOutputText = $"<color={outputExplanationTextColor.ToString().ToLower()}>{output.Text.RemoveTags("color")}</color>";
                    break;
                case ConsoleOutputType.Warning:
                    newOutputText = $"<color={outputWarningTextColor.ToString().ToLower()}>{output.Text.RemoveTags("color")}</color>";
                    break;
                case ConsoleOutputType.Error:
                    newOutputText = $"<color={outputErrorTextColor.ToString().ToLower()}>{output.Text.RemoveTags("color")}</color>";
                    break;
                case ConsoleOutputType.Custom:
                    newOutputText = $"{output.Text}";
                    break;
                default:
                    newOutputText = $"<color={defaultTextColor.ToString().ToLower()}>{output.Text.RemoveTags("color")}</color>";
                    break;
            }
            GUI.Label(new Rect(10, outputPositionY, Screen.width, (outputLabelHeight + outputLabelMarginBottom)), $"{newOutputText}", outputLabelStyle);

            outputPositionY += (outputLabelHeight + outputLabelMarginBottom);
        }

        GUI.EndScrollView();
    }

    int currentSuggestionIndex = 0;
    int currentInputHistoryIndex = inputs.Count + 1;
    bool canScrollSuggestions = true;
    bool showInputHistorySuggestion = false;
    readonly int inputTextFieldHeight = 20;

    private void HandleInputField(float inputPositionY, ref string input)
    {
        GUI.Box(new Rect(0, inputPositionY, Screen.width, inputTextFieldHeight + 10), string.Empty, inputBoxStyle);
        GUI.backgroundColor = new Color(0, 0, 0, 0);

        int textFieldControlId = GUIUtility.keyboardControl;
        string inputReflection = input;
        string inputSuggestion = string.Empty;

        TextEditor textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), textFieldControlId);

        if (textEditor.GetCaretIndex() > input.Length)
        {
            textEditor.SetCaretIndex(input.Length);
        }

        if (string.IsNullOrWhiteSpace(input))
        {
            currentSuggestionIndex = 0;

            if (showInputHistorySuggestion)
            {
                inputSuggestion = inputs.GetClosestAt(ref currentInputHistoryIndex);
            }

            if (Event.current.isKey)
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.Tab:
                        if (inputs.Count > 0)
                        {
                            // Apply input suggestion

                            input = inputs.GetClosestAt(ref currentInputHistoryIndex);
                            textEditor.SetCaretIndex(input.Length);
                            currentInputHistoryIndex = inputs.Count + 1;
                            inputSuggestion = string.Empty;
                            showInputHistorySuggestion = false;
                        }
                        break;
                    case KeyCode.UpArrow:
                        if (inputs.Count > 0 && canScrollSuggestions)
                        {
                            // Suggest previous input in history

                            currentInputHistoryIndex--;
                            inputSuggestion = inputs.GetClosestAt(ref currentInputHistoryIndex);
                            showInputHistorySuggestion = true;
                            StartCoroutine(CanScrollSuggestionsAfter(canScrollSuggestionsAfterTime));
                        }
                        Event.current.keyCode = KeyCode.None;
                        break;
                    case KeyCode.DownArrow:
                        if (inputs.Count > 0 && canScrollSuggestions)
                        {
                            // Suggest next input in history

                            currentInputHistoryIndex++;
                            inputSuggestion = inputs.GetClosestAt(ref currentInputHistoryIndex);
                            showInputHistorySuggestion = true;
                            StartCoroutine(CanScrollSuggestionsAfter(canScrollSuggestionsAfterTime));
                        }
                        Event.current.keyCode = KeyCode.None;
                        break;
                }
            }

            goto RenderTextField;
        }
        else
        {
            currentInputHistoryIndex = inputs.Count + 1;
            showInputHistorySuggestion = false;
        }
        
        GetInputSuggestions(input, out HashSet<string> inputSuggestions);

        if (!inputSuggestions.Any(s => s == inputReflection))
        {
            string currentSuggestion = inputSuggestions.GetClosestAt(ref currentSuggestionIndex);

            if (Event.current.isKey)
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.Tab:
                        if (inputSuggestions.Count > 0)
                        {
                            // Apply suggestion
                            
                            input = inputSuggestions.GetClosestAt(ref currentSuggestionIndex);
                            textEditor.SetCaretIndex(input.Length);
                            currentSuggestionIndex = 0;
                            currentSuggestion = string.Empty;
                        }
                        break;
                    case KeyCode.UpArrow:
                        if (inputSuggestions.Count > 0 && canScrollSuggestions)
                        {
                            // Show previous suggestion
                            
                            currentSuggestionIndex--;
                            currentSuggestion = inputSuggestions.GetClosestAt(ref currentSuggestionIndex);
                            StartCoroutine(CanScrollSuggestionsAfter(canScrollSuggestionsAfterTime));
                        }
                        Event.current.keyCode = KeyCode.None;
                        break;
                    case KeyCode.DownArrow:
                        if (inputSuggestions.Count > 0 && canScrollSuggestions)
                        {
                            // Show next suggestion

                            currentSuggestionIndex++;
                            currentSuggestion = inputSuggestions.GetClosestAt(ref currentSuggestionIndex);
                            StartCoroutine(CanScrollSuggestionsAfter(canScrollSuggestionsAfterTime));
                        }
                        Event.current.keyCode = KeyCode.None;
                        break;
                }
            }

            inputSuggestion = !string.IsNullOrEmpty(currentSuggestion) ? currentSuggestion.RemovePartFromStart(input) : string.Empty;
        }

        RenderTextField:
        {
            GUI.SetNextControlName("InputField");

            string _input = GUI.TextField(new Rect(10, inputPositionY + 5, Screen.width - 20, inputTextFieldHeight),
                $"{input}{(!string.IsNullOrEmpty(inputSuggestion) ? $"<color={inputSuggestionTextColor.ToString().ToLower()}>{inputSuggestion}</color>" : string.Empty)}",
                inputTextFieldStyle);
            input = !string.IsNullOrEmpty(inputSuggestion) ? _input.Replace($"<color={inputSuggestionTextColor.ToString().ToLower()}>{inputSuggestion}</color>", string.Empty) : _input;

            GUI.FocusControl("InputField");
        }
    }

    private void GetInputSuggestions(string input, out HashSet<string> inputSuggestions)
    {
        inputSuggestions = new HashSet<string>();
        
        try
        {
            List<string> _inputSuggestions = commands.Cast<GameConsoleCommandBase>().ToList()
                .Where(c => c.CommandFormat.ToLower().StartsWith(input)).Select(c => c.CommandFormat).ToList();
            inputSuggestions = new HashSet<string>(_inputSuggestions);
        }
        catch
        {
        }
    }

    /// <summary>
    /// Handle the input and execute the command.
    /// </summary>
    /// <returns>True, if the input didn't contain any errors.</returns>
    private bool HandleInput(string input)
    {
        inputs.Add(input);
        outputs.Add(new GameConsoleOutput($"> {input}", ConsoleOutputType.Information));

        string[] inputParts = input.SplitAllNonWrapped(' ', '"');

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
                            commandDefault.Invoke();
                            return true;
                        default:
                            outputs.Add(new GameConsoleOutput($"Wrong usage of the command \"{consoleCommandBase.CommandId}\", the right usage is: \"{consoleCommandBase.CommandFormat}\"", ConsoleOutputType.Error));
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
                                commandInt.Invoke(int.Parse(inputParts[1]));
                                return true;
                            }
                            catch (FormatException)
                            {
                                outputs.Add(new GameConsoleOutput($"Wrong usage of the command \"{consoleCommandBase.CommandId}\", the right usage is: \"{consoleCommandBase.CommandFormat}\"", ConsoleOutputType.Error));
                                return false;
                            }
                        default:
                            outputs.Add(new GameConsoleOutput($"Wrong usage of the command \"{consoleCommandBase.CommandId}\", the right usage is: \"{consoleCommandBase.CommandFormat}\"", ConsoleOutputType.Error));
                            return false;
                    }
                }
                else if (command is GameConsoleCommand<string> commandString)
                {
                    switch (inputParts.Length)
                    {
                        case 2:
                            try
                            {
                                commandString.Invoke(inputParts[1]);
                                return true;
                            }
                            catch (FormatException)
                            {
                                outputs.Add(new GameConsoleOutput($"Wrong usage of the command \"{consoleCommandBase.CommandId}\", the right usage is: \"{consoleCommandBase.CommandFormat}\"", ConsoleOutputType.Error));
                                return false;
                            }
                        default:
                            outputs.Add(new GameConsoleOutput($"Wrong usage of the command \"{consoleCommandBase.CommandId}\", the right usage is: \"{consoleCommandBase.CommandFormat}\"", ConsoleOutputType.Error));
                            return false;
                    }
                }

                break;
            }
        }

        if (previousOutputsCount == outputs.Count)
        {
            outputs.Add(new GameConsoleOutput($"Unknown command \"{string.Join(" ", inputParts)}\"", ConsoleOutputType.Error));
        }

        return false;
    }

    private IEnumerator CanScrollSuggestionsAfter(float seconds)
    {
        canScrollSuggestions = false;

        yield return new WaitForSeconds(seconds);

        canScrollSuggestions = true;
    }

    private IEnumerator CanDeactivateAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        canDeactivate = true;
    }

    /// <summary>
    /// Get the commands reflected.
    /// </summary>
    /// <returns>An array of the reflected commands.</returns>
    public static GameConsoleCommandBase[] GetReflectedCommands()
    {
        List<GameConsoleCommandBase> newCommands = new List<GameConsoleCommandBase>();

        foreach (GameConsoleCommandBase command in commands.Cast<GameConsoleCommandBase>())
        {
            newCommands.Add(new GameConsoleCommandBase(command.CommandId, command.CommandDescription, command.CommandFormat));
        }

        return newCommands.ToArray();
    }

    /// <summary>
    /// Get the outputs reflected.
    /// </summary>
    /// <returns>An array of the reflected outputs.</returns>
    public static GameConsoleOutput[] GetReflectedOutputs()
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
    public static void Print(string text, ConsoleOutputType outputType = ConsoleOutputType.Information)
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
