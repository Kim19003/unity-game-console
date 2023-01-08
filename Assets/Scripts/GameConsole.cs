using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Color = UnityEngine.Color;
using TextEditor = UnityEngine.TextEditor;

public class GameConsole : MonoBehaviour
{
    [SerializeField] private Font font;
    [SerializeField] private KeyCode activateKey = KeyCode.Escape;
    [SerializeField] private int fontSize = 16;
    [SerializeField] private float height = 200;
    [SerializeField] private ConsoleTextColor defaultTextColor = ConsoleTextColor.White;
    [SerializeField] private ConsoleTextColor inputSuggestionTextColor = ConsoleTextColor.Grey;
    [SerializeField] private ConsoleTextColor outputExplanationTextColor = ConsoleTextColor.Grey;
    [SerializeField] private ConsoleTextColor outputWarningTextColor = ConsoleTextColor.Yellow;
    [SerializeField] private ConsoleTextColor outputErrorTextColor = ConsoleTextColor.Red;
    [SerializeField] private ConsoleTextColor outputBoxBackgroundColor = ConsoleTextColor.Black;
    [SerializeField] private float outputBoxBackgroundColorAlpha = 0.5f;
    [SerializeField] private ConsoleTextColor inputBoxBackgroundColor = ConsoleTextColor.Black;
    [SerializeField] private float inputBoxBackgroundColorAlpha = 0.6f;

    private readonly float canRemoveWithBackspaceAfterTime = 0.2f;
    private readonly float canScrollSuggestionsAfterTime = 0.2f;
    private readonly float canCompleteSuggestionAfterTime = 0.2f;
    private readonly float dontShowSuggestionsForTime = 0.2f;

    private readonly List<object> commands = new List<object>();
    private readonly List<GameConsoleOutput> outputs = new List<GameConsoleOutput>();
    private readonly HashSet<string> inputs = new HashSet<string>();

    GUIStyle outputBoxStyle, inputBoxStyle, outputLabelStyle, inputTextFieldStyle;

    GameConsoleCommand help;
    GameConsoleCommand<string> help_of;
    GameConsoleCommand clear;
    GameConsoleCommand<string> print;
    GameConsoleCommand quit;
    GameConsoleCommand<string> load_scene;
    GameConsoleCommand reload;
    GameConsoleCommand fullscreen;
    GameConsoleCommand<string> destroy;
    GameConsoleCommand<string, string> set_active;
    GameConsoleCommand<string, string> get_attribute_of;
    GameConsoleCommand<string, string, string> set_attribute_of;
    GameConsoleCommand get_admitted_attribute_names;
    GameConsoleCommand get_command_ids;

    private string input = string.Empty;
    
    private bool activated = false;
    private bool justActivated = false;
    private bool canDeactivate = false;

    readonly TimedUnityAction timedAction = new TimedUnityAction();
    Action currentTimedAction = null;
    float currentTimedActionInterval = 0;

    private void Awake()
    {
        #region Initializing
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

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
            font = font,
            fontSize = fontSize,
            richText = true,
            normal = new GUIStyleState()
            {
                textColor = Helpers.GetConsoleTextColor(defaultTextColor)
            }
        };

        inputTextFieldStyle = new GUIStyle()
        {
            font = font,
            fontSize = fontSize,
            richText = true,
            normal = new GUIStyleState()
            {
                textColor = Helpers.GetConsoleTextColor(defaultTextColor)
            }
        };

