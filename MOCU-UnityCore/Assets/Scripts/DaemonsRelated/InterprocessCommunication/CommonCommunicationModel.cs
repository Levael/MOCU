#nullable enable

using Newtonsoft.Json;
using System;
using UnityDaemonsCommon;


namespace DaemonsNamespace.InterprocessCommunication
{
    public class UnifiedDataTransferObject
    {
        [JsonProperty]
        public readonly string name;

        [JsonProperty]
        private readonly string? _extraDataJson;

        [JsonConverter(typeof(TypeNameHandlingConverter))]
        [JsonProperty]
        private readonly Type? _extraDataType;

        public UnifiedDataTransferObject(string name, object? extraData = null)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("Name cannot be null or empty: ", nameof(name));

            this.name = name;

            if (extraData != null)
            {
                _extraDataJson = CommonUtilities.SerializeJson(extraData);
                _extraDataType = extraData?.GetType();
            }
            else
            {
                _extraDataJson = null;
                _extraDataType = null;
            }
        }

        public T? GetExtraData<T>() where T : class
        {
            if (String.IsNullOrEmpty(_extraDataJson) || _extraDataType == null)
            {
                Console.WriteLine($"String.IsNullOrEmpty(_extraDataJson) || _extraDataType == null  --> returned null: _extraDataJson: {_extraDataJson}; _extraDataType: {_extraDataType}");
                return null;
            }
                

            if (_extraDataType != typeof(T))
                throw new InvalidOperationException($"GetExtraData: Expected type {_extraDataType.FullName} but got type {typeof(T).FullName}");

            try
            {
                // temp debug
                Console.WriteLine($"CommonUtilities.DeserializeJson<T>(_extraDataJson): {CommonUtilities.DeserializeJson<T>(_extraDataJson)}");
                return CommonUtilities.DeserializeJson<T>(_extraDataJson);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to deserialize extra data.", ex);
            }
        }
    }

    public class UnifiedCommandFromClient : UnifiedDataTransferObject
    {
        public UnifiedCommandFromClient(string name, object? extraData = null) : base(name, extraData) { }
    }

    public class UnifiedResponseFromServer : UnifiedDataTransferObject
    {
        [JsonProperty]  public readonly bool? errorOccurred;
        [JsonProperty]  public readonly bool? errorIsFatal;
        [JsonProperty]  public readonly string? errorMessage;


        public UnifiedResponseFromServer (string name, object? extraData = null, bool? errorOccurred = null, bool? errorIsFatal = null, string? errorMessage = null) : base (name, extraData)
        {
            this.errorOccurred = errorOccurred;
            this.errorIsFatal = errorIsFatal;
            this.errorMessage = errorMessage;

            if (errorOccurred != null && (errorIsFatal == null || errorMessage == null))
                throw new ArgumentException("If error occurred, you have to specify is it fatal and provide its decription.");
        }
    }
}