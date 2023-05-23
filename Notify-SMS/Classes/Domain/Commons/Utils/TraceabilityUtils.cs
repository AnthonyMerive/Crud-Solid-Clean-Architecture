namespace Classes.Domain.Commons.Utils
{
    public static class TraceabilityUtils
    {
        public const string OPERATION_IN = "{0}/request";
        public const string OPERATION_OUT = "{0}/response";

        public static string GetOperationIn(string operation)
        {
            return string.Format(OPERATION_IN, operation);
        }

        public static string GetOperationOut(string operation)
        {
            return string.Format(OPERATION_OUT, operation);
        }
    }
}
