using Assets.Scripts.Attributes;
using Assets.Scripts.Extensions;
using Assets.Scripts.Other;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;
using Color = UnityEngine.Color;
using GameConsoleConvert = Assets.Scripts.Other.GameConsoleConvert;
using TextEditor = UnityEngine.TextEditor;

public class GameConsole : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private KeyCode activationKey = KeyCode.Escape;

    [Header("Text")]
    [SerializeField] private Font font;
    [SerializeField] private int fontSize = 16;
    [SerializeField] private Color defaultTextColor = Color.white;
    [SerializeField] private RichTextColor inputSuggestionTextColor = RichTextColor.Grey;
    [SerializeField] private RichTextColor outputExplanationTextColor = RichTextColor.Grey;
    [SerializeField] private RichTextColor outputWarningTextColor = RichTextColor.Yellow;
    [SerializeField] private RichTextColor outputErrorTextColor = RichTextColor.Red;

    [Header("Output")]
    [SerializeField] private float outputBoxHeight = 200;
    [SerializeField] private int outputLabelHeight = 20;
    [SerializeField] private Texture2D outputBoxBackgroundTexture;
    [SerializeField] private Color outputBoxBackgroundColor = Color.black;
    [SerializeField] private float outputBoxBackgroundColorAlpha = 1f;
    [SerializeField] private Texture2D outputBoxBorderBackgroundTexture;
    [SerializeField] private float outputBoxBorderSize = 2f;
    [SerializeField] private Color outputBoxBorderBackgroundColor = Color.gray;
    [SerializeField] private float outputBoxBorderBackgroundColorAlpha = 1f;
    [SerializeField] private bool showTimestamps = true;
    [SerializeField] private RichTextColor timestampsColor = RichTextColor.Grey;
    [SerializeField] private bool timestampsUseParentColor = true;

    [Header("Input")]
    [SerializeField] private int inputTextFieldHeight = 20;
    [SerializeField] private Texture2D inputBoxBackgroundTexture;
    [SerializeField] private Color inputBoxBackgroundColor = Color.black;
    [SerializeField] private float inputBoxBackgroundColorAlpha = 1f;
    [SerializeField] private Texture2D inputBoxBorderBackgroundTexture;
    [SerializeField] private float inputBoxBorderSize = 2f;
    [SerializeField] private Color inputBoxBorderBackgroundColor = Color.gray;
    [SerializeField] private float inputBoxBorderBackgroundColorAlpha = 1f;

    #region Boring variables
    private readonly float canDeactivateConsoleAfterTime = 0.7f;
    private readonly float canRemoveWrappersWithBackspaceAfterTime = 0.3f;
    private readonly float canScrollSuggestionsAfterTime = 0.2f;
    private readonly float canCompleteSuggestionAfterTime = 0.2f;
    private readonly float dontShowSuggestionsForTime = 0.2f;

    private readonly List<object> commands = new List<object>();
    private readonly List<GameConsoleOutput> outputs = new List<GameConsoleOutput>();
    private readonly HashSet<string> inputs = new HashSet<string>();

    GUIStyle outputBoxStyle, inputBoxStyle, outputBoxBorderStyle, inputBoxBorderStyle, outputLabelStyle, inputTextFieldStyle;

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
    GameConsoleCommand<string> set_timescale;
    GameConsoleCommand<string, string, string> set_as_timed;
    GameConsoleCommand<string> stop_timed;

    GameConsoleCommand<string> set_test_object_position;

    private string input = string.Empty;
    
    private bool activated = false;
    private bool justActivated = false;
    private bool canDeactivateConsole = false;

    private readonly HashSet<TimedCommandCallerCommand> timedCommandCallerCommands = new HashSet<TimedCommandCallerCommand>();
    #endregion

    private void Awake()
    {
        #region Initializing
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        if (outputBoxBackgroundTexture == null)
        {
            outputBoxBackgroundTexture = Helpers.MakeTexture(1, 1, new Color(outputBoxBackgroundColor.r, outputBoxBackgroundColor.g, outputBoxBackgroundColor.b, outputBoxBackgroundColorAlpha));
        }
        else
        {
            outputBoxBackgroundTexture.FillWithColor(new Color(outputBoxBackgroundColor.r, outputBoxBackgroundColor.g, outputBoxBackgroundColor.b, outputBoxBackgroundColorAlpha));
        }

        if (inputBoxBackgroundTexture == null)
        {
            inputBoxBackgroundTexture = Helpers.MakeTexture(1, 1, new Color(inputBoxBackgroundColor.r, inputBoxBackgroundColor.g, inputBoxBackgroundColor.b, inputBoxBackgroundColorAlpha));
        }
        else
        {
            inputBoxBackgroundTexture.FillWithColor(new Color(inputBoxBackgroundColor.r, inputBoxBackgroundColor.g, inputBoxBackgroundColor.b, inputBoxBackgroundColorAlpha));
        }

        if (outputBoxBorderBackgroundTexture == null)
        {
            outputBoxBorderBackgroundTexture = Helpers.MakeTexture(1, 1, new Color(outputBoxBorderBackgroundColor.r, outputBoxBorderBackgroundColor.g, outputBoxBorderBackgroundColor.b, outputBoxBorderBackgroundColorAlpha));
        }
        else
        {
            outputBoxBorderBackgroundTexture.FillWithColor(new Color(outputBoxBorderBackgroundColor.r, outputBoxBorderBackgroundColor.g, outputBoxBorderBackgroundColor.b, outputBoxBorderBackgroundColorAlpha));
        }

        if (inputBoxBorderBackgroundTexture == null)
        {
            inputBoxBorderBackgroundTexture = Helpers.MakeTexture(1, 1, new Color(inputBoxBorderBackgroundColor.r, inputBoxBorderBackgroundColor.g, inputBoxBorderBackgroundColor.b, inputBoxBorderBackgroundColorAlpha));
        }
        else
        {
            inputBoxBorderBackgroundTexture.FillWithColor(new Color(inputBoxBorderBackgroundColor.r, inputBoxBorderBackgroundColor.g, inputBoxBorderBackgroundColor.b, inputBoxBorderBackgroundColorAlpha));
        }

        outputBoxStyle = new GUIStyle()
        {
            normal = new GUIStyleState()
            {
                background = outputBoxBackgroundTexture
            }
        };

        inputBoxStyle = new GUIStyle()
        {
            normal = new GUIStyleState()
            {
                background = inputBoxBackgroundTexture
            }
        };

        outputBoxBorderStyle = new GUIStyle()
        {
            normal = new GUIStyleState()
            {
                background = outputBoxBorderBackgroundTexture
            }
        };

        inputBoxBorderStyle = new GUIStyle()
        {
            normal = new GUIStyleState()
            {
                background = inputBoxBorderBackgroundTexture
            }
        };

        outputLabelStyle = new GUIStyle()
        {
            font = font,
            fontSize = fontSize,
            richText = true,
            normal = new GUIStyleState()
            {
                textColor = defaultTextColor
            }
        };

        inputTextFieldStyle = new GUIStyle()
        {
            font = font,
            fontSize = fontSize,
            richText = true,
            normal = new GUIStyleState()
            {
                textColor = defaultTextColor
            }
        };

        DontDestroyOnLoad(gameObject);
        #endregion

        #region Commands
        help = new GameConsoleCommand(
            id: $"{nameof(help)}",
            description: "Show information about all available commands",
            format: $"{nameof(help)}",
            examples: new string[] { $"{nameof(help)}" });
        help.Action = () =>
        {
            foreach (GameConsoleCommandBase consoleCommand in commands.Cast<GameConsoleCommandBase>())
            {
                string commandFormat = consoleCommand.CommandFormat.WrapAlreadyWrappedPartsWithTags("i", '<', '>').Replace($"{consoleCommand.CommandId}",
                    $"<b>{consoleCommand.CommandId}</b>");
                Print($"{commandFormat} — {consoleCommand.CommandDescription}", ConsoleOutputType.Explanation);
            }
        };

        help_of = new GameConsoleCommand<string>(
            id: $"{nameof(help_of)}",
            description: "Show information about a command",
            format: $"{nameof(help_of)} <str: commandId>",
            examples: new string[] { $"{nameof(help_of)} \"{nameof(help)}\"" });
        help_of.Action = (commandId) =>
        {
            GameConsoleCommandBase command = commands.Cast<GameConsoleCommandBase>().FirstOrDefault(c => c.CommandId == commandId);
            
            if (command != null)
            {
                Print($"Id: {command.CommandId.AsBold()}", ConsoleOutputType.Explanation);
                Print($"Description: {command.CommandDescription}", ConsoleOutputType.Explanation);
                Print($"Format: {command.CommandFormat.WrapAlreadyWrappedPartsWithTags("i", '<', '>')}", ConsoleOutputType.Explanation);
                Print($"Usage example: {(!command.CommandExamples.IsNullOrEmpty() ? command.CommandExamples.GetRandomElement() : string.Empty)}", ConsoleOutputType.Explanation);
            }
            else
            {
                PrintNotFoundError("Command", commandId);
            }
        };
        help_of.GetInputSuggestion = () =>
        {
            return $"{help_of.CommandId} \"{commands.Cast<GameConsoleCommandBase>().Select(c => c.CommandId).GetRandomElement()}\"";
        };

        clear = new GameConsoleCommand(
            id: $"{nameof(clear)}",
            description: "Clear the console",
            format: $"{nameof(clear)}",
            examples: new string[] { $"{nameof(clear)}" });
        clear.Action = () =>
        {
            Clear();
        };

        print = new GameConsoleCommand<string>(
            id: $"{nameof(print)}",
            description: "Print text to the console",
            format: $"{nameof(print)} <str: text>",
            examples: new string[] { $"{nameof(print)} \"Hello world!\"" });
        print.Action = (text) =>
        {
            Print(text, ConsoleOutputType.Explanation);
        };

        quit = new GameConsoleCommand(
            id: $"{nameof(quit)}",
            description: "Quit the game",
            format: $"{nameof(quit)}",
            examples: new string[] { $"{nameof(quit)}" });
        quit.Action = () =>
        {
            Application.Quit();
        };

        load_scene = new GameConsoleCommand<string>(
            id: $"{nameof(load_scene)}",
            description: "Load specific scene",
            format: $"{nameof(load_scene)} <str: sceneName>",
            examples: new string[] { $"{nameof(load_scene)} \"SampleScene\"" });
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
                PrintNotFoundError("Scene", sceneName);
            }
        };

        reload = new GameConsoleCommand(
            id: $"{nameof(reload)}",
            description: "Reload the current scene",
            format: $"{nameof(reload)}",
            examples: new string[] { $"{nameof(reload)}" });
        reload.Action = () =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            Print($"Reloaded the current scene", ConsoleOutputType.Explanation);
        };

        fullscreen = new GameConsoleCommand(
            id: $"{nameof(fullscreen)}",
            description: "Switch to the fullscreen",
            format: $"{nameof(fullscreen)}",
            examples: new string[] { $"{nameof(fullscreen)}" });
        fullscreen.Action = () =>
        {
            Screen.fullScreen = !Screen.fullScreen;
            Print($"Fullscreen: {Screen.fullScreen}", ConsoleOutputType.Explanation);
        };

        destroy = new GameConsoleCommand<string>(
            id: $"{nameof(destroy)}",
            description: "Destroy specific game object",
            format: $"{nameof(destroy)} <str: gameObjectName>",
            examples: new string[] { $"{nameof(destroy)} \"Player\"" });
        destroy.Action = (gameObjectName) =>
        {
            GameObject gameObject = GameObject.Find(gameObjectName);

            if (gameObject != null)
            {
                Destroy(gameObject);
            }
            else
            {
                PrintNotFoundError("Game object", gameObjectName);
            }
        };

        set_active = new GameConsoleCommand<string, string>(
            id: $"{nameof(set_active)}",
            description: "Activate or deactivate specific game object",
            format: $"{nameof(set_active)} <str: gameObjectName> <bool: isTrue>",
            examples: new string[] { $"{nameof(set_active)} \"Player\" false" });
        set_active.Action = (gameObjectName, isTrue) =>
        {
            GameObject gameObject = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(go => go.name == gameObjectName);

            if (gameObject != null)
            {
                GameConsoleType<bool> gameConsoleType = GameConsoleConvert.ToBool(isTrue);
                if (gameConsoleType != null)
                {
                    gameObject.SetActive(gameConsoleType.Value);
                    Print($"{gameObject.transform.name} is now {(gameConsoleType.Value ? "activated" : "deactivated")}", ConsoleOutputType.Explanation);
                }
                else
                {
                    PrintIncorrectTypeError("Argument", nameof(isTrue));
                }
            }
            else
            {
                PrintNotFoundError("Game object", gameObjectName);
            }
        };

        get_attribute_of = new GameConsoleCommand<string, string>(
            id: $"{nameof(get_attribute_of)}",
            description: "Find a game object by name and get it's attribute value",
            format: $"{nameof(get_attribute_of)} <str: gameObjectName> <str: attributeName>",
            examples: new string[] { $"{nameof(get_attribute_of)} \"Player\" \"position\"" });
        get_attribute_of.Action = (gameObjectName, attributeName) =>
        {
            GameObject gameObject = GameObject.Find(gameObjectName);

            if (gameObject != null)
            {
                (GetterResult Result, string Value) = gameObject.GetAttributeValue(attributeName);
                if (Result == GetterResult.Successful)
                {
                    Print($"{gameObject.transform.name}'s {attributeName} is {Value}", ConsoleOutputType.Explanation);
                }
                else
                {
                    PrintNotFoundError("Attribute", attributeName);
                }
            }
            else
            {
                PrintNotFoundError("Game object", gameObjectName);
            }
        };

        set_attribute_of = new GameConsoleCommand<string, string, string>(
            id: $"{nameof(set_attribute_of)}",
            description: "Find a game object by name and set it's attribute value",
            format: $"{nameof(set_attribute_of)} <str: gameObjectName> <str: attributeName> <obj: attributeValue>",
            examples: new string[] { $"{nameof(set_attribute_of)} \"Player\" \"position\" \"(1, 1, 0)\"" });
        set_attribute_of.Action = (gameObjectName, attributeName, attributeValue) =>
        {
            GameObject gameObject = GameObject.Find(gameObjectName);

            if (gameObject != null)
            {
                SetterResult setterResult = gameObject.SetAttributeValue(attributeName, attributeValue);
                switch (setterResult)
                {
                    case SetterResult.Successful:
                        Print($"{gameObject.transform.name}'s {attributeName} is now {gameObject.GetAttributeValue(attributeName).Value}", ConsoleOutputType.Explanation);
                        break;
                    case SetterResult.ValueNotAllowed:
                        PrintIncorrectTypeError("Argument", nameof(attributeValue));
                        break;
                    case SetterResult.TargetNotFound:
                        PrintNotFoundError("Attribute", attributeName);
                        break;
                }
            }
            else
            {
                PrintNotFoundError("Game object", gameObjectName);
            }
        };

        get_admitted_attribute_names = new GameConsoleCommand(
            id: $"{nameof(get_admitted_attribute_names)}",
            description: "Get the GameObject type's admitted attribute names",
            format: $"{nameof(get_admitted_attribute_names)}",
            examples: new string[] { $"{nameof(get_admitted_attribute_names)}" });
        get_admitted_attribute_names.Action = () =>
        {
            foreach (string attributeName in GameObjectExtensions.AttributeNames)
            {
                Print($"{attributeName}", ConsoleOutputType.Explanation);
            }
        };

        get_command_ids = new GameConsoleCommand(
            id: $"{nameof(get_command_ids)}",
            description: "Get all command ids",
            format: $"{nameof(get_command_ids)}",
            examples: new string[] { $"{nameof(get_command_ids)}" });
        get_command_ids.Action = () =>
        {
            foreach (GameConsoleCommandBase command in commands.Cast<GameConsoleCommandBase>().ToList())
            {
                Print($"{command.CommandId}", ConsoleOutputType.Explanation);
            }
        };

        set_timescale = new GameConsoleCommand<string>(
            id: $"{nameof(set_timescale)}",
            description: "Set the scale at which time passes",
            format: $"{nameof(set_timescale)} <flt: timeScale>",
            examples: new string[] { $"{nameof(set_timescale)} \"1\"" });
        set_timescale.Action = (timeScale) =>
        {
            GameConsoleType<float> timeScaleType = GameConsoleConvert.ToFloat(timeScale);

            if (timeScaleType != null)
            {
                Time.timeScale = timeScaleType.Value;
                Print($"Time scale is now {Time.timeScale}", ConsoleOutputType.Explanation);
            }
            else
            {
                PrintIncorrectTypeError("Argument", nameof(timeScale));
            }
        };

        //  GameConsoleCommand<string, string, string> set_as_timed;

        set_as_timed = new GameConsoleCommand<string, string, string>(
            id: $"{nameof(set_as_timed)}",
            description: "Set a command as a timed command",
            format: $"{nameof(set_as_timed)} <flt: callTime>, <flt: stopTime>, <cmd: command>",
            examples: new string[] { $"{nameof(set_as_timed)} \"1\" \"10\" {{ {nameof(set_attribute_of)} \"Player\" \"position\" \"(1, 1, 0)\" }}" });
        set_as_timed.Action = (callTime, stopTime, command) =>
        {
            string foundCommandId = command.GetFirstWord();

            GameConsoleType<float> callTimeType = GameConsoleConvert.ToFloat(callTime);
            GameConsoleType<float> stopTimeType = GameConsoleConvert.ToFloat(stopTime);
            
            if (callTimeType == null)
            {
                PrintIncorrectTypeError("Argument", nameof(callTime));
            }
            else if (stopTimeType == null)
            {
                PrintIncorrectTypeError("Argument", nameof(stopTime));
            }
            else
            {
                GameConsoleCommandBase foundCommand = commands.Cast<GameConsoleCommandBase>().FirstOrDefault(c => c.CommandId == foundCommandId);

                if (foundCommand != null)
                {
                    int foundCommandsParametersAmount = foundCommand.GetParametersAmount();
                    string[] inputArguments = command.GetArguments(foundCommandId, new char[] { '\"', '\'' });

                    if (foundCommandsParametersAmount == 0 || foundCommandsParametersAmount == inputArguments.Length)
                    {
                        TimedCommandCallerCommand timedCommand;
                        switch (foundCommandsParametersAmount)
                        {
                            case 0:
                                timedCommand = new TimedCommandCallerCommand(foundCommand, callTimeType.Value, stopTimeType.Value,
                                    ((GameConsoleCommand)foundCommand).Action);
                                break;
                            case 1:
                                timedCommand = new TimedCommandCallerCommand(foundCommand, callTimeType.Value, stopTimeType.Value,
                                    ((GameConsoleCommand<string>)foundCommand).Action, inputArguments);
                                break;
                            case 2:
                                timedCommand = new TimedCommandCallerCommand(foundCommand, callTimeType.Value, stopTimeType.Value,
                                    ((GameConsoleCommand<string, string>)foundCommand).Action, inputArguments);
                                break;
                            case 3:
                                timedCommand = new TimedCommandCallerCommand(foundCommand, callTimeType.Value, stopTimeType.Value,
                                    ((GameConsoleCommand<string, string, string>)foundCommand).Action, inputArguments);
                                break;
                            default:
                                timedCommand = null;
                                break;
                        }

                        if (timedCommand != null)
                        {
                            timedCommandCallerCommands.AddIfUniqueCommand(timedCommand);
                        }
                        else
                        {
                            PrintNotRecognizedError("Command", foundCommandId);
                        }
                    }
                    else
                    {
                        PrintWrongUsageOfCommandError(foundCommandId);
                    }
                }
                else
                {
                    PrintNotFoundError("Command", foundCommandId);
                }
            }
        };

        GameConsoleCommand<string> stop_timed = new GameConsoleCommand<string>(
            id: $"{nameof(stop_timed)}",
            description: "Stop a timed command",
            format: $"{nameof(stop_timed)} <str: commandId>",
            examples: new string[] { $"{nameof(stop_timed)} \"{nameof(set_attribute_of)}\"" });
        stop_timed.Action = (commandId) =>
        {
            TimedCommandCallerCommand timedCommand = timedCommandCallerCommands.FirstOrDefault(c => c.Command.CommandId == commandId);
            
            if (timedCommand != null)
            {
                timedCommandCallerCommands.Remove(timedCommand);
                Print($"Stopped the timed command {timedCommand.Command.CommandId.AsBold()}", ConsoleOutputType.Explanation);
            }
            else
            {
                PrintNotFoundError("Timed command", commandId);
            }
        };

        set_test_object_position = new GameConsoleCommand<string>(
            id: $"{nameof(set_test_object_position)}",
            description: "Set test object position",
            format: $"{nameof(set_test_object_position)} <v3: position>",
            examples: new string[] { $"{nameof(set_test_object_position)} \"(1, 1, 0)\"" });
        set_test_object_position.Action = (position) =>
        {
            GameObject testObject = GameObject.Find("Test");

            if (testObject != null)
            {
                TestObject testObjectScript = testObject.GetComponent<TestObject>();

                if (testObjectScript != null)
                {
                    GameConsoleType<Vector3> gameConsoleType = GameConsoleConvert.ToVector3(position);
                    if (gameConsoleType != null)
                    {
                        testObjectScript.SetPosition(gameConsoleType.Value);
                        Print($"{testObject.transform.name}'s position is now {testObject.transform.position}", ConsoleOutputType.Explanation);
                    }
                    else
                    {
                        PrintIncorrectTypeError("Argument", nameof(position));
                    }
                }
                else
                {
                    PrintNotFoundError("Script", "TestObject");
                }
            }
            else
            {
                PrintNotFoundError("Game object", "Test");
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
            set_timescale,
            set_as_timed,
            stop_timed,

            set_test_object_position,
        });
        #endregion
    }

    private void Update()
    {
        if (Input.GetKeyDown(activationKey))
        {
            if (!activated)
            {
                activated = true;
                justActivated = true;
                canDeactivateConsole = false;
            }
        }

        // Timed commands running and removal
        HashSet<TimedCommandCallerCommand> timedCommandCallerCommandsToRemove = new HashSet<TimedCommandCallerCommand>();
        
        foreach (TimedCommandCallerCommand timedCommandCallerCommand in timedCommandCallerCommands)
        {
            if (timedCommandCallerCommand.TimedAction.Run())
            {
                Print($"Timed command {timedCommandCallerCommand.Command.CommandId.AsBold()} was invoked", ConsoleOutputType.Warning);
            }

            if (timedCommandCallerCommand.TimedAction.Disabled)
            {
                timedCommandCallerCommandsToRemove.Add(timedCommandCallerCommand);
            }
        }

        foreach (TimedCommandCallerCommand timedCommandCallerCommandToRemove in timedCommandCallerCommandsToRemove)
        {
            timedCommandCallerCommands.Remove(timedCommandCallerCommandToRemove);
        }
        // ----------

        // Coroutines
        if (!canRemoveWrappersWithBackspace && !canRemoveWrappersWithBackspaceAfterHasStarted)
        {
            StartCoroutine(CanRemoveWrappersWithBackspaceAfter(canRemoveWrappersWithBackspaceAfterTime));
        }
        if (!showSuggestions && !dontShowSuggestionsForHasStarted)
        {
            StartCoroutine(DontShowSuggestionsFor(dontShowSuggestionsForTime));
        }
        if (!canScrollSuggestions && !canScrollSuggestionsAfterHasStarted)
        {
            StartCoroutine(CanScrollSuggestionsAfter(canScrollSuggestionsAfterTime));
        }
        if (!canCompleteSuggestion && !canCompleteSuggestionAfterHasStarted)
        {
            StartCoroutine(CanCompleteSuggestionAfter(canCompleteSuggestionAfterTime));
        }
        if (!canDeactivateConsole && !canDeactivateConsoleAfterHasStarted)
        {
            StartCoroutine(CanDeactivateConsoleAfter(canDeactivateConsoleAfterTime));
        }
        // ----------
    }

    private Vector2 scrollViewScrollPosition;
    private Rect scrollViewViewRect;

    private void OnGUI()
    {
        if (!activated)
        {
            return;
        }

        HandleOutputField(0, ref scrollViewViewRect, ref scrollViewScrollPosition);
        HandleInputField(outputBoxHeight, ref input);

        switch (Event.current.keyCode)
        {
            case KeyCode.Return:
                if (!string.IsNullOrWhiteSpace(input))
                {
                    HandleInput(input);
                    input = string.Empty;
                }
                break;
            case KeyCode keyCode when keyCode == activationKey:
                if (canDeactivateConsole)
                {
                    input = string.Empty;
                    activated = false;
                    canDeactivateConsole = false;
                }
                break;
        }
    }

    readonly int scrollViewViewRectMarginRight = 30;

    readonly int scrollViewScrollPositionMarginTop = 5;
    readonly int scrollViewScrollPositionMarginBottom = 10;

    readonly int outputBoxPaddingTop = 2;
    readonly int outputLabelMarginLeft = 10;
    readonly int outputLabelMarginBottom = 2;

    int previousOutputCount = -1;

    private void HandleOutputField(float outputPositionY, ref Rect scrollViewViewRect, ref Vector2 scrollViewScrollPosition)
    {
        Rect borderedBoxContentPartRect = GUIExtensions.BorderedBox(new Rect(0, outputPositionY, Screen.width, outputBoxHeight), string.Empty, outputBoxStyle,
            outputBoxBorderSize, outputBoxBorderStyle);

        scrollViewViewRect = new Rect(borderedBoxContentPartRect.x, borderedBoxContentPartRect.y, borderedBoxContentPartRect.width - scrollViewViewRectMarginRight,
            (outputLabelHeight + outputLabelMarginBottom) * outputs.Count);

        if (previousOutputCount != outputs.Count)
        {
            scrollViewScrollPosition.y = scrollViewViewRect.height;
            previousOutputCount = outputs.Count;
        }
        
        scrollViewScrollPosition = GUI.BeginScrollView(new Rect(borderedBoxContentPartRect.x, borderedBoxContentPartRect.y + scrollViewScrollPositionMarginTop,
            borderedBoxContentPartRect.width, borderedBoxContentPartRect.height - scrollViewScrollPositionMarginBottom), scrollViewScrollPosition, scrollViewViewRect);

        outputPositionY = borderedBoxContentPartRect.y + outputBoxPaddingTop;

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
            GUI.Label(new Rect(borderedBoxContentPartRect.x + outputLabelMarginLeft, outputPositionY, borderedBoxContentPartRect.width,
                outputLabelHeight + outputLabelMarginBottom), $"{newOutputText}", outputLabelStyle);

            outputPositionY += outputLabelHeight + outputLabelMarginBottom;
        }

        GUI.EndScrollView();
    }

    readonly int inputBoxHeightAdditionToTextField = 10;

    readonly int textFieldMarginLeft = 10;
    readonly int textFieldMarginTop = 5;
    readonly int textFieldMarginRight = 20;

    int previousInputsCount = 0;
    bool canRemoveWrappersWithBackspace = true;
    bool showSuggestions = true;
    int currentSuggestionIndex = 0;
    int currentInputHistoryIndex = 0;
    string previousInput = string.Empty;
    bool canScrollSuggestions = true;
    bool canCompleteSuggestion = true;
    bool showInputHistorySuggestion = false;
    int delayedCaretIndex = -1;
    bool selectNewParagraph = false;

    private void HandleInputField(float inputPositionY, ref string input)
    {
        Rect borderedBoxContentPartRect = GUIExtensions.BorderedBox(new Rect(0, inputPositionY, Screen.width, inputTextFieldHeight + inputBoxHeightAdditionToTextField),
            string.Empty, inputBoxStyle, inputBoxBorderSize, inputBoxBorderStyle, BorderDirection.Left | BorderDirection.Right | BorderDirection.Bottom);

        int textFieldControlId = GUIUtility.keyboardControl;
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

        // Handle wrapper removal logic
        int caretIndex = textEditor.GetCaretIndex();

        bool caretIndexSurroundedWithWrappers = false;
        bool caretIndexSurroundedWithTooManyWrappers = false;
        bool dontAddWrappers = false;
        bool pressedBackspace = false;

        if (caretIndex > 0 && (input.GetCharAt(caretIndex - 1) == '\"' && input.GetCharAt(caretIndex) == '\"'
            || input.GetCharAt(caretIndex - 1) == '\'' && input.GetCharAt(caretIndex) == '\''))
        {
            caretIndexSurroundedWithWrappers = true;
            dontAddWrappers = true;

            if (input.GetCharAt(caretIndex - 2) == '\"' || input.GetCharAt(caretIndex + 1) == '\"'
                || input.GetCharAt(caretIndex - 2) == '\'' || input.GetCharAt(caretIndex + 1) == '\'')
            {
                caretIndexSurroundedWithTooManyWrappers = true;
            }
        }
        // ----------

        if (Event.current.isKey)
        {
            if (Event.current.keyCode == KeyCode.Backspace)
            {
                if (canRemoveWrappersWithBackspace && caretIndexSurroundedWithWrappers && !caretIndexSurroundedWithTooManyWrappers)
                {
                    input = input.Remove(caretIndex - 1, 2);
                    textEditor.SetCaretIndex(caretIndex - 1);
                    Event.current.keyCode = KeyCode.None;
                }

                dontAddWrappers = true;
                canRemoveWrappersWithBackspace = false;
                pressedBackspace = true;
            }
            else if (Event.current.control && (Event.current.keyCode == KeyCode.X || Event.current.keyCode == KeyCode.V))
            {
                dontAddWrappers = true;
            }
        }

        if (string.IsNullOrEmpty(input))
        {
            previousCommandInputSuggestions.Clear();

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
                                showSuggestions = false;
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
                            canScrollSuggestions = false;
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
                            canScrollSuggestions = false;
                        }
                        break;
                }
            }
        }
        else
        {
            currentInputHistoryIndex = inputs.Count;
            showInputHistorySuggestion = false;

            GetCommandInputSuggestions(input, out HashSet<string> commandInputSuggestions);

            ReadOnlyValueHolder<string> readOnlyTempInput = new ReadOnlyValueHolder<string>(input);
            if (showSuggestions && !commandInputSuggestions.Any(s => s == readOnlyTempInput.Value))
            {
                string currentSuggestion = commandInputSuggestions.GetClosestAt(ref currentSuggestionIndex);

                if (Event.current.isKey)
                {
                    switch (Event.current.keyCode)
                    {
                        case KeyCode.Tab:
                            if (commandInputSuggestions.Count > 0)
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
                                        canCompleteSuggestion = false;
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
                            if (commandInputSuggestions.Count > 0 && canScrollSuggestions)
                            {
                                // Show previous suggestion

                                currentSuggestionIndex--;
                                currentSuggestion = commandInputSuggestions.GetClosestAt(ref currentSuggestionIndex);
                                canScrollSuggestions = false;
                            }
                            break;
                        case KeyCode.DownArrow:
                            Event.current.keyCode = KeyCode.None;
                            if (commandInputSuggestions.Count > 0 && canScrollSuggestions)
                            {
                                // Show next suggestion

                                currentSuggestionIndex++;
                                currentSuggestion = commandInputSuggestions.GetClosestAt(ref currentSuggestionIndex);
                                canScrollSuggestions = false;
                            }
                            break;
                    }
                }

                inputSuggestion = !string.IsNullOrEmpty(currentSuggestion) ? currentSuggestion.RemovePartFromStart(input) : string.Empty;
            }
        }

        string selectedTextBeforeTextFieldUpdate = textEditor.SelectedText;

        GUI.SetNextControlName("InputField");
        
        string _input = GUI.TextField(new Rect(borderedBoxContentPartRect.x + textFieldMarginLeft, borderedBoxContentPartRect.y + textFieldMarginTop,
            borderedBoxContentPartRect.width - textFieldMarginRight, inputTextFieldHeight), $"{input}" +
            $"{(!string.IsNullOrEmpty(inputSuggestion) ? $"<color={inputSuggestionTextColor.ToString().ToLower()}>{inputSuggestion}</color>" : string.Empty)}",
            inputTextFieldStyle);

        input = !string.IsNullOrEmpty(inputSuggestion) ? _input.Replace($"<color={inputSuggestionTextColor.ToString().ToLower()}>{inputSuggestion}</color>", string.Empty)
            : _input;

        GUI.FocusControl("InputField");

        if (delayedCaretIndex > -1)
        {
            if (selectNewParagraph)
            {
                textEditor.SelectParagraphForward();
                textEditor.SetCaretIndex(delayedCaretIndex, false);

                selectNewParagraph = false;
            }
            else
            {
                textEditor.SetCaretIndex(delayedCaretIndex);
            }

            delayedCaretIndex = -1;
        }

        // On input change
        if (input != previousInput)
        {
            caretIndex = textEditor.GetCaretIndex();

            if (!dontAddWrappers)
            {
                // Handle wrapper addition logic
                if (selectedTextBeforeTextFieldUpdate.Length > 0)
                {
                    int selectIndex = textEditor.selectIndex;

                    if (selectIndex > 0)
                    {
                        char foundChar = input.TryGetWrapperAt(selectIndex - 1, new char[] { '"', '\'' });

                        if (foundChar != '\0')
                        {
                            input = input.Remove(selectIndex - 1, 1).Insert(selectIndex - 1, selectedTextBeforeTextFieldUpdate.WrapWith(foundChar));

                            delayedCaretIndex = selectIndex + selectedTextBeforeTextFieldUpdate.Length;
                            selectNewParagraph = true;
                        }
                    }
                }
                else if (caretIndex > 0)
                {
                    char foundChar = input.TryGetWrapperAt(caretIndex - 1, new char[] { '"', '\'' });

                    if (foundChar != '\0')
                    {
                        input = input.Remove(caretIndex - 1, 1).AddWrappersTo(caretIndex - 1, foundChar);
                    }
                }
                // ----------
            }
            else if (caretIndex > 0 && caretIndexSurroundedWithWrappers && !pressedBackspace)
            {
                if (input.TryGetWrapperAt(caretIndex - 1, new char[] { '"', '\'' }) != '\0')
                {
                    input = input.Remove(caretIndex - 1, 1);
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

    HashSet<string> previousCommandInputSuggestions = new HashSet<string>();

    [RelatedTo(nameof(HandleInputField), RelationTargetType.Method)]
    private void GetCommandInputSuggestions(string input, out HashSet<string> commandInputSuggestions)
    {
        commandInputSuggestions = new HashSet<string>();

        if (!string.IsNullOrEmpty(input))
        {
            foreach (var gameConsoleCommand in commands.Cast<GameConsoleCommandBase>())
            {
                string previousSameSuggestion = previousCommandInputSuggestions.FirstOrDefault(s => s == gameConsoleCommand.CommandId || s.StartsWith(gameConsoleCommand.CommandId + " "));

                string suggestion;
                if (!string.IsNullOrEmpty(previousSameSuggestion))
                {
                    suggestion = previousSameSuggestion;
                }
                else
                {
                    suggestion = gameConsoleCommand.GetInputSuggestion();
                }

                if (suggestion.ToLower().StartsWith(input.ToLower()))
                {
                    commandInputSuggestions.Add(suggestion);
                }
            }
        }

        previousCommandInputSuggestions = new HashSet<string>(commandInputSuggestions);
    }

    [RelatedTo(nameof(OnGUI), RelationTargetType.Method)]
    private bool HandleInput(string input)
    {
        inputs.Add(input);
        Print($"> {input}", ConsoleOutputType.Information);

        string[] inputParts;
        if (input.ContainsAmountOf('\"') > 1 || input.ContainsAmountOf('\'') > 1)
        {
            inputParts = input.SplitAllNonWrapped(' ', new char[] { '\"', '\'' });
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

    private void PrintNotRecognizedError(string thing, string thingName)
    {
        Print($"{thing} \"{thingName}\" not recognized", ConsoleOutputType.Error);
    }

    private void PrintIncorrectTypeError(string thing, string thingName)
    {
        Print($"{thing} \"{thingName}\" is not the correct type", ConsoleOutputType.Error);
    }

    private void PrintNotFoundError(string thing, string thingName)
    {
        Print($"{thing} \"{thingName}\" not found", ConsoleOutputType.Error);
    }

    private void PrintWrongUsageOfCommandError(string commandId)
    {
        Print($"Incorrect usage of the command \"{commandId}\" (use \"help_of {commandId.AsBold()}\" to get details about the command)", ConsoleOutputType.Error);
    }

    private RichTextColor GetRichTextColorByOutputType(ConsoleOutputType outputType)
    {
        switch (outputType)
        {
            case ConsoleOutputType.Explanation:
                return outputExplanationTextColor;
            case ConsoleOutputType.Warning:
                return outputWarningTextColor;
            case ConsoleOutputType.Error:
                return outputErrorTextColor;
            default:
                throw new ArgumentException("Didn't find the matching output type.");
        }
    }

    #region IEnumerators
    bool canRemoveWrappersWithBackspaceAfterHasStarted = false;
    /// <summary>
    /// Used to set small delay to do a thing (since OnGui() is called many times per frame)
    /// </summary>
    private IEnumerator CanRemoveWrappersWithBackspaceAfter(float seconds)
    {
        canRemoveWrappersWithBackspaceAfterHasStarted = true;

        yield return new WaitForSecondsRealtime(seconds);

        canRemoveWrappersWithBackspace = true;
        canRemoveWrappersWithBackspaceAfterHasStarted = false;
    }

    bool dontShowSuggestionsForHasStarted = false;
    /// <summary>
    /// Used to set small delay to do a thing (since OnGui() is called many times per frame)
    /// </summary>
    private IEnumerator DontShowSuggestionsFor(float seconds)
    {
        dontShowSuggestionsForHasStarted = true;

        yield return new WaitForSecondsRealtime(seconds);

        showSuggestions = true;
        dontShowSuggestionsForHasStarted = false;
    }

    bool canScrollSuggestionsAfterHasStarted = false;
    /// <summary>
    /// Used to set small delay to do a thing (since OnGui() is called many times per frame)
    /// </summary>
    private IEnumerator CanScrollSuggestionsAfter(float seconds)
    {
        canScrollSuggestionsAfterHasStarted = true;

        yield return new WaitForSecondsRealtime(seconds);

        canScrollSuggestions = true;
        canScrollSuggestionsAfterHasStarted = false;
    }

    bool canCompleteSuggestionAfterHasStarted = false;
    /// <summary>
    /// Used to set small delay to do a thing (since OnGui() is called many times per frame)
    /// </summary>
    private IEnumerator CanCompleteSuggestionAfter(float seconds)
    {
        canCompleteSuggestionAfterHasStarted = true;

        yield return new WaitForSecondsRealtime(seconds);

        canCompleteSuggestion = true;
        canCompleteSuggestionAfterHasStarted = false;
    }

    bool canDeactivateConsoleAfterHasStarted = false;
    /// <summary>
    /// Used to set small delay to do a thing (since OnGui() is called many times per frame)
    /// </summary>
    private IEnumerator CanDeactivateConsoleAfter(float seconds)
    {
        canDeactivateConsoleAfterHasStarted = true;

        yield return new WaitForSecondsRealtime(seconds);

        canDeactivateConsole = true;
        canDeactivateConsoleAfterHasStarted = false;
    }
    #endregion

    #region Public methods
    /// <summary>
    /// Set the console activation key.
    /// </summary>
    public void SetActivationKey(KeyCode keyCode)
    {
        activationKey = keyCode;
    }

    /// <summary>
    /// Get the console activation key.
    /// </summary>
    public KeyCode GetActivationKey()
    {
        return activationKey;
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
        string timestamp = string.Empty;
        if (showTimestamps)
        {
            timestamp = $"{DateTime.Now.ToString("T")} - ";
            if (!timestampsUseParentColor)
            {
                timestamp = timestamp.AsColor(timestampsColor);
                try
                {
                    RichTextColor richTextColor = GetRichTextColorByOutputType(outputType);
                    text = text.AsColor(richTextColor);
                }
                catch (ArgumentException)
                {
                }
                outputType = ConsoleOutputType.Custom;
            }
        }

        string outputText = $"{timestamp}{text}";
        GameConsoleOutput output = new GameConsoleOutput(outputText, outputType);
        outputs.Add(output);
    }

    /// <summary>
    /// Clear the console.
    /// </summary>
    public void Clear()
    {
        outputs.Clear();
    }
    #endregion
}
