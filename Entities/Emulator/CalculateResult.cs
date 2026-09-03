namespace Entities.Emulator
{
    public class CalculateResult
    {
        private readonly object _value;
        public DataType DataType { get; set; }
        public bool IsSuccess { get; }
        public string ErrorMessage { get; }

        public CalculateResult(object val, DataType type)
        {
            _value = val;
            DataType = type;
            IsSuccess = true;
            ErrorMessage = null;
        }

        public CalculateResult(string errorMessage)
        {
            _value = null;
            IsSuccess = false;
            ErrorMessage = errorMessage;
        }

        public object GetResult()
        {
            return _value;
        }
    }
}
