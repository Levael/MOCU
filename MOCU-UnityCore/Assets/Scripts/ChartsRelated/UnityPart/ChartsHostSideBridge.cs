using System;
using UnityEngine;

using DaemonsRelated;
using InterprocessCommunication;
using MoogModule;
using System.Security.Cryptography;


namespace ChartsModule
{
    public class ChartsHostSideBridge : ChartsDaemon_API, IDaemonHostBridge
    {
        private IInterprocessCommunicator _communicator;

        public ChartsHostSideBridge(IInterprocessCommunicator communicator)
        {
            _communicator = communicator;
            _communicator.MessageReceived += message => HandleIncomingMessage(message);
        }

        public void Test()
        {
            var chartsDataTransferObject = new ChartsDataTransferObject { };
            var json = JsonHelper.SerializeJson(chartsDataTransferObject);
            _communicator.SendMessage(json);
        }

        // .........

        private void HandleIncomingMessage(string message)
        {
            Debug.Log($"Message from daemon (charts): {message}");
        }
    }
}