using System.Collections.Concurrent;
using System.IO.Pipes;
using System.IO;
using System;
using System.Threading;
using System.Threading.Tasks;

using CustomDataStructures;
using DaemonsNamespace.Common;


namespace DaemonsNamespace.InterprocessCommunication
{
    public class NamedPipeClient
    {
        private NamedPipeClientStream _pipeClient;
        private StreamReader _streamReader;
        private StreamWriter _streamWriter;

        public ConcurrentQueue<string> inputMessagesQueue;
        public ConcurrentQueue<string> outputMessagesQueue;
        public ConcurrentQueue<(string messageText, InnerMessageType messageType)> innerMessagesQueue;  // for cross-class connection

        private bool _isProcessingWriting = false;
        private int _connectionTimeoutMs = 5000;    // todo: read from config (to avoid freezes)
        private string _namedPipeName;
        private CancellationTokenSource _cts = new CancellationTokenSource();

        public bool isConnectionAlive = false;  // todo: add later to server class too




        public NamedPipeClient(string namedPipeName)
        {
            _namedPipeName = namedPipeName;

            inputMessagesQueue = new();
            outputMessagesQueue = new();
            innerMessagesQueue = new();
        }

        public async Task<bool> StartAsync()
        {
            try
            {
                _pipeClient = new NamedPipeClientStream(".", _namedPipeName, PipeDirection.InOut, PipeOptions.Asynchronous);    // '.' means this PC, not via LAN

                var connectTask = _pipeClient.ConnectAsync(_cts.Token);
                if (await Task.WhenAny(connectTask, Task.Delay(_connectionTimeoutMs)) != connectTask)
                {
                    // In case connect task did not complete in time
                    _cts.Cancel();
                    isConnectionAlive = false;
                    return isConnectionAlive;
                }

                // If connected successfully, initialize stream reader and writer
                _streamReader = new StreamReader(_pipeClient);
                _streamWriter = new StreamWriter(_pipeClient);

                // Start reading messages asynchronously
                StartReadingMessagesAsync();

                isConnectionAlive = true;
                return isConnectionAlive;
            }
            catch
            {
                isConnectionAlive = false;
                return isConnectionAlive;
            }

        }


        public void Destroy()
        {
            try { _streamReader?.Close();   } catch { }
            try { _streamWriter?.Close();   } catch { }
            try { _pipeClient?.Close();     } catch { }

            try { _streamReader?.Dispose(); } catch { }
            try { _streamWriter?.Dispose(); } catch { }
            try { _pipeClient?.Dispose();   } catch { }
        }


        private async void ProcessMessages()
        {
            _isProcessingWriting = true;

            while (!outputMessagesQueue.IsEmpty)
            {
                if (outputMessagesQueue.TryDequeue(out string message))
                {
                    try
                    {
                        await _streamWriter.WriteLineAsync(message);
                        await _streamWriter.FlushAsync();

                        innerMessagesQueue.Enqueue(($"Connection manager/Client/ProcessMessages/command sent: {message}", InnerMessageType.Info));
                    }
                    catch
                    {
                        innerMessagesQueue.Enqueue(($"Connection manager/Client/ProcessMessages/error when sending: {message}", InnerMessageType.Error));
                    }
                }
            }

            _isProcessingWriting = false;
        }

        public void SendCommandAsync(string command)
        {
            if (isConnectionAlive != true) return;

            outputMessagesQueue.Enqueue(command);

            if (!_isProcessingWriting)
            {
                ProcessMessages();
            }
        }

        private async void StartReadingMessagesAsync()
        {
            try
            {
                while (_pipeClient.IsConnected)
                {
                    string response = await _streamReader.ReadLineAsync();

                    if (response == null)
                    {
                        CloseConnection("Connection manager/Client/StartReadingMessagesAsync/response == null");
                        break;
                    }

                    inputMessagesQueue.Enqueue(response);
                }
            }
            catch
            {
                CloseConnection("Connection manager/Client/StartReadingMessagesAsync/unkmown fatal error");
            }
        }

        private void CloseConnection(string message)
        {
            innerMessagesQueue.Enqueue((message, InnerMessageType.Error));
            isConnectionAlive = false;
            Destroy();
        }
    }





