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
    private List<string> inputAudioDevices;
    private List<string> outputAudioDevices;

    private DaemonProcess _daemon;
    private string executableFileName;

    private UiHandler _uiHandler;
    private ConfigHandler _configHandler;
    private ExperimentTabHandler _experimentTabHandler;
    private SettingsTabHandler _settingsTabHandler;
    private InputLogic _inputLogic;
    private DaemonsHandler _daemonsHandler;

    private Dictionary<string, string> partlyOptimizedJsonCommands; // in theory, should reduce the delay when sending commands. todo: review later
    private Dictionary<string, (AudioHandler_Statuses? subState, Action<ResponseFromServer?> action)> CommandsToExecuteAccordingToServerResponse;  // serverResponse -> updState -> executeNextCommand
    #endregion PRIVATE FIELDS




    #region MANDATORY STANDARD FUNCTIONALITY

    void Awake()
    {
        executableFileName = "AudioControl";    // todo: read it from config (private)

        _uiHandler = GetComponent<UiHandler>();
        _configHandler = GetComponent<ConfigHandler>();
        _experimentTabHandler = GetComponent<ExperimentTabHandler>();
        _settingsTabHandler = GetComponent<SettingsTabHandler>();
        _inputLogic = GetComponent<InputLogic>();

        _daemonsHandler = GetComponent<DaemonsHandler>();

        stateTracker = new StateTracker(typeof(AudioHandler_Statuses));

        inputAudioDevices = new();
        outputAudioDevices = new();

        partlyOptimizedJsonCommands = new() {
            { "StartIntercomStream_ResearcherToParticipant_Command", CommonUtilities.SerializeJson(new StartIntercomStream_ResearcherToParticipant_Command()) },
            { "StartIntercomStream_ParticipantToResearcher_Command", CommonUtilities.SerializeJson(new StartIntercomStream_ParticipantToResearcher_Command()) },
            { "StopIntercomStream_ResearcherToParticipant_Command", CommonUtilities.SerializeJson(new StopIntercomStream_ResearcherToParticipant_Command()) },
            { "StopIntercomStream_ParticipantToResearcher_Command", CommonUtilities.SerializeJson(new StopIntercomStream_ParticipantToResearcher_Command()) },
        };

        // todo: rename action to "{commandName}_commandHandler"
        /// 1) sends configs (audio files path)
        /// 2) sends desired data (which devices to use)
        /// 3) 
        CommandsToExecuteAccordingToServerResponse = new()
        {
            { "SendConfigs_Command",                (subState: AudioHandler_Statuses.SetConfigs,                action: SendClientAudioDataDesire) },
            { "UpdateDevicesParameters_Command",    (subState: AudioHandler_Statuses.SendAudioDevices,          action: GetServerAudioDataDecision) }

            /*{ "SendConfigs_Command",                (subState: AudioHandler_Statuses.SetConfigs,                action: RequestAudioDevices) },
            { "GetAudioDevices_Command",            (subState: AudioHandler_Statuses.RequestAudioDevices,       action: SendClientAudioDataDesire) },
            { "UpdateDevicesParameters_Command",    (subState: AudioHandler_Statuses.SendClientAudioDataDesire,          action: GetServerAudioDataDecision) }*/
        };

        // Reading from config Audio Devices Data
        audioDevicesInfo = _configHandler.defaultConfig.AudioConfig;


        // Event listeners for intercom
        _inputLogic.startOutgoingIntercomStream += () => {
            _daemon.namedPipeClient.SendCommandAsync(partlyOptimizedJsonCommands["StartIntercomStream_ResearcherToParticipant_Command"]);
        };

        _inputLogic.stopOutgoingIntercomStream += () => {
            _daemon.namedPipeClient.SendCommandAsync(partlyOptimizedJsonCommands["StopIntercomStream_ResearcherToParticipant_Command"]);
        };

        _inputLogic.startIncomingIntercomStream += () => {
            _daemon.namedPipeClient.SendCommandAsync(partlyOptimizedJsonCommands["StartIntercomStream_ParticipantToResearcher_Command"]);
        };

        _inputLogic.stopIncomingIntercomStream += () => {
            _daemon.namedPipeClient.SendCommandAsync(partlyOptimizedJsonCommands["StopIntercomStream_ParticipantToResearcher_Command"]);
        };
    }

    async void Start()
    {
        _daemon = await _daemonsHandler.InitAndRunDaemon(executableFileName, isHidden: false);

        stateTracker.UpdateSubState(AudioHandler_Statuses.StartAudioProcess, _daemon.isProcessOk);
        stateTracker.UpdateSubState(AudioHandler_Statuses.StartNamedPipeConnection, _daemon.isConnectionOk);

        if (_daemon.isProcessOk && _daemon.isConnectionOk)
            SendConfigurationDetails();
        else
            CloseConnection("AudioHandler / Start : daemon error");
    }

    void Update()
    {
        if (_daemon == null) return;    // in case it is still not ready

        ProccessInputMessagesQueue();
        ProccessInnerMessagesQueue();
    }

    #endregion MANDATORY STANDARD FUNCTIONALITY

    

    #region PRIVATE METHODS

    private void CloseConnection(string message)
    {
        _experimentTabHandler.PrintToWarnings(message);
        _daemonsHandler.KillDaemon(_daemon);
    }



    private void ProccessInputMessagesQueue()
    {
        while (_daemon.namedPipeClient.inputMessagesQueue.TryDequeue(out string message))
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
        while (_daemon.namedPipeClient.innerMessagesQueue.TryDequeue(out (string messageText, InnerMessageType messageType) message))
        {
            if (message.messageType == InnerMessageType.Info)
            {
                //_experimentTabHandler.PrintToInfo(message.messageText);
            }

            if (message.messageType == InnerMessageType.Error)
            {
                CloseConnection($"Audio connection closed. {message.messageText}");
                stateTracker.UpdateSubState(AudioHandler_Statuses.StartNamedPipeConnection, false);
            }
        }
    }

    // todo: think about path that external program can access to. also thing about an option to change audio files from UI dynamically
    private void SendConfigurationDetails(ResponseFromServer response = null)
    {
        _daemon.namedPipeClient.SendCommandAsync(CommonUtilities.SerializeJson(new SendConfigs_Command(
            unityAudioDirectory: Path.Combine(Application.dataPath, "Audio")
        )));
    }
    
    private void RequestAudioDevices(ResponseFromServer response)
    {
        _daemon.namedPipeClient.SendCommandAsync(CommonUtilities.SerializeJson(new GetAudioDevices_Command(doUpdate: false)));
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

    private void SendClientAudioDataDesire(ResponseFromServer response)
    {
        ValidateAndUpdateDevicesInfo(response);
        _daemon.namedPipeClient.SendCommandAsync(CommonUtilities.SerializeJson(new UpdateDevicesParameters_Command(audioDevicesInfo)));
    }

    /// <summary>
    /// Called after successful devices update on the server side
    /// </summary>
    private void GetServerAudioDataDecision(ResponseFromServer response)
    {
        //_configHandler.UpdateSubConfig(audioDevicesInfo);                     // if all ok -- server returns null (figure out how to handle half-errors)
        // todo: trigger event in SettingsTabHandler to update UI               <--- HERE
        //print($"inputAudioDevices: {inputAudioDevices.Count}");
        //print($"outputAudioDevices: {outputAudioDevices.Count}");

        /*_settingsTabHandler.UpdateAudioDevices(new UnifiedAudioDataPacket(
            audioDevicesInfo: audioDevicesInfo,
            inputAudioDevices: inputAudioDevices,
            outputAudioDevices: outputAudioDevices
        ));*/
        
        _settingsTabHandler.UpdateAudioDevices((UnifiedAudioDataPacket)response.ExtraData);

        // todo: update config too
    }


    #endregion PRIVATE METHODS
}