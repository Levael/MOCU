using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class NamedPipeClient : MonoBehaviour
{
    private NamedPipeClientStream pipeClient;
    private StreamReader streamReader;
    private StreamWriter streamWriter;
    private ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();
    private Process audioControlProcess;
    private UiHandler _uiHandler;

    private void Awake()
    {
        _uiHandler = GetComponent<UiHandler>();
    }

    private async void Start()
    {
        StartAudioControlProcess();
        _uiHandler.mainScreen.GetElement("main-test-btn").RegisterCallback<ClickEvent>(evt => SendCommand("xxx"));

        pipeClient = new NamedPipeClientStream(".", "AudioPipe", PipeDirection.InOut, PipeOptions.Asynchronous);
        await pipeClient.ConnectAsync();
        streamReader = new StreamReader(pipeClient);
        streamWriter = new StreamWriter(pipeClient);
        ReadMessagesAsync();
    }

    private async void ReadMessagesAsync()
    {
        while (pipeClient.IsConnected)
        {
            string response = await streamReader.ReadLineAsync();
            if (response != null)
            {
                messageQueue.Enqueue(response);
            }
        }
    }

    public async void SendCommand(string command)
    {
        await streamWriter.WriteLineAsync(command);
        await streamWriter.FlushAsync();
    }

    private void Update()
    {
        while (messageQueue.TryDequeue(out string message))
        {
            UnityEngine.Debug.Log("Received: " + message);
        }
    }

    private void OnDestroy()
    {
        streamReader?.Close();
        streamWriter?.Close();
        pipeClient?.Close();
    }

    // Пример отправки команды
    public void OnButtonPressed()
    {
        SendCommand("YourCommandHere");
    }

    void StartAudioControlProcess()
    {
        //string externalAppPath = "Assets/ExternalTools/AudioControl/AudioControl.exe";
        //string externalAppPath = @"C:\Users\Levael\GitHub\MOCU\Assets\ExternalTools\AudioControl\AudioControl.exe";
        string externalAppPath = @"C:\Users\Levael\GitHub\AudioControl\AudioControl\bin\Release\net7.0\AudioControl.exe";
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
        _uiHandler.mainScreen.GetElement("main-test-btn").RegisterCallback<ClickEvent>(evt => SendCommand("xxx"));

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

    private void SendCommand(string command)
    {
        try
        {
            if (streamWriter != null)
            {
                UnityEngine.Debug.Log("SendCommand before");
                streamWriter.WriteLine(command);
                streamWriter.Flush();
                UnityEngine.Debug.Log("SendCommand after");
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