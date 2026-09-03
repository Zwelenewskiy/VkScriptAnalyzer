namespace Entities.Emulator
{
    public class CalculateResult
    {
        private object value;
        public DataType DataType { get; set; }

        public CalculateResult(object val, DataType type)
        {
            value = val;
            DataType = type;
        }

        public object GetResult()
        {
            return value;
        }
    }
}
