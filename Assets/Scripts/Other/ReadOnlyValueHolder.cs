namespace Assets.Scripts.Other
{
    public class ReadOnlyValueHolder<T>
    {
        public T Value { get { return value; } }
        private readonly T value;

        public ReadOnlyValueHolder(T value)
        {
            this.value = value;
        }
    }
}
