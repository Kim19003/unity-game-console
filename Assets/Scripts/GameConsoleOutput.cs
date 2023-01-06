public enum ConsoleOutputType
{
    Information,
    Explanation,
    Warning,
    Error,
    Custom
}

public class GameConsoleOutput
{
    public string Text { get { return text; } }
    private readonly string text;
    public ConsoleOutputType OutputType { get { return outputType; } }
    private readonly ConsoleOutputType outputType;

    public GameConsoleOutput(string text, ConsoleOutputType outputType = ConsoleOutputType.Information)
    {
        this.text = text;
        this.outputType = outputType;
    }
}