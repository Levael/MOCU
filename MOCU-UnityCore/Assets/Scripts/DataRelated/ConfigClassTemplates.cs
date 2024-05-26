using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioControl; // custom class
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class ProtocolConfig
{
    public AudioDevicesInfo AudioConfig;


    // For all data that doesn't match 'ProtocolConfig' format
    [JsonExtensionData]
    public Dictionary<string, JToken> UnserializableData = new();
}
