using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Reflection;

using UnityDaemonsCommon;


public class ConfigHandler : MonoBehaviour
{
    public ProtocolConfig defaultConfig;

    private string _defaultConfigFilePath;

    void Awake()
    {
        _defaultConfigFilePath = Path.Combine(Application.streamingAssetsPath, "Config/config.json");
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



    private ProtocolConfig? ReadConfig(string filePath)
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

    private void WriteConfig(string filePath, ProtocolConfig config)
    {
        try
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,               // Setting indents for improved readability
                NullValueHandling = NullValueHandling.Include   // Including fields with null values in JSON
            };

            string jsonString = CommonUtilities.SerializeJson(config, settings);
            if (String.IsNullOrEmpty(jsonString))
                return;

            File.WriteAllText(filePath, jsonString);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error writing or serializing the file: {ex.Message}");
        }
    }

    public void UpdateSubConfig<T>(T newValue)
    {
        FieldInfo fieldToUpdate = null;

        foreach (FieldInfo field in typeof(ProtocolConfig).GetFields())
        {
            if (field.FieldType == typeof(T))
            {
                fieldToUpdate = field;
                break;
            }
        }

        if (fieldToUpdate != null)
            fieldToUpdate.SetValue(defaultConfig, newValue);
        else
            UnityEngine.Debug.LogError("No matching field found for type " + typeof(T).Name);
    }

}
