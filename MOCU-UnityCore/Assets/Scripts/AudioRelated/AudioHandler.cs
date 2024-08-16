using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

using AudioControl;
using InterprocessCommunication;

// todo: not allow intercom (on the client side) if any device is missing (null)


public partial class AudioHandler : MonoBehaviour, IDaemonUser, IControllableInitiation
{
    #region PRIVATE FIELDS
    private DaemonHandler_Client _daemon;

    private AudioDevicesInfo audioDevicesInfo;
    private List<string> inputAudioDevices;
    private List<string> outputAudioDevices;

    private UiHandler _uiHandler;
    private ConfigHandler _configHandler;
    private ExperimentTabHandler _experimentTabHandler;
    private SettingsTabHandler _settingsTabHandler;
    private InputLogic _inputLogic;
    private DaemonsHandler _daemonsHandler;

    private Dictionary<string, UnifiedCommandFrom_Client> partlyOptimizedJsonCommands; // in theory, should reduce the delay when sending commands. todo: review later
    private Dictionary<string, (AudioHandler_Statuses? subState, Action<UnifiedResponseFrom_Server> action)> commandsToExecuteAccordingToServerResponse;  // serverResponse -> updState -> executeNextCommand
    #endregion PRIVATE FIELDS

    public bool IsComponentReady {  get; private set; }


    #region MANDATORY STANDARD FUNCTIONALITY

    public void ControllableAwake()
    {
        stateTracker = new StateTracker(typeof(AudioHandler_Statuses));

        inputAudioDevices = new();
        outputAudioDevices = new();

        partlyOptimizedJsonCommands = new() {
            { "StartIntercomStream_ResearcherToParticipant_Command", new UnifiedCommandFrom_Client(name: "StartOutgoingIntercomStream_Command") },
            { "StartIntercomStream_ParticipantToResearcher_Command", new UnifiedCommandFrom_Client(name: "StartIncomingIntercomStream_Command") },
            { "StopIntercomStream_ResearcherToParticipant_Command", new UnifiedCommandFrom_Client(name: "StopOutgoingIntercomStream_Command") },
            { "StopIntercomStream_ParticipantToResearcher_Command", new UnifiedCommandFrom_Client(name: "StopIncomingIntercomStream_Command") },
        };

        // todo: rename action to "{commandName}_commandHandler"
        /// 1) sends configs (audio files path)
        /// 2) sends desired data (which devices to use)
        /// 3) 
        commandsToExecuteAccordingToServerResponse = new()
        {
            { "SetConfigurations_Response",       (subState: AudioHandler_Statuses.SetConfigs,        action: SendClientAudioDataDesire) },
            { "AudioDataHasBeenUpdated_Response", (subState: AudioHandler_Statuses.GetAudioDevices,   action: GotServerAudioDataDecision) }
        };
    }

    public async void ControllableStart()
    {
        _uiHandler = GetComponent<UiHandler>();
        _configHandler = GetComponent<ConfigHandler>();
        _experimentTabHandler = GetComponent<ExperimentTabHandler>();
        _settingsTabHandler = GetComponent<SettingsTabHandler>();
        _inputLogic = GetComponent<InputLogic>();
        _daemonsHandler = GetComponent<DaemonsHandler>();

        // Reading from config Audio Devices Data
        audioDevicesInfo = _configHandler.defaultConfig.AudioConfig;

        _daemon = await _daemonsHandler.CreateDaemon(DaemonsHandler.Daemons.Audio);

        stateTracker.UpdateSubState(AudioHandler_Statuses.StartAudioProcess, _daemon.isProcessOk);
        stateTracker.UpdateSubState(AudioHandler_Statuses.StartNamedPipeConnection, _daemon.isConnectionOk);

        if (_daemon.isProcessOk && _daemon.isConnectionOk)
            SendConfigurationDetails();
        else
            CloseConnectionWithDaemon("AudioHandler / Start : daemon error");

        AddEventListeners();
    }


    #endregion MANDATORY STANDARD FUNCTIONALITY

