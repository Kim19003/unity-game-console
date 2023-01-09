namespace Assets.Scripts.Other
{
    public class GameConsoleType<T>
    {
        public T Value { get; }

        public GameConsoleType(T value)
        {
            Value = value;
        }
    }
}
