using System;
using System.IO;
using System.IO.Pipes;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

using UnityEngine;
using UnityEngine.UIElements;

using AudioControl;



public class AudioHandler : MonoBehaviour
{
    public DeviceConnectionStatus audioPipeConnectionStatus;

    private NamedPipeClientStream pipeClient;
    private StreamReader streamReader;
    private StreamWriter streamWriter;
    private ConcurrentQueue<string> inputMessageQueue;
    private ConcurrentQueue<string> outputMessageQueue;
    private bool isProcessingWriting = false;
    private Process audioControlProcess;

    private UiHandler _uiHandler;
    private InputLogic _inputLogic;

    public List<string> inputAudioDevices;
    public List<string> outputAudioDevices;

    void Awake()
    {
        _uiHandler = GetComponent<UiHandler>();
        _inputLogic = GetComponent<InputLogic>();

        audioPipeConnectionStatus = DeviceConnectionStatus.Disconnected;
        inputMessageQueue = new();
        outputMessageQueue = new();

        inputAudioDevices = new();
        outputAudioDevices = new();
    }

    async void Start()
    {
        try
        {
            audioPipeConnectionStatus = DeviceConnectionStatus.InProgress;
            StartAudioControlProcess();

            pipeClient = new NamedPipeClientStream(".", "AudioPipe", PipeDirection.InOut, PipeOptions.Asynchronous);    // '.' means this PC, not via LAN
            await pipeClient.ConnectAsync();
            streamReader = new StreamReader(pipeClient);
            streamWriter = new StreamWriter(pipeClient);

            ReadMessagesAsync();

            audioPipeConnectionStatus = DeviceConnectionStatus.Connected;

            SetAudioDevices();  // send to server side devices parameters to initiate them there


            // Event listeners for intercom
            _inputLogic.startOutgoingIntercomStream += () => {
                SendCommandAsync(JsonUtility.ToJson(new StartIntercomStream_ResearcherToParticipant_Command()));
            };

            _inputLogic.stopOutgoingIntercomStream += () => {
                SendCommandAsync(JsonUtility.ToJson(new StopIntercomStream_ResearcherToParticipant_Command()));
            };

            _inputLogic.startIncomingIntercomStream += () => {
                SendCommandAsync(JsonUtility.ToJson(new StartIntercomStream_ParticipantToResearcher_Command()));
            };

            _inputLogic.stopIncomingIntercomStream += () => {
                SendCommandAsync(JsonUtility.ToJson(new StopIntercomStream_ParticipantToResearcher_Command()));
            };

            _uiHandler.mainTabScreen.GetElement("main-test-btn").RegisterCallback<ClickEvent>(evt => SendTestAudioSignalToDevice("Speakers (Realtek High Definition Audio)", "test.mp3"));



        } catch (Exception ex)
        {
            audioPipeConnectionStatus = DeviceConnectionStatus.Disconnected;
            _uiHandler.PrintToWarnings($"Error in Start func AudioHandler: {ex}");
        }
        
    }

    void Update()
    {
        while (inputMessageQueue.TryDequeue(out string message))
        {
            _uiHandler.PrintToInfo($"Output queue length: {outputMessageQueue.Count}\nInput queue length: {inputMessageQueue.Count}\n", true);
            //_uiHandler.PrintToInfo($"Received: {message}\n");

            // SendStartConfig
            //SendCommandAsync(JsonUtility.ToJson(new PlayAudioFile_Command(audioFileName: "test.mp3", audioOutputDeviceName: "Fifine")));
        }
    }

    void OnDestroy()
    {
        try { streamReader?.Close();        } catch { }
        try { streamWriter?.Close();        } catch { }
        try { pipeClient?.Close();          } catch { }
        try { pipeClient?.Dispose();        } catch { }
        try { audioControlProcess?.Kill();  } catch { }

        audioPipeConnectionStatus = DeviceConnectionStatus.Disconnected;
    }






    async Task ProcessMessages()
    {
        while (!outputMessageQueue.IsEmpty)
        {
            if (outputMessageQueue.TryDequeue(out string message))
            {
                try
                {
                    await streamWriter.WriteLineAsync(message);
                    await streamWriter.FlushAsync();

                    //_uiHandler.PrintToWarnings($"command sent: {message}");
                }
                catch (InvalidOperationException ex)
                {
                    _uiHandler.PrintToWarnings(ex.Message);
                }
            }
        }

        isProcessingWriting = false;
    }

// todo: come back to this later
#pragma warning disable CS1998
#pragma warning disable CS4014
    public async void SendCommandAsync(string command)
    {
        //UnityEngine.Debug.Log(command);
        outputMessageQueue.Enqueue(command);

        if (!isProcessingWriting)
        {
            isProcessingWriting = true;
            ProcessMessages(); // this method is without "await" in order not to block the current thread
        }
    }
#pragma warning restore CS1998
#pragma warning restore CS4014

    private async void ReadMessagesAsync()
    {
        while (pipeClient.IsConnected)
        {
            string response = await streamReader.ReadLineAsync();
            if (response != null)
            {
                inputMessageQueue.Enqueue(response);
            } else
            {
                audioPipeConnectionStatus = DeviceConnectionStatus.Disconnected;
            }
        }
    }

    private void StartAudioControlProcess()
    {
        string relativeExternalAppPath = @"AudioControl.exe";
        string fullExternalAppPath = Path.Combine(Application.streamingAssetsPath, relativeExternalAppPath);

        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = fullExternalAppPath,
            Arguments = Process.GetCurrentProcess().Id.ToString(),
            UseShellExecute = true,    // false
            RedirectStandardOutput = false,
            CreateNoWindow = false       // true
        };

        audioControlProcess = new Process() { StartInfo = startInfo };
        audioControlProcess.Start();
    }

    public void SetAudioDevices()
    {
        SendCommandAsync(JsonUtility.ToJson(new SetDevicesParameters_Command(
            audioOutputDeviceNameResearcher: "Speakers (Realtek High Definition Audio)",
            audioInputDeviceNameResearcher: "Microphone (fifine Microphone)",
            audioOutputDeviceNameParticipant: "Headphones (Rift Audio)",
            audioInputDeviceNameParticipant: "Microphone (Rift Audio)",
            audioOutputDeviceVolumeResearcher: 77f,
            audioOutputDeviceVolumeParticipant: 69f
        )));
    }

    public void SendTestAudioSignalToDevice(string audioOutputDeviceName, string audioFileName = "test.mp3")
    {
        SendCommandAsync(JsonUtility.ToJson(new PlayAudioFile_Command(audioFileName: audioFileName, audioOutputDeviceName: audioOutputDeviceName)));
    }
}