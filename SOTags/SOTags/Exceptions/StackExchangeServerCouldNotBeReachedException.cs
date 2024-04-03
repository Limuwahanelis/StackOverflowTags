namespace SOTags.Exceptions
{
    public class StackExchangeServerCouldNotBeReachedException:Exception
    {
        public string OperationMessage { get; set; } = "";
        public string StackExchangeSetverMessage { get; set; } = "";
    }
}
