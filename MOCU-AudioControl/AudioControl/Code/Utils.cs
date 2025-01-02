using System.Text.RegularExpressions;


namespace AudioModule.Daemon
{
    public static class Utils
    {
        public static Guid ExtractGuid(string deviceId)
        {
            var match = Regex.Match(deviceId, @"\{[0-9a-fA-F\-]{36}\}");

            if (match.Success)
            {
                string guidString = match.Value.Trim('{', '}');

                if (Guid.TryParse(guidString, out var deviceGuid))
                    return deviceGuid;
            }

            throw new Exception($"Coudn't exctract Guid from deviceId: {deviceId}");
        }
    }
}