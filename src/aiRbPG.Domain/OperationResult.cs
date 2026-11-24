namespace aiRbPG.Domain
{
    public sealed class OperationResult<T>
    {
        private OperationResult(T value, string error)
        {
            Value = value;
            Error = error;
        }

        public T Value { get; private set; }
        public string Error { get; private set; }
        public bool Succeeded { get { return Error == null; } }

        public static OperationResult<T> Failure(string message)
        {
            return new OperationResult<T>(default(T), message);
        }

        public static implicit operator OperationResult<T>(T value)
        {
            return new OperationResult<T>(value, null);
        }
    }
}
