using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using AudioControl;
using CommonUtilitiesNamespace;
using InterprocessCommunication;
using System.Threading.Tasks;
using System.Collections;

public class AudioHandler : MonoBehaviour
{
    public StateTracker stateTracker;

    private NamedPipeClient namedPipeClient;
    private Process audioControlProcess;
    private string namedPipeName;

    private UiHandler _uiHandler;
    private ExperimentTabHandler _experimentTabHandler;
    private InputLogic _inputLogic;

    public List<string> inputAudioDevices;
    public List<string> outputAudioDevices;

    private Dictionary<string, string> partlyOptimizedJsonCommands; // in theory, should reduce the delay when sending commands. todo: review later



    void Awake()
    {
        namedPipeName = "AudioPipe";

        _uiHandler = GetComponent<UiHandler>();
        _experimentTabHandler = GetComponent<ExperimentTabHandler>();
        _inputLogic = GetComponent<InputLogic>();

        stateTracker = new StateTracker(new[] { "SetConfigs", "UpdateAudioDevices", "StartAudioProcess", "StartNamedPipeConnection" });    // todo: maybe read from config. maybe not

        inputAudioDevices = new();
        outputAudioDevices = new();

        partlyOptimizedJsonCommands = new() {
            {"StartIntercomStream_ResearcherToParticipant_Command", CommonUtilities.SerializeJson(new StartIntercomStream_ResearcherToParticipant_Command())},
            {"StartIntercomStream_ParticipantToResearcher_Command", CommonUtilities.SerializeJson(new StartIntercomStream_ParticipantToResearcher_Command())},
            {"StopIntercomStream_ResearcherToParticipant_Command", CommonUtilities.SerializeJson(new StopIntercomStream_ResearcherToParticipant_Command())},
            {"StopIntercomStream_ParticipantToResearcher_Command", CommonUtilities.SerializeJson(new StopIntercomStream_ParticipantToResearcher_Command())},
        };

    }

    // Workaround for Unity's limitations with async in 'Start' method
    void Start()
    {
        try
        {
            StartAudioControlProcess();

            StartCoroutine(ConnectToServer(result => {
                if (result == false)
                {
                    CloseConnection($"AudioHandler/Start/ConnectToServer");
                    stateTracker.UpdateSubState("StartNamedPipeConnection", false);
                    return;
                }

                stateTracker.UpdateSubState("StartNamedPipeConnection", true);

                SendConfigs();      // paths and file names
                UpdateAudioDevices();  // send to server side devices parameters to initiate them there
            }));




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

            _uiHandler.mainTabScreen.GetElement("main-test-btn").RegisterCallback<ClickEvent>(evt => {
                RequestAudioDevicesNames();
            });


        } catch (Exception ex)
        {
            CloseConnection($"AudioHandler/Start/error message: {ex}");
            stateTracker.UpdateSubState("StartNamedPipeConnection", false);
        }
        
    }

    void Update()
    {
        while (namedPipeClient.inputMessagesQueue.TryDequeue(out string message))
        {
            var commandName = CommonUtilities.GetSerializedObjectType(message);

            // { } are for using same variable 'obj'
            switch (commandName)
            {
                case "GeneralResponseFromServer_Command":
                    var obj = CommonUtilities.DeserializeJson<GeneralResponseFromServer_Command>(message);
                    switch (obj.ReceivedCommand)
                    {
                        case "SendConfigs_Command":
                            if (obj.HasError)
                            {
                                stateTracker.UpdateSubState("SetConfigs", false);
                                _experimentTabHandler.PrintToWarnings("Failed to 'SendConfigs'");
                            }
                            else
                            {
                                stateTracker.UpdateSubState("SetConfigs", true);
                            }
                            break;
                        case "UpdateDevicesParameters_Command":
                            if (obj.HasError)
                            {
                                stateTracker.UpdateSubState("UpdateAudioDevices", false);
                                _experimentTabHandler.PrintToWarnings("Failed to 'UpdateAudioDevices'");
                            }
                            else
                            {
                                stateTracker.UpdateSubState("UpdateAudioDevices", true);
                            }
                            break;
                    }
                    break;
                case "ResponseFromServer_GetAudioDevices_Command":
                    //_uiHandler.PrintToInfo($"2: {commandName}\n");
                    break;


                default:
                    //_uiHandler.PrintToInfo($"3: {commandName}\n");
                    break;
            }
        }


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

    // Workaround for Unity's limitations with async in 'Start' method
    private IEnumerator ConnectToServer(Action<bool> callback)
    {
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

        Task.Run(async () => {
            try
            {
                namedPipeClient = new NamedPipeClient(namedPipeName);
                bool result = await namedPipeClient.StartAsync();
                tcs.SetResult(result);
            }
            catch
            {
                tcs.SetResult(false);
            }
        });

        yield return new WaitUntil(() => tcs.Task.IsCompleted);
        callback?.Invoke(tcs.Task.Result);
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


    public void SendConfigs()
    {
        namedPipeClient.SendCommandAsync(CommonUtilities.SerializeJson(new SendConfigs_Command(
            unityAudioDirectory: Path.Combine(Application.dataPath, "Audio")
        )));
    }

    public void UpdateAudioDevices()
    {
        namedPipeClient.SendCommandAsync(CommonUtilities.SerializeJson(new UpdateDevicesParameters_Command( new AudioDevicesInfo(
            audioOutputDeviceNameResearcher: "Speakers (Realtek High Definition Audio)",
            audioInputDeviceNameResearcher: "Microphone (fifine Microphone)",
            audioOutputDeviceNameParticipant: "Headphones (Rift Audio)",
            audioInputDeviceNameParticipant: "Microphone (Rift Audio)",
            audioOutputDeviceVolumeResearcher: 77f,
            audioOutputDeviceVolumeParticipant: 69f
        ))));
    }

    public void SendTestAudioSignalToDevice(string audioOutputDeviceName, string audioFileName = "test.mp3")
    {
        namedPipeClient.SendCommandAsync(CommonUtilities.SerializeJson(new PlayAudioFile_Command(audioFileName: audioFileName, audioOutputDeviceName: audioOutputDeviceName)));
    }

    public void RequestAudioDevicesNames()
    {
        namedPipeClient.SendCommandAsync(CommonUtilities.SerializeJson(new GetAudioDevices_Command(doUpdate: true)));
    }

    // didn't test
    /*public void ChangeAudioDeviceVolume(string deviceName, float volume)
    {
        namedPipeClient.SendCommandAsync(CommonUtilities.SerializeJson(new ChangeOutputDeviceVolume_Command(name: deviceName, volume: volume)));
    }*/
}