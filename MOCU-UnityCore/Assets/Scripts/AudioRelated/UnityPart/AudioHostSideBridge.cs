﻿using InterprocessCommunication;
using DaemonsRelated;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;


namespace AudioModule
{
    public class AudioHostSideBridge : AudioDaemon_API, IDaemonHostBridge
    {
        private IInterprocessCommunicator _communicator;

        public AudioHostSideBridge(IInterprocessCommunicator communicator)
        {
            _communicator = communicator;

            _communicator.MessageReceived       += message => UnityEngine.Debug.Log($"Got message from daemon: {message}");
            _communicator.MessageSent           += message => UnityEngine.Debug.Log($"Sent message to daemon: {message}");
            _communicator.ConnectionEstablished += message => UnityEngine.Debug.Log($"Connection established: {message}");
            _communicator.ConnectionBroked      += message => UnityEngine.Debug.Log($"Connection broked: {message}");
        }

        public event Action<IEnumerable<AudioDeviceData>> DevicesDataChanged;
        public event Action<IEnumerable<AudioClipData>> ClipsDataChanged;
        public event Action<IEnumerable<AudioIntercomData>> IntercomStatesChanged;
        public event Action<IEnumerable<DaemonErrorReport>> ErrorsOccurred;

        // for daemons debug purpose only (todo: maybe refactor later)
        public event Action<string> MessageReceived;
        public event Action<string> MessageSent;

        public void PlayClips(IEnumerable<PlayAudioClipCommand> playCommandsData)
        {
            throw new NotImplementedException();
        }

        public void StartCommunication()
        {
            // method is async, but there is no need to use 'await'
            _communicator.Start();
        }

        public void TestMethod()
        {
            try
            {
                var testObj = new AudioDataTransferObject()
                {
                    CustomMessage = "testMessage",
                    //DaemonErrorReports = new List<DaemonErrorReport>() { new DaemonErrorReport() { message = "test inner message"} },
                    PlayClipCommands = new List<PlayAudioClipCommand>() { 
                        new PlayAudioClipCommand() { ClipData = new AudioClipData() { name = AudioClipName.PingDevice } },
                        new PlayAudioClipCommand() { ClipData = new AudioClipData() { name = AudioClipName.CorrectAnswer } }
                    },
                    DoTerminateTheDaemon = false,
                };

                _communicator.SendMessage(JsonHelper.SerializeJson(testObj));
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Error in 'AudioHostSideBridge.TestMethod': {ex}");
            }
        }

        public void UpdateClipsData(IEnumerable<AudioClipData> clipsData)
        {
            throw new NotImplementedException();
        }

        public void UpdateDevicesData(IEnumerable<AudioDeviceData> devicesData)
        {
            throw new NotImplementedException();
        }

        public void UpdateIntercomStates(IEnumerable<AudioIntercomData> intercomsData)
        {
            throw new NotImplementedException();
        }
    }
}