    private void AddEventListeners ()
    {
        // Event listeners for intercom
        // todo: is there any check of status?
        _inputLogic.startOutgoingIntercomStream += () => {
            _daemon.SendCommand(partlyOptimizedJsonCommands["StartIntercomStream_ResearcherToParticipant_Command"]);
        };

        _inputLogic.stopOutgoingIntercomStream += () => {
            _daemon.SendCommand(partlyOptimizedJsonCommands["StopIntercomStream_ResearcherToParticipant_Command"]);
        };

        _inputLogic.startIncomingIntercomStream += () => {
            _daemon.SendCommand(partlyOptimizedJsonCommands["StartIntercomStream_ParticipantToResearcher_Command"]);
        };

        _inputLogic.stopIncomingIntercomStream += () => {
            _daemon.SendCommand(partlyOptimizedJsonCommands["StopIntercomStream_ParticipantToResearcher_Command"]);
        };
    }

    public void ProcessResponse(UnifiedResponseFrom_Server response)
    {
        try
        {
            var receivedCommand = commandsToExecuteAccordingToServerResponse[response.name];
            var subStateName = receivedCommand.subState;
            var funcToBeExecuted = receivedCommand.action;

            if (response.errorOccurred == true && response.errorIsFatal != true)
            {
                _experimentTabHandler.PrintToWarnings($"Minor error: {response.errorMessage}");
            }
            else if (response.errorOccurred == true && response.errorIsFatal == true)
            {
                stateTracker.UpdateSubState(subStateName, false);
                _experimentTabHandler.PrintToWarnings($"Fatal error: {response.errorMessage}");
                return;
            }

            // In case everything is fine
            if (subStateName != null)
                stateTracker.UpdateSubState(subStateName, true);

            funcToBeExecuted.Invoke(response);
        }
        catch
        {
            _experimentTabHandler.PrintToWarnings($"Total fail while trying read incoming message from server");
        }
    }



    #region PRIVATE METHODS

    private void CloseConnectionWithDaemon(string message)
    {
        _experimentTabHandler.PrintToWarnings(message);
        _daemon.StopDaemon();
    }

    private void SendConfigurationDetails(UnifiedResponseFrom_Server response = null)
    {
        if (!IsDaemonOk())
        {
            UnityEngine.Debug.LogError("Custom: 'SendConfigurationDetails' is unavailable right now. 'IsDaemonOk' returned 'false'");
            return;
        }

        var commandName = "SetConfigurations_Command";
        var payloadData = new SetConfigurations_CommandDetails(unityAudioDirectory: Path.Combine(Application.streamingAssetsPath, "Audio"));
        var fullCommand = new UnifiedCommandFrom_Client(name: commandName, extraData: payloadData);

        _daemon.SendCommand(fullCommand);
    }
    
    /*private void RequestAudioDevices(ResponseFromServer response)
    {
        _daemon.namedPipeClient.SendCommandAsync(CommonUtilities.SerializeJson(new GetAudioDevices_Command(doUpdate: false)));
    }*/

    /*private void ValidateAndUpdateDevicesInfo(ResponseFromServer response)
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
    }*/

    private void SendClientAudioDataDesire(UnifiedResponseFrom_Server response)
    {
        //ValidateAndUpdateDevicesInfo(response);
        //print("before 'SendClientAudioDataDesire'");
        if (!IsDaemonOk())
        {
            UnityEngine.Debug.LogError("Custom: 'SendClientAudioDataDesire' is unavailable right now. 'IsDaemonOk' returned 'false'");
            return;
        }

        _daemon.SendCommand(new UnifiedCommandFrom_Client(name: "SetUpdatedAudioDevicesInfo_Command", extraData: audioDevicesInfo));
        //print("after 'SendClientAudioDataDesire'");
    }

    /// <summary>
    /// Called after successful devices update on the server side
    /// </summary>
    private void GotServerAudioDataDecision(UnifiedResponseFrom_Server response)
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
        
        _settingsTabHandler.UpdateAudioDevices(response.GetExtraData<UnifiedAudioDataPacket>());

        // todo: update config too
    }

    private bool IsDaemonOk()
    {
        return (_daemon != null && _daemon.isConnectionOk && _daemon.isProcessOk);
    }

    #endregion PRIVATE METHODS
}