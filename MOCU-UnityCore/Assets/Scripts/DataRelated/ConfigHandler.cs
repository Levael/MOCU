using CommonUtilitiesNamespace;
using System;
using System.IO;
using UnityEngine;

public class ConfigHandler : MonoBehaviour
{
    public ProtocolConfig defaultConfig;

    private string _defaultConfigFilePath;

    void Awake()
    {
        _defaultConfigFilePath = Path.Combine(Application.dataPath, "Scripts/DataRelated/Config.json");
        defaultConfig = ReadConfig(_defaultConfigFilePath);
        
        //UnityEngine.Debug.Log($"in ConfigHandler Awake after '='");
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }


    public static ProtocolConfig ReadConfig(string filePath)
    {
        try
        {
            string jsonString = File.ReadAllText(filePath);
            return CommonUtilities.DeserializeJson<ProtocolConfig>(jsonString);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.Log($"Error reading or deserializing the file: {ex.Message}");
            return null;
        }
    }
}
