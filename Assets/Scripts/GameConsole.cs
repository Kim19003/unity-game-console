using Assets.Scripts.Attributes;
using Assets.Scripts.Extensions;
using Assets.Scripts.Other;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Color = UnityEngine.Color;
using GameConsoleConvert = Assets.Scripts.Other.GameConsoleConvert;
using TextEditor = UnityEngine.TextEditor;

public class GameConsole : MonoBehaviour
{
    [SerializeField] private Font font;
    [SerializeField] private KeyCode activateKey = KeyCode.Escape;
    [SerializeField] private int fontSize = 16;
    [SerializeField] private float outputBoxHeight = 200;
    [SerializeField] private int outputLabelHeight = 20;
    [SerializeField] private int inputTextFieldHeight = 20;

    [SerializeField] private Color defaultTextColor = Color.white;
    [SerializeField] private RichTextColor inputSuggestionTextColor = RichTextColor.Grey;
    [SerializeField] private RichTextColor outputExplanationTextColor = RichTextColor.Grey;
    [SerializeField] private RichTextColor outputWarningTextColor = RichTextColor.Yellow;
    [SerializeField] private RichTextColor outputErrorTextColor = RichTextColor.Red;
    
    [SerializeField] private Texture2D outputBoxBackgroundTexture;
    [SerializeField] private Color outputBoxBackgroundColor = Color.black;
    [SerializeField] private float outputBoxBackgroundColorAlpha = 0.5f;
    [SerializeField] private Texture2D outputBoxBorderBackgroundTexture;
    [SerializeField] private float outputBoxBorderSize = 0f;
    [SerializeField] private Color outputBoxBorderBackgroundColor = Color.gray;
    [SerializeField] private float outputBoxBorderBackgroundColorAlpha = 1f;

    [SerializeField] private Texture2D inputBoxBackgroundTexture;
    [SerializeField] private Color inputBoxBackgroundColor = Color.black;
    [SerializeField] private float inputBoxBackgroundColorAlpha = 0.6f;
    [SerializeField] private Texture2D inputBoxBorderBackgroundTexture;
    [SerializeField] private float inputBoxBorderSize = 0f;
    [SerializeField] private Color inputBoxBorderBackgroundColor = Color.gray;
    [SerializeField] private float inputBoxBorderBackgroundColorAlpha = 1f;

    #region Boring variables
    private readonly float canDeactivateConsoleAfterTime = 0.2f;
    private readonly float canRemoveWrappersWithBackspaceAfterTime = 0.2f;
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

    GameConsoleCommand<string> set_test_object_position;

    private string input = string.Empty;
    
    private bool activated = false;
    private bool justActivated = false;
    private bool canDeactivateConsole = false;