        DontDestroyOnLoad(gameObject);
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
                string commandFormat = consoleCommand.CommandFormat.WrapAlreadyWrappedPartsWithTags("i", '<', '>').Replace($"{consoleCommand.CommandId}",
                    $"<b>{consoleCommand.CommandId}</b>");
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
            $"{nameof(help_of)} <str: commandId>",
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
            $"{nameof(print)} <str: text>",
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
            $"{nameof(load_scene)} <str: sceneName>",
            $"{nameof(load_scene)} \"SampleScene\"");
        load_scene.Action = (sceneName) =>
        {
            int sceneBuildIndex = SceneUtility.GetBuildIndexByScenePath(sceneName);

            if (sceneBuildIndex > -1)
            {
                SceneManager.LoadScene(sceneName);
                Print($"Loaded scene {sceneName}", ConsoleOutputType.Explanation);
            }
            else
            {
                Print($"Scene {sceneName} not found", ConsoleOutputType.Error);
            }
        };

        reload = new GameConsoleCommand(
            $"{nameof(reload)}",
            "Reload the current scene",
            $"{nameof(reload)}",
            $"{nameof(reload)}");
        reload.Action = () =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            Print($"Reloaded the current scene", ConsoleOutputType.Explanation);
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
            $"{nameof(destroy)} <str: gameObjectName>",
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
            $"{nameof(set_active)} <str: gameObjectName> <bool: isTrue>",
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
                    Print($"Game object {gameObjectName} not found", ConsoleOutputType.Error);
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
            $"{nameof(get_attribute_of)} <str: gameObjectName> <str: gameObjectAttributeName>",
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
            $"{nameof(set_attribute_of)} <str: gameObjectName> <str: gameObjectAttributeName> <obj: gameObjectAttributeValue>",
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
            reload,
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

    void Start()
    {
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(activateKey))
        {
            if (!activated)
            {
                activated = true;
                justActivated = true;
                StartCoroutine(CanDeactivateAfter(0.2f));
            }
        }

        if (currentTimedAction != null)
        {
            if (currentTimedActionInterval > 0)
            {
                timedAction.Run(() => currentTimedAction(), currentTimedActionInterval);
            }
            else
            {
                currentTimedAction = null;
                currentTimedActionInterval = 0;
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
            case KeyCode keyCode when keyCode == activateKey:
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

    int previousInputsCount = 0;
    bool canRemoveWithBackspace = true;
    bool showSuggestions = true;
    int currentSuggestionIndex = 0;
    int currentInputHistoryIndex = 0;
    string previousInput = string.Empty;
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

        if (inputs.Count != previousInputsCount)
        {
            currentSuggestionIndex = 0;
            currentInputHistoryIndex = inputs.Count;
            previousInputsCount = inputs.Count;
        }

        bool dontAddWrappers = false;
        
        if (Event.current.isKey && Event.current.keyCode == KeyCode.Backspace)
        {
            if (canRemoveWithBackspace)
            {
                int caretIndex = textEditor.GetCaretIndex();

                if (caretIndex > 1)
                {
                    if (input.GetCharAt(caretIndex - 1) == '\"' && input.GetCharAt(caretIndex - 2) == '\"'
                        || input.GetCharAt(caretIndex - 1) == '\'' && input.GetCharAt(caretIndex - 2) == '\'')
                    {
                        input = input.Remove(caretIndex - 2, 2);
                        textEditor.SetCaretIndex(caretIndex - 2);
                        Event.current.keyCode = KeyCode.None;
                        StartCoroutine(CanRemoveWithBackspaceAfter(canRemoveWithBackspaceAfterTime));
                    }
                }
            }

            dontAddWrappers = true;
        }

        if (string.IsNullOrEmpty(input))
        {
            if (showInputHistorySuggestion)
            {
                inputSuggestion = inputs.GetClosestAt(ref currentInputHistoryIndex, true) ?? string.Empty;
            }

            if (Event.current.isKey)
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.Tab:
                        if (inputs.Count > 0)
                        {
                            // Apply input suggestion

                            if (!string.IsNullOrEmpty(inputSuggestion))
                            {
                                input = inputSuggestion;
                                textEditor.SetCaretIndex(input.Length);
                                currentInputHistoryIndex = inputs.Count;
                                inputSuggestion = string.Empty;
                                showInputHistorySuggestion = false;
                                dontAddWrappers = true;
                                StartCoroutine(DontShowSuggestionsFor(dontShowSuggestionsForTime));
                            }
                        }
                        break;
                    case KeyCode.UpArrow:
                        Event.current.keyCode = KeyCode.None;
                        if (inputs.Count > 0 && canScrollSuggestions)
                        {
                            // Suggest previous input in history

                            if (currentInputHistoryIndex == -1)
                            {
                                currentInputHistoryIndex = inputs.Count;
                            }
                            currentInputHistoryIndex--;
                            inputSuggestion = inputs.GetClosestAt(ref currentInputHistoryIndex, true) ?? string.Empty;
                            showInputHistorySuggestion = true;
                            StartCoroutine(CanScrollSuggestionsAfter(canScrollSuggestionsAfterTime));
                        }
                        break;
                    case KeyCode.DownArrow:
                        Event.current.keyCode = KeyCode.None;
                        if (inputs.Count > 0 && canScrollSuggestions)
                        {
                            // Suggest next input in history

                            if (currentInputHistoryIndex == inputs.Count)
                            {
                                currentInputHistoryIndex = -1;
                            }
                            currentInputHistoryIndex++;
                            inputSuggestion = inputs.GetClosestAt(ref currentInputHistoryIndex, true) ?? string.Empty;
                            showInputHistorySuggestion = true;
                            StartCoroutine(CanScrollSuggestionsAfter(canScrollSuggestionsAfterTime));
                        }
                        break;
                }
            }
        }
        else
        {
            currentInputHistoryIndex = inputs.Count;
            showInputHistorySuggestion = false;

            GetInputSuggestions(input, out HashSet<string> inputSuggestions);

            if (showSuggestions && !inputSuggestions.Any(s => s == inputReflection))
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

                                if (!string.IsNullOrEmpty(currentSuggestion))
                                {
                                    string fullSuggestion = currentSuggestion;
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
                                    dontAddWrappers = true;
                                }
                            }
                            break;
                        case KeyCode.UpArrow:
                            Event.current.keyCode = KeyCode.None;
                            if (inputSuggestions.Count > 0 && canScrollSuggestions)
                            {
                                // Show previous suggestion

                                currentSuggestionIndex--;
                                currentSuggestion = inputSuggestions.GetClosestAt(ref currentSuggestionIndex);
                                StartCoroutine(CanScrollSuggestionsAfter(canScrollSuggestionsAfterTime));
                            }
                            break;
                        case KeyCode.DownArrow:
                            Event.current.keyCode = KeyCode.None;
                            if (inputSuggestions.Count > 0 && canScrollSuggestions)
                            {
                                // Show next suggestion

                                currentSuggestionIndex++;
                                currentSuggestion = inputSuggestions.GetClosestAt(ref currentSuggestionIndex);
                                StartCoroutine(CanScrollSuggestionsAfter(canScrollSuggestionsAfterTime));
                            }
                            break;
                    }
                }

                inputSuggestion = !string.IsNullOrEmpty(currentSuggestion) ? currentSuggestion.RemovePartFromStart(input) : string.Empty;
            }
        }

        GUI.SetNextControlName("InputField");

        string _input = GUI.TextField(new Rect(10, inputPositionY + 5, Screen.width - 20, inputTextFieldHeight),
            $"{input}{(!string.IsNullOrEmpty(inputSuggestion) ? $"<color={inputSuggestionTextColor.ToString().ToLower()}>{inputSuggestion}</color>" : string.Empty)}",
            inputTextFieldStyle);
        input = !string.IsNullOrEmpty(inputSuggestion) ? _input.Replace($"<color={inputSuggestionTextColor.ToString().ToLower()}>{inputSuggestion}</color>", string.Empty) : _input;

        GUI.FocusControl("InputField");

        // On input change
        if (input != previousInput)
        {
            if (!dontAddWrappers)
            {
                int caretIndex = textEditor.GetCaretIndex();
                string selectedText = textEditor.SelectedText;

                if (caretIndex > 0)
                {
                    char foundChar = '\0';

                    switch (input.GetCharAt(caretIndex - 1))
                    {
                        case '"':
                            foundChar = '"';
                            break;
                        case '\'':
                            foundChar = '\'';
                            break;
                    }

                    if (foundChar != '\0')
                    {
                        if (selectedText.Length > 0)
                        {
                            input = input.WrapFirstFoundPart(selectedText, foundChar);
                        }
                        else
                        {
                            input = input.Remove(caretIndex - 1, 1).AddWrappersTo(caretIndex - 1, foundChar);
                        }
                    }
                }
            }

            currentSuggestionIndex = 0;

            previousInput = input;
        }
        
        // On activate
        if (justActivated)
        {


            justActivated = false;
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

        string[] inputParts;
        if (input.ContainsAmount('"') > 1)
        {
            inputParts = input.SplitAllNonWrapped(' ', '"');
        }
        else if (input.ContainsAmount('\'') > 1)
        {
            inputParts = input.SplitAllNonWrapped(' ', '\'');
        }
        else
        {
            inputParts = input.Split(' ');
        }

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

    private void PrintWrongUsageOfCommandError(string commandId)
    {
        Print($"Incorrect usage of the command \"{commandId}\" (use \"help_of '{commandId}'\" to get details about the command)", ConsoleOutputType.Error);
    }

    /// <summary>
    /// Used to set small delay to suggestion showing (since OnGui() is called many times per frame)
    /// </summary>
    private IEnumerator CanRemoveWithBackspaceAfter(float seconds)
    {
        canRemoveWithBackspace = false;

        yield return new WaitForSeconds(seconds);

        canRemoveWithBackspace = true;
    }

    /// <summary>
    /// Used to set small delay to suggestion showing (since OnGui() is called many times per frame)
    /// </summary>
    private IEnumerator DontShowSuggestionsFor(float seconds)
    {
        showSuggestions = false;

        yield return new WaitForSeconds(seconds);

        showSuggestions = true;
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
        canDeactivate = false;

        yield return new WaitForSeconds(seconds);

        canDeactivate = true;
    }

    /// <summary>
    /// Set the console activation key.
    /// </summary>
    public void SetActivateKey(KeyCode keyCode)
    {
        activateKey = keyCode;
    }

    /// <summary>
    /// Get the console activation key.
    /// </summary>
    public KeyCode GetActivateKey()
    {
        return activateKey;
    }

    /// <summary>
    /// Get the commands reflected.
    /// </summary>
    /// <returns>An array of the reflected commands.</returns>
    public GameConsoleCommandBase[] GetReflectedCommands()
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
    public GameConsoleOutput[] GetReflectedOutputs()
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
    public void Print(string text, ConsoleOutputType outputType = ConsoleOutputType.Information)
    {
        outputs.Add(new GameConsoleOutput(text, outputType));
    }

    /// <summary>
    /// Clear the console.
    /// </summary>
    public void Clear()
    {
        outputs.Clear();
    }
}
