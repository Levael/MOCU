using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Reflection;

using UnityDeamonsCommon;


public class ConfigHandler : MonoBehaviour
{
    public ProtocolConfig defaultConfig;

    private string _defaultConfigFilePath;

    void Awake()
    {
        _defaultConfigFilePath = Path.Combine(Application.dataPath, "Scripts/DataRelated/Config.json");
        defaultConfig = ReadConfig(filePath: _defaultConfigFilePath);
    }

    void Start()
    {
    }

    void Update()
    {
    }

    void OnApplicationQuit()
    {
        WriteConfig(filePath: _defaultConfigFilePath, config: defaultConfig);
    }



    public ProtocolConfig? ReadConfig(string filePath)
    {
        try
        {

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Populate,   // 'Populate' ensures missing JSON properties are initialized to default values
                NullValueHandling = NullValueHandling.Include           // Including fields with null values in JSON
            };

            string jsonString = File.ReadAllText(filePath);
            return CommonUtilities.DeserializeJson<ProtocolConfig>(jsonString, settings);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error reading or deserializing the file: {ex.Message}");
            return null;
        }
    }

    public void WriteConfig(string filePath, ProtocolConfig config)
    {
        try
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,               // Setting indents for improved readability
                NullValueHandling = NullValueHandling.Include   // Including fields with null values in JSON
            };

            string jsonString = CommonUtilities.SerializeJson(config, settings);
            // add check for "null"
            File.WriteAllText(filePath, jsonString);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error writing or serializing the file: {ex.Message}");
        }
    }

    public void UpdateSubConfig<T>(T newValue)
    {
        PropertyInfo propertyToUpdate = null;

        foreach (PropertyInfo property in typeof(ProtocolConfig).GetProperties())
        {
            if (property.PropertyType == typeof(T))
            {
                propertyToUpdate = property;
                break;
            }
        }

        if (propertyToUpdate != null)
        {
            propertyToUpdate.SetValue(defaultConfig, newValue);
        }
        else
        {
            UnityEngine.Debug.LogError("No matching property found for type " + typeof(T).Name);
        }
    }
}
