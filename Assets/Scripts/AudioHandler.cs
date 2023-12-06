using System.IO.Pipes;
using System.IO;
using UnityEngine;
using System.Threading;
using System.Diagnostics;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public class AudioHandler : MonoBehaviour
{
    private UiHandler _uiHandler;

    private Process audioControlProcess;
    private NamedPipeClientStream pipeClient;
    private StreamWriter streamWriter;
    private StreamReader streamReader;
    private CancellationTokenSource cancellationTokenSource;

    private ConcurrentQueue<string> messagesToProcess = new ConcurrentQueue<string>();




    void Awake()
    {
        _uiHandler = GetComponent<UiHandler>();

    }

    void Start()
    {
        StartAudioControlProcess();
        StartPipeClientAsync();
        /*StartListeningResponses();*/
    }

    void Update()
    {
        /*while (messagesToProcess.TryDequeue(out string message))
        {
            UnityEngine.Debug.Log("Обработка сообщения: " + message);
        }*/
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
        string externalAppPath = "Assets/ExternalTools/AudioControl/AudioControl.exe";
        int parentProcessId = Process.GetCurrentProcess().Id;

        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = externalAppPath,
            Arguments = parentProcessId.ToString(),
            UseShellExecute = false,
            RedirectStandardOutput = false,
            CreateNoWindow = true
        };

        audioControlProcess = new Process() { StartInfo = startInfo };
        audioControlProcess.Start();
    }

    async void StartPipeClientAsync()
    {
        try
        {
            cancellationTokenSource = new CancellationTokenSource();
            pipeClient = new NamedPipeClientStream(".", "AudioPipe", PipeDirection.InOut);

            await pipeClient.ConnectAsync(cancellationTokenSource.Token);
            streamWriter = new StreamWriter(pipeClient);
            streamReader = new StreamReader(pipeClient);
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
            while (true)
            {
                try
                {
                    string response = ReceiveResponse();
                    if (response == null) break;
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
        streamWriter.WriteLine(command);
        streamWriter.Flush();
    }

    private string ReceiveResponse()
    {
        return streamReader.ReadLine();
    }

}
