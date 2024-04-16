using CommonUtilitiesNamespace;
using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;


public class ConfigHandler : MonoBehaviour
{
    public ProtocolConfig defaultConfig;

    private string _defaultConfigFilePath;

    void Awake()
    {
        _defaultConfigFilePath = Path.Combine(Application.dataPath, "Scripts/DataRelated/Config.json");
        defaultConfig = ReadConfig(filePath: _defaultConfigFilePath);

        UnityEngine.Debug.Log(defaultConfig.UnserializableData.Count);
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



    public static ProtocolConfig? ReadConfig(string filePath)
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

    public static void WriteConfig(string filePath, ProtocolConfig config)
    {
        try
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,               // Setting indents for improved readability
                NullValueHandling = NullValueHandling.Include   // Including fields with null values in JSON
            };

            string jsonString = CommonUtilities.SerializeJson(config, settings);
            File.WriteAllText(filePath, jsonString);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error writing or serializing the file: {ex.Message}");
        }
    }
}
