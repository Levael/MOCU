using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;

#nullable enable

namespace UnityDeamonsCommon
{
    public static class CommonUtilities
    {
        public static T? DeserializeJson<T>(string jsonString, JsonSerializerSettings? optionalSettings = null) where T : class
        {
            try
            {
                T? deserializedObject = JsonConvert.DeserializeObject<T>(jsonString, optionalSettings);
                if (deserializedObject == null)
                    throw new Exception();

                return deserializedObject;
            }
            catch
            {
                return null;
            }
        }

        public static string? SerializeJson<T>(T obj, JsonSerializerSettings? optionalSettings = null)
        {
            try
            {
                string jsonString = JsonConvert.SerializeObject(obj, optionalSettings);
                if (String.IsNullOrEmpty(jsonString) || jsonString == "null")
                    throw new Exception();

                return jsonString;
            }
            catch
            {
                return null;
            }
        }

        public static T ConvertJObjectToType<T>(JObject jObject)
        {
            if (jObject == null)
                throw new ArgumentNullException(nameof(jObject));

            return jObject.ToObject<T>() ?? throw new InvalidOperationException("Conversion resulted in null.");
        }

        public static string GetCallerMethodName()
        {
            StackTrace stackTrace = new StackTrace();
            StackFrame? frame = stackTrace.GetFrame(2); // 0 - текущий метод, 1 - метод GetCallerMethodName, 2 - вызывающий метод
            return frame?.GetMethod()?.Name ?? "Unknown";
        }
    }

}