    public class NamedPipeServer
    {
        private NamedPipeServerStream   _pipeServer;
        private StreamReader            _streamReader;
        private StreamWriter            _streamWriter;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private string                  _namedPipeName;

        /// <summary>
        /// Occurs when an error is detected in the NamedPipeServer and handles by parrent Class
        /// </summary>
        public event Action<string> ErrorOccurred;

        public ObservableConcurrentQueue<string> inputMessagesQueue;   // input message, outputMessagesQueue where to write the response
        public ObservableConcurrentQueue<string> outputMessagesQueue;
        public ObservableConcurrentQueue<(string messageText, InnerMessageType messageType)> innerMessagesQueue;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="namedPipeName"></param>
        /// <param name="commandProcessor">Is called every time when new message added to "inputMessagesQueue"</param>
        public NamedPipeServer(string namedPipeName, ObservableConcurrentQueue<string>.ProcessItemDelegate commandProcessor)
        {
            _namedPipeName = namedPipeName;

            inputMessagesQueue = new(commandProcessor);
            outputMessagesQueue = new(SendCommandAsync);
            innerMessagesQueue = new(ShowNotification);
        }

        public async void StartAsync()
        {
            try
            {
                _pipeServer = new NamedPipeServerStream(_namedPipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                await _pipeServer.WaitForConnectionAsync(_cts.Token);

                _streamReader = new StreamReader(_pipeServer);
                _streamWriter = new StreamWriter(_pipeServer);

                ReadMessages();
            }
            catch
            {
                OnErrorOccurred("StartAsync - error uccured");
            }
            
        }

        private async void ReadMessages()
        {
            while (!_cts.Token.IsCancellationRequested && _pipeServer.IsConnected)
            {
                try
                {
                    string message = await ReadCommandAsync();  // 99.99% of time will be here waiting for new message
                    inputMessagesQueue.Enqueue(message);
                }
                catch (IOException)
                {
                    OnErrorOccurred("ReadMessages-StartAsync-IO exception :: IOException");
                }
                catch (OperationCanceledException)
                {
                    OnErrorOccurred("ReadMessages :: Operation canceled");
                }
                catch (Exception ex)
                {
                    OnErrorOccurred($"ReadMessages :: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Is almost always ON and waits for new messages from the client
        /// </summary>
        public async Task<string> ReadCommandAsync()
        {
            string? message = await _streamReader.ReadLineAsync();

            if (message == null) throw new Exception("ReadCommandAsync :: message == null");

            return message;
        }

        /// <summary>
        /// Is called when result added to "outputMessagesQueue"
        /// </summary>
        public async void SendCommandAsync(string message)
        {
            try
            {
                await _streamWriter.WriteLineAsync(message);
                await _streamWriter.FlushAsync();
            }
            catch
            {
                OnErrorOccurred("SendCommandAsync");
            }
        }

        /// <summary>
        /// Is called when new notification added to "innerMessagesQueue"
        /// </summary>
        public void ShowNotification((string messageText, InnerMessageType messageType) notification)
        {
            if (notification.messageType == InnerMessageType.Error)
                DaemonsUtilities.ConsoleError(notification.messageText);
            else if (notification.messageType == InnerMessageType.Warning)
                DaemonsUtilities.ConsoleWarning(notification.messageText);
            else
                DaemonsUtilities.ConsoleInfo(notification.messageText);
        }

        public void ShowNotification(string messageText)
        {
            DaemonsUtilities.ConsoleInfo(messageText);
        }




        public void Stop()
        {
            _cts.Cancel();
            Destroy();
        }

        private void Destroy()
        {
            try { _streamReader?.Close();   } catch { }
            try { _streamWriter?.Close();   } catch { }
            try { _pipeServer?.Close();     } catch { }

            try { _streamReader?.Dispose(); } catch { }
            try { _streamWriter?.Dispose(); } catch { }
            try { _pipeServer?.Dispose();   } catch { }
        }

        protected virtual void OnErrorOccurred(string message)
        {
            Stop();
            ErrorOccurred?.Invoke(message);
        }
    }


    public enum InnerMessageType
    {
        Error,
        Warning,
        Info
    }

}