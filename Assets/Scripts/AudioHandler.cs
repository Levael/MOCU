using System;
using System.IO;
using System.IO.Pipes;
using System.Diagnostics;
using System.Threading.Tasks;
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

    void Awake()
    {
        _uiHandler = GetComponent<UiHandler>();
        audioPipeConnectionStatus = DeviceConnectionStatus.Disconnected;
        inputMessageQueue = new();
        outputMessageQueue = new();
    }

    async void Start()
    {
        try
        {
            audioPipeConnectionStatus = DeviceConnectionStatus.InProgress;
            StartAudioControlProcess(); 

            _uiHandler.mainScreen.GetElement("main-test-btn").RegisterCallback<ClickEvent>(evt => SendCommandAsync(JsonUtility.ToJson(new StartIntercomStreamCommand(microphoneIndex: 0, speakerIndex: 1))));
            //_uiHandler.mainScreen.GetElement("main-test-btn").RegisterCallback<ClickEvent>(evt => SendCommandAsync(JsonUtility.ToJson(new GetAudioDevicesCommand(doUpdate: true ))));

            pipeClient = new NamedPipeClientStream(".", "AudioPipe", PipeDirection.InOut, PipeOptions.Asynchronous);    // '.' means this PC, not via LAN
            await pipeClient.ConnectAsync();
            streamReader = new StreamReader(pipeClient);
            streamWriter = new StreamWriter(pipeClient);

            ReadMessagesAsync();

            audioPipeConnectionStatus = DeviceConnectionStatus.Connected;
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
            _uiHandler.PrintToWarnings($"Received: {message}\n");
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
                }
                catch (InvalidOperationException ex)
                {
                    _uiHandler.PrintToWarnings(ex.Message);
                }
            }
        }

        isProcessingWriting = false;
    }

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
}






/*using System.IO.Pipes;
using System.IO;
using UnityEngine;
using System.Threading;
using System.Diagnostics;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine.UIElements;

public class AudioHandler : MonoBehaviour
{
    private UiHandler _uiHandler;

    private Process audioControlProcess;
    private NamedPipeClientStream pipeClient;
    private StreamWriter streamWriter;
    private StreamReader streamReader;
    private CancellationTokenSource cancellationTokenSource;

    private ConcurrentQueue<string> messagesToProcess = new ConcurrentQueue<string>();
    private int delayBeforeNextReceiveResponse = 500;   //ms




    void Awake()
    {
        _uiHandler = GetComponent<UiHandler>();

    }

    void Start()
    {
        _uiHandler.mainScreen.GetElement("main-test-btn").RegisterCallback<ClickEvent>(evt => SendCommandAsync("xxx"));

        cancellationTokenSource = new CancellationTokenSource();

        StartAudioControlProcess();
        StartPipeClientAsync();
    }

    void Update()
    {
        while (messagesToProcess.TryDequeue(out string message))
        {
            UnityEngine.Debug.Log("Обработка сообщения: " + message);
        }
    }

    void OnDestroy()
    {
        cancellationTokenSource.Cancel();
        pipeClient?.Dispose();

        if (audioControlProcess != null && !audioControlProcess.HasExited)
        {
            audioControlProcess.Kill();
        }
    }





    void StartAudioControlProcess()
    {
        //string externalAppPath = "Assets/ExternalTools/AudioControl/AudioControl.exe";
        string externalAppPath = @"C:\Users\Levael\GitHub\MOCU\Assets\ExternalTools\AudioControl\AudioControl.exe";
        int parentProcessId = Process.GetCurrentProcess().Id;

        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = externalAppPath,
            Arguments = parentProcessId.ToString(),
            UseShellExecute = true,
            RedirectStandardOutput = false,
            CreateNoWindow = false
        };

        audioControlProcess = new Process() { StartInfo = startInfo };
        audioControlProcess.Start();
    }

    async void StartPipeClientAsync()
    {
        try
        {
            pipeClient = new NamedPipeClientStream(".", "AudioPipe", PipeDirection.InOut);

            await pipeClient.ConnectAsync(cancellationTokenSource.Token);

            streamWriter = new StreamWriter(pipeClient);
            streamReader = new StreamReader(pipeClient);

            StartListeningResponses();
        }
        catch (OperationCanceledException)
        {
            UnityEngine.Debug.Log("Подключение к именованному каналу отменено.");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("Ошибка при подключении к именованному каналу: " + e.Message);
        }
    }

    private void StartListeningResponses()
    {
        Thread responseThread = new Thread(() =>
        {
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    string response = ReceiveResponse();                                                // will block the thread until new message
                    if (response == null) { Thread.Sleep(delayBeforeNextReceiveResponse); continue; };  // in case of error
                    messagesToProcess.Enqueue(response);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError("Ошибка при чтении ответа: " + e.Message);
                    break;
                }
            }
        });
        responseThread.IsBackground = true;
        responseThread.Start();
    }

    private void SendCommandAsync(string command)
    {
        try
        {
            if (streamWriter != null)
            {
                UnityEngine.Debug.Log("SendCommandAsync before");
                streamWriter.WriteLine(command);
                streamWriter.Flush();
                UnityEngine.Debug.Log("SendCommandAsync after");
            }
            else
            {
                UnityEngine.Debug.LogError("StreamWriter не инициализирован.");
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("Ошибка при отправке команды: " + e.Message);
        }
    }

    private string ReceiveResponse()
    {
        try
        {
            if (streamReader != null && !cancellationTokenSource.Token.IsCancellationRequested)
            {
                return streamReader.ReadLine();
            }
            else
            {
                UnityEngine.Debug.LogError("StreamReader не инициализирован.");
                return null;
            }
        }
        catch (IOException e)
        {
            UnityEngine.Debug.LogError("Ошибка ввода-вывода при получении ответа: " + e.Message);
            return null;
        }
        catch (ObjectDisposedException e)
        {
            UnityEngine.Debug.LogError("Попытка использовать закрытый поток: " + e.Message);
            return null;
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("Неизвестная ошибка при получении ответа: " + e.Message);
            return null;
        }
    }

}
*/