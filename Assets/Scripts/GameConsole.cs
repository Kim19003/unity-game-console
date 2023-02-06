using Assets.Scripts.Attributes;
using Assets.Scripts.Extensions;
using Assets.Scripts.Other;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEditor.Progress;
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
    [SerializeField] private RichTextColor outputHighlightTextColor = RichTextColor.Lightblue;
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

    private GUIStyle outputBoxStyle, inputBoxStyle, outputBoxBorderStyle, inputBoxBorderStyle, outputLabelStyle, inputTextFieldStyle;

    private string input = string.Empty;
    
    private readonly char[] generalInputWrappers = new char[] { '\"', '\'' };
    private readonly (char, char)[] commandInputWrappers = new (char, char)[] { ('{', '}') };
    private readonly (char, char)[] objectInputWrappers = new (char, char)[] { ('(', ')') };

    private bool activated = false;
    private bool justActivated = false;
    private bool canDeactivateConsole = false;

    private readonly HashSet<TimedCommandCallerCommand> timedCommandCallerCommands = new HashSet<TimedCommandCallerCommand>();
    private readonly Dictionary<string, string> aliases = new Dictionary<string, string>();
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
        else if (outputBoxBackgroundColorAlpha > 0)
        {
            outputBoxBackgroundTexture.FillWithColor(new Color(outputBoxBackgroundColor.r, outputBoxBackgroundColor.g, outputBoxBackgroundColor.b, outputBoxBackgroundColorAlpha));
        }

        if (inputBoxBackgroundTexture == null)
        {
            inputBoxBackgroundTexture = Helpers.MakeTexture(1, 1, new Color(inputBoxBackgroundColor.r, inputBoxBackgroundColor.g, inputBoxBackgroundColor.b, inputBoxBackgroundColorAlpha));
        }
        else if (inputBoxBackgroundColorAlpha > 0)
        {
            inputBoxBackgroundTexture.FillWithColor(new Color(inputBoxBackgroundColor.r, inputBoxBackgroundColor.g, inputBoxBackgroundColor.b, inputBoxBackgroundColorAlpha));
        }

        if (outputBoxBorderBackgroundTexture == null)
        {
            outputBoxBorderBackgroundTexture = Helpers.MakeTexture(1, 1, new Color(outputBoxBorderBackgroundColor.r, outputBoxBorderBackgroundColor.g, outputBoxBorderBackgroundColor.b, outputBoxBorderBackgroundColorAlpha));
        }
        else if (outputBoxBorderBackgroundColorAlpha > 0)
        {
            outputBoxBorderBackgroundTexture.FillWithColor(new Color(outputBoxBorderBackgroundColor.r, outputBoxBorderBackgroundColor.g, outputBoxBorderBackgroundColor.b, outputBoxBorderBackgroundColorAlpha));
        }

        if (inputBoxBorderBackgroundTexture == null)
        {
            inputBoxBorderBackgroundTexture = Helpers.MakeTexture(1, 1, new Color(inputBoxBorderBackgroundColor.r, inputBoxBorderBackgroundColor.g, inputBoxBorderBackgroundColor.b, inputBoxBorderBackgroundColorAlpha));
        }
        else if (inputBoxBorderBackgroundColorAlpha > 0)
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
        GameConsoleCommand help = new GameConsoleCommand(
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

        GameConsoleCommand<string> help_of = new GameConsoleCommand<string>(
            id: $"{nameof(help_of)}",
            description: "Show information about a command",
            format: $"{nameof(help_of)} <str: commandId>",
            examples: new string[] { $"{nameof(help_of)} {nameof(help)}" });
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
        help_of.SetInputSuggestion(() =>
        {
            return $"\"{commands.Cast<GameConsoleCommandBase>().Where(c => c.CommandId != help_of.CommandId).Select(c => c.CommandId).GetRandomElement()}\"";
        });

        GameConsoleCommand clear = new GameConsoleCommand(
            id: $"{nameof(clear)}",
            description: "Clear the console",
            format: $"{nameof(clear)}",
            examples: new string[] { $"{nameof(clear)}" });
        clear.Action = () =>
        {
            Clear();
        };

        GameConsoleCommand<string> print = new GameConsoleCommand<string>(
            id: $"{nameof(print)}",
            description: "Print text to the console",
            format: $"{nameof(print)} <str: text>",
            examples: new string[] { $"{nameof(print)} \"Hello world!\"" });
        print.Action = (text) =>
        {
            Print(text, ConsoleOutputType.Explanation);
        };

        GameConsoleCommand quit = new GameConsoleCommand(
            id: $"{nameof(quit)}",
            description: "Quit the game",
            format: $"{nameof(quit)}",
            examples: new string[] { $"{nameof(quit)}" });
        quit.Action = () =>
        {
            Application.Quit();
        };

        GameConsoleCommand<string> load_scene = new GameConsoleCommand<string>(
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

        GameConsoleCommand reload = new GameConsoleCommand(
            id: $"{nameof(reload)}",
            description: "Reload the current scene",
            format: $"{nameof(reload)}",
            examples: new string[] { $"{nameof(reload)}" });
        reload.Action = () =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            Print($"Reloaded the current scene", ConsoleOutputType.Explanation);
        };

        GameConsoleCommand fullscreen = new GameConsoleCommand(
            id: $"{nameof(fullscreen)}",
            description: "Switch to the fullscreen",
            format: $"{nameof(fullscreen)}",
            examples: new string[] { $"{nameof(fullscreen)}" });
        fullscreen.Action = () =>
        {
            Screen.fullScreen = !Screen.fullScreen;
            Print($"Fullscreen: {Screen.fullScreen}", ConsoleOutputType.Explanation);
        };

        GameConsoleCommand<string> destroy = new GameConsoleCommand<string>(
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

        GameConsoleCommand<string, string> set_active = new GameConsoleCommand<string, string>(
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

        GameConsoleCommand<string, string> get_attribute_of = new GameConsoleCommand<string, string>(
            id: $"{nameof(get_attribute_of)}",
            description: "Find a game object by name and get it's attribute value",
            format: $"{nameof(get_attribute_of)} <str: gameObjectName> <str: attributeName>",
            examples: new string[] { $"{nameof(get_attribute_of)} \"Player\" position" });
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

        GameConsoleCommand<string, string, string> set_attribute_of = new GameConsoleCommand<string, string, string>(
            id: $"{nameof(set_attribute_of)}",
            description: "Find a game object by name and set it's attribute value",
            format: $"{nameof(set_attribute_of)} <str: gameObjectName> <str: attributeName> <any: attributeValue>",
            examples: new string[] { $"{nameof(set_attribute_of)} \"Player\" position (1, 1, 0)" });
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

        GameConsoleCommand get_admitted_attribute_names = new GameConsoleCommand(
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

        GameConsoleCommand get_command_ids = new GameConsoleCommand(
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

        GameConsoleCommand<string> set_timescale = new GameConsoleCommand<string>(
            id: $"{nameof(set_timescale)}",
            description: "Set the scale at which time passes",
            format: $"{nameof(set_timescale)} <flt: timeScale>",
            examples: new string[] { $"{nameof(set_timescale)} 1" });
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

        GameConsoleCommand<string, string, string> set_as_timed = new GameConsoleCommand<string, string, string>(
            id: $"{nameof(set_as_timed)}",
            description: "Set a command as an active timed command",
            format: $"{nameof(set_as_timed)} <flt: callTime> <flt: stopTime> <cmd: command>",
            examples: new string[] { $"{nameof(set_as_timed)} 1 10 {{ {nameof(set_attribute_of)} \"Player\" position (1, 1, 0) }}" });
        set_as_timed.Action = (callTime, stopTime, command) =>
        {
            command = command.RemoveCommandInputWrappers(commandInputWrappers);

            string foundCommandId = command.GetFirstWord().Replace(generalInputWrappers[0].ToString(),
                string.Empty).Replace(generalInputWrappers[1].ToString(), string.Empty);

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
                    string[] inputArguments = command.GetArguments(foundCommandId, generalInputWrappers);

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
                            Print($"Set {foundCommandId.AsBold()} as an active timed command", ConsoleOutputType.Explanation);
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
            description: "Stop an active timed command",
            format: $"{nameof(stop_timed)} <str: commandId>",
            examples: new string[] { $"{nameof(stop_timed)} >all" });
        stop_timed.Action = (commandId) =>
        {
            if (commandId.ToLower() == ">all")
            {
                if (timedCommandCallerCommands.Count > 0)
                {
                    timedCommandCallerCommands.Clear();
                    Print($"Stopped all the active timed commands", ConsoleOutputType.Explanation);
                }
                else
                {
                    Print($"No active timed commands to stop", ConsoleOutputType.Error);
                }
            }
            else
            {
                TimedCommandCallerCommand timedCommand = timedCommandCallerCommands.FirstOrDefault(c => c.Command.CommandId == commandId);

                if (timedCommand != null)
                {
                    timedCommandCallerCommands.Remove(timedCommand);
                    Print($"Stopped the active timed command {timedCommand.Command.CommandId.AsBold()}", ConsoleOutputType.Explanation);
                }
                else
                {
                    PrintNotFoundError("Timed command", commandId);
                }
            }
        };

        GameConsoleCommand get_all_timed = new GameConsoleCommand(
            id: $"{nameof(get_all_timed)}",
            description: "Get all the active timed commands",
            format: $"{nameof(get_all_timed)}",
            examples: new string[] { $"{nameof(get_all_timed)}" });
        get_all_timed.Action = () =>
        {
            if (timedCommandCallerCommands.Count > 0)
            {
                int i = 0;
                foreach (TimedCommandCallerCommand timedCommandCallerCommand in timedCommandCallerCommands)
                {
                    DateTime endTime = timedCommandCallerCommand.CreationTime.AddSeconds(timedCommandCallerCommand.DisableAfter);
                    Print($"{i + 1}. {timedCommandCallerCommand.Command.CommandId.AsBold()} (started {timedCommandCallerCommand.CreationTime.ToString("T")}, " +
                        $"ends {endTime.ToString("T")})", ConsoleOutputType.Explanation);
                    i++;
                }
            }
            else
            {
                Print($"No active timed commands found", ConsoleOutputType.Error);
            }
        };

        GameConsoleCommand<string, string> set_alias = new GameConsoleCommand<string, string>(
            id: $"{nameof(set_alias)}",
            description: "Set alias",
            format: $"{nameof(set_alias)} <str: alias> <any: content>",
            examples: new string[] { $"{nameof(set_alias)} \"h\" {{ help }}" });
        set_alias.Action = (alias, content) =>
        {
            if (!alias.Contains(" "))
            {
                if (!commands.Cast<GameConsoleCommandBase>().Any(c => c.CommandId == alias))
                {
                    content = content.RemoveCommandInputWrappers(commandInputWrappers);
                    
                    if (!aliases.ContainsKey(alias))
                    {
                        aliases.Add(alias, content);
                    }
                    else
                    {
                        aliases[alias] = content;
                    }

                    Print($"{alias.AsBold()} -> {{ {content} }}", ConsoleOutputType.Explanation);
                }
                else
                {
                    Print($"Alias can't be the same as any of the command ids", ConsoleOutputType.Error);
                }
            }
            else
            {
                Print($"Alias can't contain spaces", ConsoleOutputType.Error);
            }
        };

        GameConsoleCommand<string> remove_alias = new GameConsoleCommand<string>(
            id: $"{nameof(remove_alias)}",
            description: "Remove alias",
            format: $"{nameof(remove_alias)} <str: alias>",
            examples: new string[] { $"{nameof(remove_alias)} >all" });
        remove_alias.Action = (alias) =>
        {
            if (alias.ToLower() == ">all")
            {
                if (aliases.Count > 0)
                {
                    aliases.Clear();
                    Print($"Removed all aliases", ConsoleOutputType.Explanation);
                }
                else
                {
                    Print($"No aliases to remove", ConsoleOutputType.Error);
                }
            }
            else if (aliases.ContainsKey(alias))
            {
                aliases.Remove(alias);
                Print($"Removed alias {alias.AsBold()}", ConsoleOutputType.Explanation);
            }
            else
            {
                PrintNotFoundError("Alias", alias);
            }
        };

        GameConsoleCommand get_all_aliases = new GameConsoleCommand(
            id: $"{nameof(get_all_aliases)}",
            description: "Get all aliases",
            format: $"{nameof(get_all_aliases)}",
            examples: new string[] { $"{nameof(get_all_aliases)}" });
        get_all_aliases.Action = () =>
        {
            if (aliases.Count > 0)
            {
                int i = 0;
                foreach (var alias in aliases)
                {
                    Print($"{i + 1}. {alias.Key.AsBold()} -> {{ {alias.Value} }}", ConsoleOutputType.Explanation);
                    i++;
                }
            }
            else
            {
                Print($"No aliases found", ConsoleOutputType.Error);
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
            get_all_timed,
            set_alias,
            remove_alias,
            get_all_aliases,
        });
        #endregion
    }

    private void Start()
    {
        HashSet<string> commandIds = new HashSet<string>();
        foreach (GameConsoleCommandBase command in commands.Cast<GameConsoleCommandBase>())
        {
            if (!commandIds.Contains(command.CommandId))
            {
                commandIds.Add(command.CommandId);
            }
            else
            {
                throw new ArgumentException($"The command id '{command.CommandId}' is not unique");
            }
        }
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
                Print($"Timed command {timedCommandCallerCommand.Command.CommandId.AsBold()} was just invoked", ConsoleOutputType.Highlight);
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
                    newOutputText = output.Text.RemoveTags("color").AsColor(outputExplanationTextColor);
                    break;
                case ConsoleOutputType.Highlight:
                    newOutputText = output.Text.RemoveTags("color").AsColor(outputHighlightTextColor);
                    break;
                case ConsoleOutputType.Warning:
                    newOutputText = output.Text.RemoveTags("color").AsColor(outputWarningTextColor);
                    break;
                case ConsoleOutputType.Error:
                    newOutputText = output.Text.RemoveTags("color").AsColor(outputErrorTextColor);
                    break;
                case ConsoleOutputType.Custom:
                    newOutputText = output.Text;
                    break;
                default:
                    newOutputText = output.Text.RemoveTags("color").AsColor(RichTextColor.White);
                    break;
            }
            GUI.Label(new Rect(borderedBoxContentPartRect.x + outputLabelMarginLeft, outputPositionY, borderedBoxContentPartRect.width,
                outputLabelHeight + outputLabelMarginBottom), newOutputText, outputLabelStyle);

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
    int delayedSelectionIndex = -1;
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

        bool caretIndexSurroundedWithGeneralWrappers = false;
        bool caretIndexSurroundedWithTooManyGeneralWrappers = false;
        bool dontAddGeneralWrappers = false;
        
        bool caretIndexSurroundedWithCommandWrappers = false;
        bool dontAddCommandWrappers = false;

        bool caretIndexSurroundedWithObjectWrappers = false;
        bool dontAddObjectWrappers = false;

        bool pressedBackspace = false;

        if (caretIndex > 0)
        {
            if (input.GetCharAt(caretIndex - 1) == generalInputWrappers[0] && input.GetCharAt(caretIndex) == generalInputWrappers[0]
                || input.GetCharAt(caretIndex - 1) == generalInputWrappers[1] && input.GetCharAt(caretIndex) == generalInputWrappers[1])
            {
                caretIndexSurroundedWithGeneralWrappers = true;
                dontAddGeneralWrappers = true;

                if (input.GetCharAt(caretIndex - 2) == generalInputWrappers[0] || input.GetCharAt(caretIndex + 1) == generalInputWrappers[0]
                    || input.GetCharAt(caretIndex - 2) == generalInputWrappers[1] || input.GetCharAt(caretIndex + 1) == generalInputWrappers[1])
                {
                    caretIndexSurroundedWithTooManyGeneralWrappers = true;
                }
            }
            else if (input.GetCharAt(caretIndex - 2) == commandInputWrappers[0].Item1 && input.GetCharAt(caretIndex + 1) == commandInputWrappers[0].Item2
                && input.GetCharAt(caretIndex - 1) == ' ' && input.GetCharAt(caretIndex) == ' ')
            {
                caretIndexSurroundedWithCommandWrappers = true;
                dontAddCommandWrappers = true;
            }
            else if (input.GetCharAt(caretIndex - 1) == objectInputWrappers[0].Item1 && input.GetCharAt(caretIndex) == objectInputWrappers[0].Item2)
            {
                caretIndexSurroundedWithObjectWrappers = true;
                dontAddObjectWrappers = true;
            }
        }
        // ----------

        if (Event.current.isKey)
        {
            if (Event.current.keyCode == KeyCode.Backspace)
            {
                if (canRemoveWrappersWithBackspace)
                {
                    bool removedWrappers = false;

                    if (caretIndexSurroundedWithGeneralWrappers && !caretIndexSurroundedWithTooManyGeneralWrappers)
                    {
                        input = input.Remove(caretIndex - 1, 2);
                        textEditor.SetCaretIndex(caretIndex - 1);
                        removedWrappers = true;
                    }
                    else if (caretIndexSurroundedWithCommandWrappers)
                    {
                        input = input.Remove(caretIndex - 2, 4);
                        textEditor.SetCaretIndex(caretIndex - 2);
                        removedWrappers = true;
                    }
                    else if (caretIndexSurroundedWithObjectWrappers)
                    {
                        input = input.Remove(caretIndex - 1, 2);
                        textEditor.SetCaretIndex(caretIndex - 1);
                        removedWrappers = true;
                    }

                    if (removedWrappers)
                    {
                        Event.current.keyCode = KeyCode.None;
                    }
                }

                dontAddGeneralWrappers = true;
                dontAddCommandWrappers = true;
                dontAddObjectWrappers = true;
                canRemoveWrappersWithBackspace = false;
                pressedBackspace = true;
            }
            else if (Event.current.control && (Event.current.keyCode == KeyCode.X || Event.current.keyCode == KeyCode.V))
            {
                dontAddGeneralWrappers = true;
                dontAddCommandWrappers = true;
                dontAddObjectWrappers = true;
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
                                dontAddGeneralWrappers = true;
                                dontAddCommandWrappers = true;
                                dontAddObjectWrappers = true;
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
                                    dontAddGeneralWrappers = true;
                                    dontAddCommandWrappers = true;
                                    dontAddObjectWrappers = true;
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

        if (delayedSelectionIndex > -1)
        {
            textEditor.selectIndex = delayedSelectionIndex;

            delayedSelectionIndex = -1;
        }

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
            HandleWrappersAddition(ref input, textEditor, selectedTextBeforeTextFieldUpdate, ref delayedCaretIndex, ref delayedSelectionIndex, ref selectNewParagraph,
                dontAddGeneralWrappers, dontAddCommandWrappers, dontAddObjectWrappers, caretIndexSurroundedWithGeneralWrappers, caretIndexSurroundedWithCommandWrappers,
                caretIndexSurroundedWithObjectWrappers, pressedBackspace);

            currentSuggestionIndex = 0;

            previousInput = input;
        }
        
        // On activate
        if (justActivated)
        {


            justActivated = false;
        }
    }

    [RelatedTo(nameof(HandleInputField), RelationTargetType.Method)]
    private void HandleWrappersAddition(ref string input, TextEditor textEditor, string selectedTextBeforeTextFieldUpdate, ref int delayedCaretIndex,
        ref int delayedSelectionIndex, ref bool selectNewParagraph, bool dontAddGeneralWrappers, bool dontAddCommandWrappers, bool dontAddObjectWrappers,
        bool caretIndexSurroundedWithGeneralWrappers, bool caretIndexSurroundedWithCommandWrappers, bool caretIndexSurroundedWithObjectWrappers, bool pressedBackspace)
    {
        int caretIndex = textEditor.GetCaretIndex();

        if (!dontAddGeneralWrappers)
        {
            if (selectedTextBeforeTextFieldUpdate.Length > 0)
            {
                int selectIndex = textEditor.selectIndex;

                if (selectIndex > 0)
                {
                    string firstWrapper = string.Empty, secondWrapper = string.Empty;
                    int fixedCaretIndex = -1, fixedSelectionIndex = -1;
                    bool makeNewParagraphSelected = false;

                    char foundChar = input[selectIndex - 1];
                    if (generalInputWrappers.Contains(foundChar))
                    {
                        foreach (char generalInputWrapper in generalInputWrappers)
                        {
                            if (generalInputWrapper == foundChar)
                            {
                                firstWrapper = generalInputWrapper.ToString();
                                secondWrapper = generalInputWrapper.ToString();
                                fixedCaretIndex = selectIndex + selectedTextBeforeTextFieldUpdate.Length;
                                makeNewParagraphSelected = true;

                                break;
                            }
                        }
                    }
                    else if (!dontAddCommandWrappers && commandInputWrappers.Select(c => c.Item1).Contains(foundChar))
                    {
                        foreach ((char, char) commandInputWrapper in commandInputWrappers)
                        {
                            if (commandInputWrapper.Item1 == foundChar)
                            {
                                firstWrapper = $"{commandInputWrapper.Item1} ";
                                secondWrapper = $" {commandInputWrapper.Item2}";
                                fixedCaretIndex = selectIndex + selectedTextBeforeTextFieldUpdate.Length + 1;
                                fixedSelectionIndex = selectIndex + 1;
                                makeNewParagraphSelected = true;

                                break;
                            }
                        }
                    }
                    else if (!dontAddObjectWrappers && objectInputWrappers.Select(c => c.Item1).Contains(foundChar))
                    {
                        foreach ((char, char) objectInputWrapper in objectInputWrappers)
                        {
                            if (objectInputWrapper.Item1 == foundChar)
                            {
                                firstWrapper = objectInputWrapper.Item1.ToString();
                                secondWrapper = objectInputWrapper.Item2.ToString();
                                fixedCaretIndex = selectIndex + selectedTextBeforeTextFieldUpdate.Length;
                                makeNewParagraphSelected = true;

                                break;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(firstWrapper) && !string.IsNullOrEmpty(secondWrapper))
                    {
                        input = input.Remove(selectIndex - 1, 1).Insert(selectIndex - 1, selectedTextBeforeTextFieldUpdate.WrapWith(firstWrapper, secondWrapper));

                        delayedCaretIndex = fixedCaretIndex != -1 ? fixedCaretIndex : delayedCaretIndex;
                        delayedSelectionIndex = fixedSelectionIndex != -1 ? fixedSelectionIndex : delayedSelectionIndex;
                        selectNewParagraph = makeNewParagraphSelected;
                    }
                }
            }
            else if (caretIndex > 0)
            {
                string firstWrapper = string.Empty, secondWrapper = string.Empty;
                int fixedCaretIndex = -1;

                char foundChar = input[caretIndex - 1];
                if (generalInputWrappers.Contains(foundChar))
                {
                    foreach (char generalInputWrapper in generalInputWrappers)
                    {
                        if (generalInputWrapper == foundChar)
                        {
                            firstWrapper = generalInputWrapper.ToString();
                            secondWrapper = generalInputWrapper.ToString();

                            break;
                        }
                    }
                }
                else if (!dontAddCommandWrappers && commandInputWrappers.Select(c => c.Item1).Contains(foundChar))
                {
                    foreach ((char, char) commandInputWrapper in commandInputWrappers)
                    {
                        if (commandInputWrapper.Item1 == foundChar)
                        {
                            firstWrapper = $"{commandInputWrapper.Item1} ";
                            secondWrapper = $" {commandInputWrapper.Item2}";
                            fixedCaretIndex = caretIndex + 1;

                            break;
                        }
                    }
                }
                else if (!dontAddObjectWrappers && objectInputWrappers.Select(c => c.Item1).Contains(foundChar))
                {
                    foreach ((char, char) objectInputWrapper in objectInputWrappers)
                    {
                        if (objectInputWrapper.Item1 == foundChar)
                        {
                            firstWrapper = objectInputWrapper.Item1.ToString();
                            secondWrapper = objectInputWrapper.Item2.ToString();

                            break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(firstWrapper) && !string.IsNullOrEmpty(secondWrapper))
                {
                    input = input.Remove(caretIndex - 1, 1).Insert(caretIndex - 1, $"{firstWrapper}{secondWrapper}");

                    delayedCaretIndex = fixedCaretIndex != -1 ? fixedCaretIndex : delayedCaretIndex;
                }
            }
        }
        
        if (caretIndex > 0 && !pressedBackspace)
        {
            // Disable adding many wrappers in a row

            bool removeLastCharacter = false;

            char foundChar = input[caretIndex - 1];

            if (dontAddGeneralWrappers && caretIndexSurroundedWithGeneralWrappers && generalInputWrappers.Contains(foundChar)
                || dontAddCommandWrappers && caretIndexSurroundedWithCommandWrappers && commandInputWrappers.Select(c => c.Item1).Contains(foundChar)
                || dontAddObjectWrappers && caretIndexSurroundedWithObjectWrappers && objectInputWrappers.Select(c => c.Item1).Contains(foundChar))
            {
                removeLastCharacter = true;
            }

            if (removeLastCharacter)
            {
                input = input.Remove(caretIndex - 1, 1);
                delayedCaretIndex = caretIndex - 1;
            }
        }
    }

    HashSet<string> previousCommandInputSuggestions = new HashSet<string>();

    [RelatedTo(nameof(HandleInputField), RelationTargetType.Method)]
    private void GetCommandInputSuggestions(string input, out HashSet<string> commandInputSuggestions)
    {
        commandInputSuggestions = new HashSet<string>();

        if (!string.IsNullOrEmpty(input))
        {
            foreach (var alias in aliases)
            {
                string suggestion = alias.Key;

                if (suggestion.ToLower().StartsWith(input.ToLower()))
                {
                    commandInputSuggestions.Add(suggestion);
                }
            }
            
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
        Print($">> {input}", ConsoleOutputType.Information);

        string foundAliasContent = aliases.FirstOrDefault(a => a.Key == input).Value;
        if (!string.IsNullOrEmpty(foundAliasContent))
        {
            input = foundAliasContent;
        }

        string[] inputParts;
        if (input.ContainsAmountOf('\"') > 1 || input.ContainsAmountOf('\'') > 1)
        {
            inputParts = input.SplitAllNonWrapped(' ', generalInputWrappers);
        }
        else
        {
            inputParts = input.Split(' ');
        }

        foreach (var objectInputWrapper in objectInputWrappers)
        {
            if (inputParts.Contains(objectInputWrapper.Item1.ToString()) && inputParts.Contains(objectInputWrapper.Item2.ToString())
                || inputParts.Any(i => i.StartsWith(objectInputWrapper.Item1.ToString()) && !i.EndsWith(objectInputWrapper.Item2.ToString()))
                && inputParts.Any(i => i.EndsWith(objectInputWrapper.Item2.ToString()) && !i.StartsWith(objectInputWrapper.Item1.ToString())))
            {
                inputParts = inputParts.CombineSplittedWrappedParts((objectInputWrapper.Item1.ToString(), objectInputWrapper.Item2.ToString()));
            }
        }

        foreach (var commandInputWrapper in commandInputWrappers)
        {
            if (inputParts.Contains(commandInputWrapper.Item1.ToString()) && inputParts.Contains(commandInputWrapper.Item2.ToString()))
            {
                inputParts = inputParts.CombineSplittedCommandWrappedParts(new (char, char)[] { (commandInputWrapper.Item1, commandInputWrapper.Item2) });
            }
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
            case ConsoleOutputType.Highlight:
                return outputHighlightTextColor;
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
            timestamp = $"[{DateTime.Now.ToString("T")}] ";
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
