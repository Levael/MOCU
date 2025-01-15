using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#nullable enable


namespace DaemonsRelated
{
    public static class JsonHelper
    {
        public static T DeserializeJson<T>(string jsonString, JsonSerializerSettings? optionalSettings = null) where T : class
        {
            T? deserializedObject = JsonConvert.DeserializeObject<T>(jsonString, optionalSettings);
            return deserializedObject == null ? throw new Exception($"Couldn't deserialize the object {typeof(T)}") : deserializedObject;
        }

        public static string SerializeJson<T>(T obj, JsonSerializerSettings? optionalSettings = null)
        {
            string jsonString = JsonConvert.SerializeObject(obj, optionalSettings);
            return (String.IsNullOrEmpty(jsonString) || jsonString == "null") ? throw new Exception($"Couldn't serialize the object {typeof(T)}") : jsonString;
        }
    }
}

/*
public static class JsonHelper
    {
        public static T DeserializeJson<T>(string jsonString, JsonSerializerSettings? optionalSettings = null) where T : class
        {
            T? deserializedObject = JsonConvert.DeserializeObject<T>(jsonString, optionalSettings);
            return deserializedObject ?? throw new Exception($"Couldn't deserialize the object {typeof(T)}");
        }

        public static string SerializeJson<T>(T obj, JsonSerializerSettings? optionalSettings = null)
        {
            var settings = optionalSettings ?? new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            string jsonString = JsonConvert.SerializeObject(obj, settings);

            return
                (String.IsNullOrEmpty(jsonString) || jsonString == "null")
                ? throw new Exception($"Couldn't serialize the object {typeof(T)}")
                : jsonString;
        }

        public static string FormatJson(string json)
        {
            var parsedJson = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }
    }
*/