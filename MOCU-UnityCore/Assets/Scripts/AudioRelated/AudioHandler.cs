using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Linq;

using AudioControl;
using DaemonsNamespace.InterprocessCommunication;
using UnityDaemonsCommon;


public partial class AudioHandler : MonoBehaviour
{
    #region PRIVATE FIELDS
    private AudioDevicesInfo audioDevicesInfo;

    private NamedPipeClient namedPipeClient;
    private Process audioControlProcess;
    private string namedPipeName;

    private UiHandler _uiHandler;
    private ConfigHandler _configHandler;
    private ExperimentTabHandler _experimentTabHandler;
    private SettingsTabHandler _settingsTabHandler;
    private InputLogic _inputLogic;

    public List<string> inputAudioDevices;
    public List<string> outputAudioDevices;

    private Dictionary<string, string> partlyOptimizedJsonCommands; // in theory, should reduce the delay when sending commands. todo: review later
    private Dictionary<string, (string? subState, Action<ResponseFromServer?> action)> CommandsToExecuteAccordingToServerResponse;  // serverResponse -> updState -> executeNextCommand
    #endregion PRIVATE FIELDS




    #region MANDATORY STANDARD FUNCTIONALITY

    void Awake()
    {
        namedPipeName = "AudioPipe";    // todo: read it from config (private)

        _uiHandler = GetComponent<UiHandler>();
        _configHandler = GetComponent<ConfigHandler>();
        _experimentTabHandler = GetComponent<ExperimentTabHandler>();
        _settingsTabHandler = GetComponent<SettingsTabHandler>();
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

        // todo: rename action to "{commandName}_commandHandler"
        CommandsToExecuteAccordingToServerResponse = new()
        {
            { "TryConnectToServer_Command",         (subState: "StartNamedPipeConnection",  action: SendConfigurationDetails) },
            { "SendConfigs_Command",                (subState: "SetConfigs",                action: RequestAudioDevices) },
            { "GetAudioDevices_Command",            (subState: "RequestAudioDevices",       action: SendAudioDevices) },
            { "UpdateDevicesParameters_Command",    (subState: "SendAudioDevices",          action: UpdateClient) }
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

    void Update()
    {
        if (namedPipeClient == null) return;    // in case it is still not ready

        ProccessInputMessagesQueue();
        ProccessInnerMessagesQueue();
    }

    void OnDestroy()
    {
        try { namedPipeClient?.Destroy();   } catch { }
        try { audioControlProcess?.Kill();  } catch { }
    }

    #endregion MANDATORY STANDARD FUNCTIONALITY

    

    #region PRIVATE METHODS

    // todo: make abstract for this function (in interprocessCommunication)
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
            CloseConnection($"Audio connection closed. Failed to 'StartAudioControlProcess'");
        }
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
            CloseConnection($"Audio connection closed. Failed to 'TryConnectToServer'");
        }
    }

    private void CloseConnection(string message)
    {
        _experimentTabHandler.PrintToWarnings(message);
        OnDestroy();
    }



    private void ProccessInputMessagesQueue()
    {
        while (namedPipeClient.inputMessagesQueue.TryDequeue(out string message))
        {
            try
            {
                // currently no check for "is it realy 'ResponseFromServer'", because this code only gets responses. maybe add later (todo)
                var deserializedMessage = CommonUtilities.DeserializeJson<ResponseFromServer>(message);
                var receivedCommand = CommandsToExecuteAccordingToServerResponse[deserializedMessage.ReceivedCommand];
                var subStateName = receivedCommand.subState;
                var funcToBeExecuted = receivedCommand.action;

                // todo: if 'UpdateDevicesParameters_Command' returned error, it doesn't mean the error is fatal. it needs attention
                if (deserializedMessage.HasError)
                {
                    stateTracker.UpdateSubState(subStateName, false);
                    _experimentTabHandler.PrintToWarnings($"Failed to '{subStateName}'");

                    // pass this message, go to the next (if there is any). Will not continue to execute methods in the chain
                    continue;
                }

                // In case everything is fine
                stateTracker.UpdateSubState(subStateName, true);
                funcToBeExecuted.Invoke(deserializedMessage);       // pay attention: deserializedMessage.ExtraData is still 'JObject' type
            }
            catch
            {
                //stateTracker.UpdateSubState("StartAudioProcess", false);    // not realy it, but need to be something
                _experimentTabHandler.PrintToWarnings($"Total fail while trying read incoming message from server");
            }
        }
    }

    private void ProccessInnerMessagesQueue()
    {
        while (namedPipeClient.innerMessagesQueue.TryDequeue(out (string messageText, InnerMessageType messageType) message))
        {
            if (message.messageType == InnerMessageType.Info)
            {
                //_uiHandler.PrintToInfo(message.messageText);
            }

            if (message.messageType == InnerMessageType.Error)
            {
                CloseConnection($"Audio connection closed. {message.messageText}");
                stateTracker.UpdateSubState("StartNamedPipeConnection", false);
            }
        }
    }

    // todo: think about path that external program can access to. also thing about an option to change audio files from UI dynamically
    private void SendConfigurationDetails(ResponseFromServer response)
    {
        namedPipeClient.SendCommandAsync(CommonUtilities.SerializeJson(new SendConfigs_Command(
            unityAudioDirectory: Path.Combine(Application.dataPath, "Audio")
        )));
    }
    
    private void RequestAudioDevices(ResponseFromServer response)
    {
        namedPipeClient.SendCommandAsync(CommonUtilities.SerializeJson(new GetAudioDevices_Command(doUpdate: false)));
    }

    private void ValidateAndUpdateDevicesInfo(ResponseFromServer response)
    {
        // need this (JObject) convertion because of json lib. It doesn't know what 'object' is, so translates it to 'JObject'
        var devicesData = CommonUtilities.ConvertJObjectToType<AudioDevicesLists>((JObject)response.ExtraData);

        inputAudioDevices = devicesData.InputDevices;
        outputAudioDevices = devicesData.OutputDevices;

        var op = audioDevicesInfo.audioOutputDeviceName_Participant;
        var or = audioDevicesInfo.audioOutputDeviceName_Researcher;
        var ip = audioDevicesInfo.audioInputDeviceName_Participant;
        var ir = audioDevicesInfo.audioInputDeviceName_Researcher;

        // HERE: I guess bug is somewhere here: check if it returns NULL and why

        // if in reality there is no such device (but in config is), update it (in 'audioDevicesInfo') to 'null'
        op = outputAudioDevices.Contains(op) ? op : null;
        or = outputAudioDevices.Contains(or) ? or : null;
        ip = inputAudioDevices.Contains(ip) ? ip : null;
        ir = inputAudioDevices.Contains(ir) ? ir : null;
    }

    private void SendAudioDevices(ResponseFromServer response)
    {
        ValidateAndUpdateDevicesInfo(response);
        namedPipeClient.SendCommandAsync(CommonUtilities.SerializeJson(new UpdateDevicesParameters_Command(audioDevicesInfo)));
    }

    /// <summary>
    /// Called after successful devices update on the server side
    /// </summary>
    private void UpdateClient(ResponseFromServer response)
    {
        //_configHandler.UpdateSubConfig(audioDevicesInfo);                     // if all ok -- server returns null (figure out how to handle half-errors)
        // todo: trigger event in SettingsTabHandler to update UI               <--- HERE
        //print($"inputAudioDevices: {inputAudioDevices.Count}");
        //print($"outputAudioDevices: {outputAudioDevices.Count}");

        _settingsTabHandler.UpdateAudioDevices(new AudioDataCrossClassesPacket(
            audioDevicesInfo: audioDevicesInfo,
            inputAudioDevices: inputAudioDevices,
            outputAudioDevices: outputAudioDevices
        ));
    }


    #endregion PRIVATE METHODS

    public enum DevicesListTypes
    {
        Output,
        Input
    }
}