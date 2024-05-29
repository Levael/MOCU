using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;


namespace DaemonsNamespace.InterprocessCommunication
{
    public static class CommonClientServerMethods
    {

        public static string? GetSerializedObjectType(string jsonString)
        {
            try
            {
                var jsonObject = JsonConvert.DeserializeObject<JObject>(jsonString);

                if (jsonObject == null)
                    throw new Exception();

                var objectType = jsonObject["Command"].ToString();
                return objectType;
            }
            catch
            {
                return null;
            }
        }

    }
}
