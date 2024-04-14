using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using AudioControl; // custom class
using CommonUtilitiesNamespace;
using InterprocessCommunication;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;

public class AudioHandler : MonoBehaviour
{
    public StateTracker stateTracker;
    public AudioDevicesInfo audioDevicesInfo;

    private NamedPipeClient namedPipeClient;
    private Process audioControlProcess;
    private string namedPipeName;

    private UiHandler _uiHandler;
    private ConfigHandler _configHandler;
    private ExperimentTabHandler _experimentTabHandler;
    private InputLogic _inputLogic;

    public List<string> inputAudioDevices;
    public List<string> outputAudioDevices;

    private Dictionary<string, string> partlyOptimizedJsonCommands; // in theory, should reduce the delay when sending commands. todo: review later
    private Dictionary<string, string> uiReferenceToDevicesInfoMap;
    private Dictionary<string, (string? subState, Action<ResponseFromServer?> action)> CommandsToExecuteAccordingToServerResponse;  // serverResponse -> updState -> executeNextCommand


    void Awake()
    {
        namedPipeName = "AudioPipe";

        _uiHandler = GetComponent<UiHandler>();
        _configHandler = GetComponent<ConfigHandler>();
        _experimentTabHandler = GetComponent<ExperimentTabHandler>();
        _inputLogic = GetComponent<InputLogic>();

        stateTracker = new StateTracker(new[] { "StartAudioProcess", "StartNamedPipeConnection", "SetConfigs", "RequestAudioDevices", "SendAudioDevices"});

        inputAudioDevices = new();
        outputAudioDevices = new();

        partlyOptimizedJsonCommands = new() {
            { "StartIntercomStream_ResearcherToParticipant_Command", CommonUtilities.SerializeJson(new StartIntercomStream_ResearcherToParticipant_Command()) },
            { "StartIntercomStream_ParticipantToResearcher_Command", CommonUtilities.SerializeJson(new StartIntercomStream_ParticipantToResearcher_Command()) },
            { "StopIntercomStream_ResearcherToParticipant_Command", CommonUtilities.SerializeJson(new StopIntercomStream_ResearcherToParticipant_Command()) },
            { "StopIntercomStream_ParticipantToResearcher_Command", CommonUtilities.SerializeJson(new StopIntercomStream_ParticipantToResearcher_Command()) },
        };

        uiReferenceToDevicesInfoMap = new()
        {
            { "settings-device-box-speaker-researcher",      "audioOutputDeviceVolume_Researcher" },
            { "settings-device-box-speaker-participant",     "audioOutputDeviceVolume_Participant" },
            { "settings-device-box-microphone-researcher",   "audioInputDeviceVolume_Researcher" },
            { "settings-device-box-microphone-participant",  "audioInputDeviceVolume_Participant" },
        };

        CommandsToExecuteAccordingToServerResponse = new()
        {
            //{ "StartAudioControlProcess_Command",           (subState: "StartAudioProcess",         action: TryConnectToServer) },
            { "TryConnectToServer_Command",                 (subState: "StartNamedPipeConnection",  action: SendConfigurationDetails) },
            { "SendConfigs_Command",                        (subState: "SetConfigs",                action: RequestAudioDevices) },
            { "GetAudioDevices_Command",                    (subState: "RequestAudioDevices",       action: SendAudioDevices) },
            { "UpdateDevicesParameters_Command",            (subState: "SendAudioDevices",          action: UpdateConfig) }
        };

        // Reading from config Audio Devices Data
        audioDevicesInfo = _configHandler.defaultConfig.AudioConfig;


        // Event listeners for intercom
        _inputLogic.startOutgoingIntercomStream += () => {
            namedPipeClient.SendCommandAsync(partlyOptimizedJsonCommands["StartIntercomStream_ResearcherToParticipant_Command"]);
        };

        _inputLogic.stopOutgoingIntercomStream += () => {
            namedPipeClient.SendCommandAsync(partlyOptimizedJsonCommands["StopIntercomStream_ResearcherToParticipant_Command"]);
        };

        _inputLogic.startIncomingIntercomStream += () => {
            namedPipeClient.SendCommandAsync(partlyOptimizedJsonCommands["StartIntercomStream_ParticipantToResearcher_Command"]);
        };

        _inputLogic.stopIncomingIntercomStream += () => {
            namedPipeClient.SendCommandAsync(partlyOptimizedJsonCommands["StopIntercomStream_ParticipantToResearcher_Command"]);
        };
    }

    void Start()
    {
        StartAudioControlProcess();
        TryConnectToServer();
    }

