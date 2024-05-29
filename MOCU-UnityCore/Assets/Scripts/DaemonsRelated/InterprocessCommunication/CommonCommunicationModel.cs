#nullable enable

namespace DaemonsNamespace.InterprocessCommunication
{
    public abstract class GeneralCommandToServer
    {
        public string Command { get; set; }
    }

    public class ResponseFromServer
    {
        public string Command { get; set; } = "ResponseFromServer";
        public string ReceivedCommand { get; set; }
        public bool HasError { get; set; }

        public string? ErrorMessage { get; set; }
        public object? ExtraData { get; set; }

        public ResponseFromServer(string receivedCommand, bool hasError, string? errorMessage = null, object? extraData = null)
        {
            ReceivedCommand = receivedCommand;
            HasError = hasError;
            ErrorMessage = errorMessage;
            ExtraData = extraData;
        }
    }
}