using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Color = UnityEngine.Color;
using TextEditor = UnityEngine.TextEditor;

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

    GameConsoleCommand help;
    GameConsoleCommand<string> help_of;
    GameConsoleCommand clear;
    GameConsoleCommand<string> print;
    GameConsoleCommand quit;
    GameConsoleCommand<string> load_scene;
    GameConsoleCommand restart;
    GameConsoleCommand fullscreen;
    GameConsoleCommand<string> destroy;
    GameConsoleCommand<string, string> set_active;
    GameConsoleCommand<string, string> get_attribute_of;
    GameConsoleCommand<string, string, string> set_attribute_of;
    GameConsoleCommand get_admitted_attribute_names;
    GameConsoleCommand get_command_ids;

    private string input = string.Empty;
    private readonly float canScrollSuggestionsAfterTime = 0.2f;
    private readonly float canCompleteSuggestionAfterTime = 0.2f;

    private bool activated;
    private bool canDeactivate;

    private void Awake()
    {
        #region Initializing
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

        inputs.Add(string.Empty);
        #endregion

        #region Commands
        help = new GameConsoleCommand(
            $"{nameof(help)}",
            "Show information about all available commands",
            $"{nameof(help)}",
            $"{nameof(help)}");
        help.Action = () =>
        {
            foreach (GameConsoleCommandBase consoleCommand in commands.Cast<GameConsoleCommandBase>())
            {
                string commandFormat = consoleCommand.CommandFormat.WrapAlreadyWrappedPartsWithTags("i", '<', '>');
                string commandExample = string.Empty; //consoleCommand.CommandExample
                //if (!string.IsNullOrEmpty(commandExample))
                //{
                //    commandExample = $"[example: {commandExample}]";
                //}
                Print($"{commandFormat} — {consoleCommand.CommandDescription}{(!string.IsNullOrEmpty(commandExample) ? " " + commandExample : string.Empty)}", ConsoleOutputType.Explanation);
            }
        };

        help_of = new GameConsoleCommand<string>(
            $"{nameof(help_of)}",
            "Show information about a command",
            $"{nameof(help_of)} <string: commandId>",
            $"{nameof(help_of)} \"{nameof(help)}\"");
        help_of.Action = (commandId) =>
        {
            GameConsoleCommandBase command = commands.Cast<GameConsoleCommandBase>().FirstOrDefault(c => c.CommandId == commandId);
            
            if (command != null)
            {
                Print($"Id: {command.CommandId}", ConsoleOutputType.Explanation);
                Print($"Description: {command.CommandDescription}", ConsoleOutputType.Explanation);
                Print($"Format: {command.CommandFormat.WrapAlreadyWrappedPartsWithTags("i", '<', '>')}", ConsoleOutputType.Explanation);
                Print($"Usage example: {command.CommandExample}", ConsoleOutputType.Explanation);
            }
            else
            {
                PrintWrongUsageOfCommandError(help_of.CommandId);
            }
        };

        clear = new GameConsoleCommand(
            $"{nameof(clear)}",
            "Clear the console",
            $"{nameof(clear)}",
            $"{nameof(clear)}");
        clear.Action = () =>
        {
            Clear();
        };

        print = new GameConsoleCommand<string>(
            $"{nameof(print)}",
            "Print text to the console",
            $"{nameof(print)} <string: text>",
            $"{nameof(print)} \"Hello world!\"");
        print.Action = (text) =>
        {
            Print(text, ConsoleOutputType.Explanation);
        };

        quit = new GameConsoleCommand(
            $"{nameof(quit)}",
            "Quit the game",
            $"{nameof(quit)}",
            $"{nameof(quit)}");
        quit.Action = () =>
        {
            Application.Quit();
        };

        load_scene = new GameConsoleCommand<string>(
            $"{nameof(load_scene)}",
            "Load specific scene",
            $"{nameof(load_scene)} <string: sceneName>",
            $"{nameof(load_scene)} \"SampleScene\"");
        load_scene.Action = (sceneName) =>
        {
            SceneManager.LoadScene(sceneName);
        };

        restart = new GameConsoleCommand(
            $"{nameof(restart)}",
            "Restart current scene",
            $"{nameof(restart)}",
            $"{nameof(restart)}");
        restart.Action = () =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        };

        fullscreen = new GameConsoleCommand(
            $"{nameof(fullscreen)}",
            "Switch to the fullscreen",
            $"{nameof(fullscreen)}",
            $"{nameof(fullscreen)}");
        fullscreen.Action = () =>
        {
            Print($"Fullscreen: {!Screen.fullScreen}", ConsoleOutputType.Explanation);
            Screen.fullScreen = !Screen.fullScreen;
        };

        destroy = new GameConsoleCommand<string>(
            $"{nameof(destroy)}",
            "Destroy specific game object",
            $"{nameof(destroy)} <string: gameObjectName>",
            $"{nameof(destroy)} \"Player\"");
        destroy.Action = (gameObjectName) =>
        {
            GameObject gameObject = GameObject.Find(gameObjectName);

            if (gameObject != null)
            {
                Destroy(gameObject);
            }
            else
            {
                PrintWrongUsageOfCommandError(destroy.CommandId);
            }
        };

        set_active = new GameConsoleCommand<string, string>(
            $"{nameof(set_active)}",
            "Activate or deactivate specific game object",
            $"{nameof(set_active)} <string: gameObjectName> <bool: isTrue>",
            $"{nameof(set_active)} \"Player\" false");
        set_active.Action = (gameObjectName, isTrue) =>
        {
            try
            {
                GameObject gameObject = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(go => go.name == gameObjectName);

                bool value = Convert.ToBool(isTrue);

                if (gameObject != null)
                {
                    gameObject.SetActive(value);
                }
                else
                {
                    throw new Exception();
                }
            }
            catch
            {
                PrintWrongUsageOfCommandError(set_active.CommandId);
            }
        };

        get_attribute_of = new GameConsoleCommand<string, string>(
            $"{nameof(get_attribute_of)}",
            "Find a game object by name and get it's attribute value",
            $"{nameof(get_attribute_of)} <string: gameObjectName> <string: gameObjectAttributeName>",
            $"{nameof(get_attribute_of)} \"Player\" \"position\"");
        get_attribute_of.Action = (gameObjectName, attributeName) =>
        {
            try
            {
                GameObject gameObject = GameObject.Find(gameObjectName);
                string attributeValue = gameObject.GetAttributeValue(attributeName);
                Print($"{gameObject.transform.name}'s {attributeName} is {attributeValue}", ConsoleOutputType.Explanation);
            }
            catch
            {
                PrintWrongUsageOfCommandError(get_attribute_of.CommandId);
            }
        };

        set_attribute_of = new GameConsoleCommand<string, string, string>(
            $"{nameof(set_attribute_of)}",
            "Find a game object by name and set it's attribute value",
            $"{nameof(set_attribute_of)} <string: gameObjectName> <string: gameObjectAttributeName> \"<object: gameObjectAttributeValue>\"",
            $"{nameof(set_attribute_of)} \"Player\" \"position\" \"(1.0, 1.0)\"");
        set_attribute_of.Action = (gameObjectName, attributeName, attributeValue) =>
        {
            try
            {
                GameObject gameObject = GameObject.Find(gameObjectName);
                gameObject.SetAttributeValue(attributeName, attributeValue);
                Print($"{gameObject.transform.name}'s {attributeName} is now {gameObject.GetAttributeValue(attributeName)}", ConsoleOutputType.Explanation);
            }
            catch
            {
                PrintWrongUsageOfCommandError(set_attribute_of.CommandId);
            }
        };

        get_admitted_attribute_names = new GameConsoleCommand(
            $"{nameof(get_admitted_attribute_names)}",
            "Get the game object's admitted attribute names",
            $"{nameof(get_admitted_attribute_names)}",
            $"{nameof(get_admitted_attribute_names)}");
        get_admitted_attribute_names.Action = () =>
        {
            foreach (string attributeName in GameObjectExtensions.AttributeNames)
            {
                Print($"{attributeName}", ConsoleOutputType.Explanation);
            }
        };

        get_command_ids = new GameConsoleCommand(
            $"{nameof(get_command_ids)}",
            "Get all command ids",
            $"{nameof(get_command_ids)}",
            $"{nameof(get_command_ids)}");
        get_command_ids.Action = () =>
        {
            foreach (GameConsoleCommandBase command in commands.Cast<GameConsoleCommandBase>().ToList())
            {
                Print($"{command.CommandId}", ConsoleOutputType.Explanation);
            }
        };

        commands.AddRange(new List<object>()
        {
            help,
            help_of,
            clear,
            print,
            quit,
            load_scene,
            restart,
            fullscreen,
            destroy,
            set_active,
            get_attribute_of,
            set_attribute_of,
            get_admitted_attribute_names,
            get_command_ids,
        });
        #endregion
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
    bool canCompleteSuggestion = true;
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

                            string fullSuggestion = inputSuggestions.GetClosestAt(ref currentSuggestionIndex);
                            string firstPartOfSuggestion = fullSuggestion.GetFirstWord();
                            if (canCompleteSuggestion && input.ToLower() == firstPartOfSuggestion.ToLower())
                            {
                                input = fullSuggestion;
                            }
                            else
                            {
                                input = firstPartOfSuggestion;
                                StartCoroutine(CanCompleteSuggestionAfter(canCompleteSuggestionAfterTime));
                            }
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
                .Where(c => c.CommandFormat.ToLower().StartsWith(input.ToLower())).Select(c => c.CommandFormat).ToList();
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
        Print($"> {input}", ConsoleOutputType.Information);

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
                            PrintWrongUsageOfCommandError(consoleCommandBase.CommandId);
                            return false;
                    }
                }
                else if (command is GameConsoleCommand<string> commandString)
                {
                    switch (inputParts.Length)
                    {
                        case 2:
                            commandString.Invoke(inputParts[1]);
                            return true;
                        default:
                            PrintWrongUsageOfCommandError(consoleCommandBase.CommandId);
                            return false;
                    }
                }
                else if (command is GameConsoleCommand<string, string> commandStringString)
                {
                    switch (inputParts.Length)
                    {
                        case 3:
                            commandStringString.Invoke(inputParts[1], inputParts[2]);
                            return true;
                        default:
                            PrintWrongUsageOfCommandError(consoleCommandBase.CommandId);
                            return false;
                    }
                }
                else if (command is GameConsoleCommand<string, string, string> commandStringStringString)
                {
                    switch (inputParts.Length)
                    {
                        case 4:
                            commandStringStringString.Invoke(inputParts[1], inputParts[2], inputParts[3]);
                            return true;
                        default:
                            PrintWrongUsageOfCommandError(consoleCommandBase.CommandId);
                            return false;
                    }
                }

                break;
            }
        }

        if (previousOutputsCount == outputs.Count)
        {
            Print($"Unknown command \"{string.Join(" ", inputParts)}\"", ConsoleOutputType.Error);
        }

        return false;
    }

    private static void PrintWrongUsageOfCommandError(string commandId)
    {
        Print($"Incorrect usage of the command \"{commandId}\". Use \"help_of {commandId}\" to get more information about the command.", ConsoleOutputType.Error);
    }

    /// <summary>
    /// Used to set small delay to suggestion scrolling (since OnGui() is called many times per frame)
    /// </summary>
    private IEnumerator CanScrollSuggestionsAfter(float seconds)
    {
        canScrollSuggestions = false;

        yield return new WaitForSeconds(seconds);

        canScrollSuggestions = true;
    }

    /// <summary>
    /// Used to set small delay to suggestion completion (since OnGui() is called many times per frame)
    /// </summary>
    private IEnumerator CanCompleteSuggestionAfter(float seconds)
    {
        canCompleteSuggestion = false;

        yield return new WaitForSeconds(seconds);

        canCompleteSuggestion = true;
    }

    /// <summary>
    /// Used to set small delay to allowing user close the opened console (since OnGui() is called many times per frame)
    /// </summary>
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