    // todo: make each loop a separate method
    void Update()
    {
        if (namedPipeClient == null) return;    // in case it is still not ready

        // Checking input queue of namedPipeClient to see is there anything new
        while (namedPipeClient.inputMessagesQueue.TryDequeue(out string message))
        {
            try
            {
                // currently no check for "is it realy 'ResponseFromServer'", because this code only gets responses. maybe add later (todo)
                var obj = CommonUtilities.DeserializeJson<ResponseFromServer>(message);
                var subStateName = CommandsToExecuteAccordingToServerResponse[obj.ReceivedCommand].subState;
                var funcToBeExecuted = CommandsToExecuteAccordingToServerResponse[obj.ReceivedCommand].action;

                if (obj.HasError)
                {
                    stateTracker.UpdateSubState(subStateName, false);
                    _experimentTabHandler.PrintToWarnings($"Failed to '{subStateName}'");

                    if (subStateName == "StartNamedPipeConnection" || subStateName  == "StartNamedPipeConnection")
                    {
                        CloseConnection($"AudioHandler/{subStateName}");
                    }

                    // pass this message, go to the next (if there is any)
                    continue;
                }

                // In case everything is fine
                stateTracker.UpdateSubState(subStateName, true);
                funcToBeExecuted.Invoke(obj);
            }
            catch
            {
                //stateTracker.UpdateSubState("StartAudioProcess", false);    // not realy it, but need to be something
                _experimentTabHandler.PrintToWarnings($"Total fail while trying read incoming message from server");
            }
        }


        // Checking inner queue of namedPipeClient to see is there anything new
        while (namedPipeClient.innerMessagesQueue.TryDequeue(out (string messageText, InnerMessageType messageType) message))
        {
            if (message.messageType == InnerMessageType.Info)
            {
                //_uiHandler.PrintToInfo(message.messageText);
            }

            if (message.messageType == InnerMessageType.Error)
            {
                CloseConnection(message.messageText);
                stateTracker.UpdateSubState("StartNamedPipeConnection", false);
            }
        }
    }

    void OnDestroy()
    {
        try { namedPipeClient?.Destroy();   } catch { }
        try { audioControlProcess?.Kill();  } catch { }
    }

    private void CloseConnection(string message)
    {
        _experimentTabHandler.PrintToWarnings(message);
        OnDestroy();
    }

    private async void TryConnectToServer()
    {
        try
        {
            namedPipeClient = new NamedPipeClient(namedPipeName);
            bool result = await namedPipeClient.StartAsync();
            if (result == false) throw new Exception();

            namedPipeClient.inputMessagesQueue.Enqueue(CommonUtilities.SerializeJson(new ResponseFromServer(receivedCommand: "TryConnectToServer_Command", hasError: false)));
            // not a real command (not a class in 'AudioControlCommunicationModels' but more for homogeneity of 'connection to server steps')
        }
        catch
        {
            namedPipeClient.inputMessagesQueue.Enqueue(CommonUtilities.SerializeJson(new ResponseFromServer(receivedCommand: "TryConnectToServer_Command", hasError: true)));
            // not a real command (not a class in 'AudioControlCommunicationModels' but more for homogeneity of 'connection to server steps')
        }
    }

    private void StartAudioControlProcess()
    {
        try
        {
            string relativeExternalAppPath = @"AudioControl.exe";
            string fullExternalAppPath = Path.Combine(Application.streamingAssetsPath, relativeExternalAppPath);
            bool isProcessHidden = false;

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = fullExternalAppPath,
                Arguments = $"{Process.GetCurrentProcess().Id} {namedPipeName} {isProcessHidden}", // takes string where spaces separate arguments
                UseShellExecute = !isProcessHidden,
                RedirectStandardOutput = false,
                CreateNoWindow = isProcessHidden
            };

            audioControlProcess = new Process() { StartInfo = startInfo };
            audioControlProcess.Start();

            stateTracker.UpdateSubState("StartAudioProcess", true);
        }
        catch
        {
            stateTracker.UpdateSubState("StartAudioProcess", false);
            _experimentTabHandler.PrintToWarnings("Failed to 'StartAudioControlProcess'");
        }
    }


    public void SendConfigurationDetails(ResponseFromServer response)
    {
        namedPipeClient.SendCommandAsync(CommonUtilities.SerializeJson(new SendConfigs_Command(
            unityAudioDirectory: Path.Combine(Application.dataPath, "Audio")
        )));
    }

    public void SendAudioDevices(ResponseFromServer response)
    {
        // todo: read devices data here from response
        // todo: add here checks
        namedPipeClient.SendCommandAsync(CommonUtilities.SerializeJson(new UpdateDevicesParameters_Command(audioDevicesInfo)));
    }

    public void SendTestAudioSignalToDevice(string audioOutputDeviceName, string audioFileName = "test.mp3")
    {
        namedPipeClient.SendCommandAsync(CommonUtilities.SerializeJson(new PlayAudioFile_Command(audioFileName: audioFileName, audioOutputDeviceName: audioOutputDeviceName)));
    }

    public void RequestAudioDevices(ResponseFromServer response)
    {
        namedPipeClient.SendCommandAsync(CommonUtilities.SerializeJson(new GetAudioDevices_Command(doUpdate: false)));
    }

    // TODO: clean up and refactor
    public void ChangeAudioDeviceVolume(string? deviceName, float? volume)
    {
        if (String.IsNullOrEmpty(deviceName))
        {
            // temp
            UnityEngine.Debug.Log($"The device name is incorrect: {deviceName}");
            return;
        }

        if (volume < 0f || volume > 100f || volume == null) //  || newVolume == oldVolume
        {
            // temp
            UnityEngine.Debug.Log($"The device volume is incorrect: {volume}");
            return;
        }

        UnityEngine.Debug.Log($"Before: {audioDevicesInfo.audioOutputDeviceVolume_Researcher}");
        var linkToVolume = uiReferenceToDevicesInfoMap[deviceName];
        audioDevicesInfo.GetType().GetProperty(linkToVolume)?.SetValue(audioDevicesInfo, volume);   // upd device volume
        UnityEngine.Debug.Log($"After: {audioDevicesInfo.audioOutputDeviceVolume_Researcher}");
        //SendAudioDevices();

        // maybe make it separate func
    }

    private void UpdateConfig(ResponseFromServer response)
    {
        // todo
        //UnityEngine.Debug.Log($"Reached the end successfully");
    }
}