    readonly TimedUnityAction timedAction = new TimedUnityAction();
    Action currentTimedAction = null;
    float currentTimedActionInterval = 0;
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
                Print($"Id: <b>{command.CommandId}</b>", ConsoleOutputType.Explanation);
                Print($"Description: {command.CommandDescription}", ConsoleOutputType.Explanation);
                Print($"Format: {command.CommandFormat.WrapAlreadyWrappedPartsWithTags("i", '<', '>')}", ConsoleOutputType.Explanation);
                Print($"Usage example: {command.CommandExample}", ConsoleOutputType.Explanation);
            }
            else
            {
                PrintNotFoundError("Command", commandId);
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
                PrintNotFoundError("Scene", sceneName);
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
            Screen.fullScreen = !Screen.fullScreen;
            Print($"Fullscreen: {!Screen.fullScreen}", ConsoleOutputType.Explanation);
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
                PrintNotFoundError("Game object", gameObjectName);
            }
        };

        set_active = new GameConsoleCommand<string, string>(
            $"{nameof(set_active)}",
            "Activate or deactivate specific game object",
            $"{nameof(set_active)} <str: gameObjectName> <bool: isTrue>",
            $"{nameof(set_active)} \"Player\" false");
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
            $"{nameof(get_attribute_of)}",
            "Find a game object by name and get it's attribute value",
            $"{nameof(get_attribute_of)} <str: gameObjectName> <str: attributeName>",
            $"{nameof(get_attribute_of)} \"Player\" \"position\"");
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
            $"{nameof(set_attribute_of)}",
            "Find a game object by name and set it's attribute value",
            $"{nameof(set_attribute_of)} <str: gameObjectName> <str: attributeName> <obj: attributeValue>",
            $"{nameof(set_attribute_of)} \"Player\" \"position\" \"(1, 1, 0)\"");
        set_attribute_of.Action = (gameObjectName, attributeName, attributeValue) =>
        {
            GameObject gameObject = GameObject.Find(gameObjectName);

            if (gameObject != null)
            {
                SetterResult setterResult = gameObject.SetAttributeValue(attributeName, attributeValue);
                switch (setterResult)
                {
                    case SetterResult.Successful:
                        Print($"{gameObject.transform.name}'s {attributeName} is now {gameObject.GetAttributeValue(attributeName)}", ConsoleOutputType.Explanation);
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
            $"{nameof(get_admitted_attribute_names)}",
            "Get the GameObject type's admitted attribute names",
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

        set_test_object_position = new GameConsoleCommand<string>(
            $"{nameof(set_test_object_position)}",
            "Set test object position",
            $"{nameof(set_test_object_position)} <v3: position>",
            $"{nameof(set_test_object_position)} \"(1, 1, 0)\"");
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

            set_test_object_position,
        });
        #endregion
    }

    private void Update()
    {
        if (Input.GetKeyDown(activateKey))
        {
            if (!activated)
            {
                activated = true;
                justActivated = true;
                StartCoroutine(CanDeactivateConsoleAfter(canDeactivateConsoleAfterTime));
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
            case KeyCode keyCode when keyCode == activateKey:
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
            if (canRemoveWrappersWithBackspace)
            {
                int caretIndex = textEditor.GetCaretIndex();

                if (caretIndex > 0)
                {
                    if (input.GetCharAt(caretIndex - 1) == '\"' && input.GetCharAt(caretIndex) == '\"'
                        || input.GetCharAt(caretIndex - 1) == '\'' && input.GetCharAt(caretIndex) == '\'')
                    {
                        input = input.Remove(caretIndex - 1, 2);
                        textEditor.SetCaretIndex(caretIndex - 1);
                        Event.current.keyCode = KeyCode.None;
                    }
                }
            }

            StartCoroutine(CanRemoveWrappersWithBackspaceAfter(canRemoveWrappersWithBackspaceAfterTime));
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
            if (!dontAddWrappers)
            {
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
                else
                {
                    int caretIndex = textEditor.GetCaretIndex();

                    if (caretIndex > 0)
                    {
                        char foundChar = input.TryGetWrapperAt(caretIndex - 1, new char[] { '"', '\'' });

                        if (foundChar != '\0')
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

    [RelatedTo(nameof(HandleInputField), RelationTargetType.Method)]
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
        Print($"Incorrect usage of the command \"{commandId}\" (use \"help_of <b>{commandId}</b>\" to get details about the command)", ConsoleOutputType.Error);
    }

    #region IEnumerators
    /// <summary>
    /// Used to set small delay to do a thing (since OnGui() is called many times per frame)
    /// </summary>
    private IEnumerator CanRemoveWrappersWithBackspaceAfter(float seconds)
    {
        canRemoveWrappersWithBackspace = false;

        yield return new WaitForSeconds(seconds);

        canRemoveWrappersWithBackspace = true;
    }

    /// <summary>
    /// Used to set small delay to do a thing (since OnGui() is called many times per frame)
    /// </summary>
    private IEnumerator DontShowSuggestionsFor(float seconds)
    {
        showSuggestions = false;

        yield return new WaitForSeconds(seconds);

        showSuggestions = true;
    }

    /// <summary>
    /// Used to set small delay to do a thing (since OnGui() is called many times per frame)
    /// </summary>
    private IEnumerator CanScrollSuggestionsAfter(float seconds)
    {
        canScrollSuggestions = false;

        yield return new WaitForSeconds(seconds);

        canScrollSuggestions = true;
    }

    /// <summary>
    /// Used to set small delay to do a thing (since OnGui() is called many times per frame)
    /// </summary>
    private IEnumerator CanCompleteSuggestionAfter(float seconds)
    {
        canCompleteSuggestion = false;

        yield return new WaitForSeconds(seconds);

        canCompleteSuggestion = true;
    }

    /// <summary>
    /// Used to set small delay to do a thing (since OnGui() is called many times per frame)
    /// </summary>
    private IEnumerator CanDeactivateConsoleAfter(float seconds)
    {
        canDeactivateConsole = false;

        yield return new WaitForSeconds(seconds);

        canDeactivateConsole = true;
    }
    #endregion

    #region Public methods
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
    #endregion
}
