using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System;
using System.Threading;
//using UnityEngine;

#nullable enable

namespace CommonUtilitiesNamespace
{
    /// <summary>
    /// Provides static utility methods for working with JSON format.
    /// </summary>
    public static class CommonUtilities
    {
        public static T DeserializeJson<T>(string jsonString, JsonSerializerSettings? optionalSettings = null)
        {
            try
            {
                T deserializedObject = JsonConvert.DeserializeObject<T>(jsonString, optionalSettings);
                return deserializedObject;
            }
            catch
            {
                return default;
            }
        }

        public static string? SerializeJson<T>(T obj, JsonSerializerSettings? optionalSettings = null)
        {
            try
            {
                string jsonString = JsonConvert.SerializeObject(obj, optionalSettings);
                if (String.IsNullOrEmpty(jsonString) || jsonString == "null")
                    throw new Exception("custom null");

                return jsonString;
            }
            catch (Exception ex)
            {
                //UnityEngine.Debug.LogWarning($"SerializeJson return 'null' with error: {ex}");
                return null;
            }
        }

        public static string GetSerializedObjectType(string jsonString)
        {
            JObject jsonObject = JsonConvert.DeserializeObject<JObject>(jsonString);
            string objectType = jsonObject["Command"].ToString();
            return objectType;
        }

        public static T ConvertJObjectToType<T>(JObject jObject)
        {
            if (jObject == null)
                throw new ArgumentNullException(nameof(jObject));

            return jObject.ToObject<T>() ?? throw new InvalidOperationException("Conversion resulted in null.");
        }
    }



    /// <summary>
    /// Represents a thread-safe, observable concurrent queue.
    /// Allows enqueueing items from any thread and processes them in a separate thread.
    /// Enables additional processing through an event mechanism.
    /// </summary>
    /// <typeparam name="T">The type of elements in the queue.</typeparam>
    public class ObservableConcurrentQueue<T> : ConcurrentQueue<T>
    {
        // Delegate for the primary item processing function.
        public delegate void ProcessItemDelegate(T item);
        private ProcessItemDelegate processItem;

        // Event for additional processing (like logging, debugging etc).
        public event Action<T> AdditionalProcessing;

        private SemaphoreSlim   signal = new SemaphoreSlim(0);
        private Thread          workerThread;
        private volatile bool   isRunning = true;

        /// <param name="processItem">The main processing function for the queue items</param>
        public ObservableConcurrentQueue(ProcessItemDelegate processItem)
        {
            this.processItem = processItem ?? throw new ArgumentNullException(nameof(processItem));
            workerThread = new Thread(Worker);
            workerThread.Start();
        }


        public new void Enqueue(T item)
        {
            base.Enqueue(item);
            signal.Release();   // Signal the worker thread for processing (is ok even if called a few times before actual processing, will just do a few empty "while" cycles)
        }

        /// <summary>
        /// The worker method that runs in a separate thread to process queue items.
        /// </summary>
        private void Worker()
        {
            while (isRunning)
            {
                signal.Wait();                          // Wait for an item to be enqueued

                while (this.TryDequeue(out T item))
                {
                    processItem(item);                  // Main processing of the item
                    AdditionalProcessing?.Invoke(item); // Additional processing (e.g., logging)
                }
            }
        }

        /// <summary>
        /// Stops the processing thread, but ensures the worker thread finishes all tasks
        /// </summary>
        public void Stop()
        {
            isRunning = false;
            signal.Release();
        }

        /// <summary>
        /// Disposes the semaphore.
        /// </summary>
        public void Dispose()
        {
            signal.Dispose();
        }
    }


}
