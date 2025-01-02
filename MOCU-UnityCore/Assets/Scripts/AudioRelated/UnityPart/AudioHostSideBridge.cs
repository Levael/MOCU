using InterprocessCommunication;
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
                /*var testObj = new AudioDataTransferObject()
                {
                    PlayClipCommands = new List<PlayAudioClipCommand>() {
                        new PlayAudioClipCommand() {
                            InterruptPlayingClips = true,
                            OutputDeviceId = Guid.Parse("515a3dc0-ca38-4fb9-aa8b-6d268cbd94ac"),
                            ClipData = new AudioClipData() {
                                name = AudioClipName.PingDevice
                            }
                        }
                    }
                };*/

                var testObj = new AudioDataTransferObject()
                {
                    IntercomCommands = new List<AudioIntercomData>() {
                        new AudioIntercomData() {
                            isOn = true,
                            id = new Guid("ed3c3975-74c7-433a-b9ae-96cfd0e8a000"),
                            fromDevices = new List<Guid> { Guid.Parse("ed3c3975-74c7-433a-b9ae-96cfd0e8a426") },
                            toDevices = new List<Guid> { Guid.Parse("f6e626d3-fc5c-44ab-a993-81e2cbaccdeb") },
                        }
                    }
                };

                _communicator.SendMessage(JsonHelper.SerializeJson(testObj));
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Error in 'AudioHostSideBridge.TestMethod': {ex}");
            }
        }

        public void TestMethod2()
        {
            try
            {
                /*var testObj = new AudioDataTransferObject()
                {
                    ClipChanges = new List<AudioClipData>() {
                        new AudioClipData() {
                            name = AudioClipName.PingDevice,
                            volume = 100,
                            fullFilePath = @"C:\Users\Levael\GitHub\MOCU\MOCU-UnityCore\Assets\StreamingAssets\Audio\test.mp3"
                        },
                        new AudioClipData() {
                            name = AudioClipName.CorrectAnswer,
                            volume = 100,
                            fullFilePath = @"C:\Users\Levael\GitHub\MOCU\MOCU-UnityCore\Assets\StreamingAssets\Audio\audioTestSample2.mp3"
                        },
                    }
                };*/

                var testObj = new AudioDataTransferObject()
                {
                    IntercomCommands = new List<AudioIntercomData>() {
                        new AudioIntercomData() {
                            isOn = false,
                            id = new Guid("ed3c3975-74c7-433a-b9ae-96cfd0e8a000")
                        }
                    }
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