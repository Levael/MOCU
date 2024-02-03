using System.Collections.Concurrent;
using System.IO.Pipes;
using System.IO;
using System;
using System.Threading;
using System.Threading.Tasks;
using CommonUtilitiesNamespace;

namespace InterprocessCommunication
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



        public NamedPipeClient(string namedPipeName)
        {
            _pipeClient = new NamedPipeClientStream(".", namedPipeName, PipeDirection.InOut, PipeOptions.Asynchronous);    // '.' means this PC, not via LAN
            _pipeClient.Connect();

            _streamReader = new StreamReader(_pipeClient);
            _streamWriter = new StreamWriter(_pipeClient);

            inputMessagesQueue = new();
            outputMessagesQueue = new();
            innerMessagesQueue = new();

            ReadMessagesAsync();
        }

        public void Destroy()
        {
            try { _streamReader?.Close(); } catch { }
            try { _streamWriter?.Close(); } catch { }
            try { _pipeClient?.Close(); } catch { }

            try { _streamReader?.Dispose(); } catch { }
            try { _streamWriter?.Dispose(); } catch { }
            try { _pipeClient?.Dispose(); } catch { }
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

                        innerMessagesQueue.Enqueue(($"command sent: {message}", InnerMessageType.Info));
                    }
                    catch
                    {
                        innerMessagesQueue.Enqueue(($"error when sending: {message}", InnerMessageType.Error));
                    }
                }
            }

            _isProcessingWriting = false;
        }

        public void SendCommandAsync(string command)
        {
            outputMessagesQueue.Enqueue(command);

            if (!_isProcessingWriting)
            {
                ProcessMessages();
            }
        }

        private async void ReadMessagesAsync()
        {
            while (_pipeClient.IsConnected)
            {
                string response = await _streamReader.ReadLineAsync();
                if (response != null)
                {
                    inputMessagesQueue.Enqueue(response);
                }
                else
                {
                    innerMessagesQueue.Enqueue(($"response == null", InnerMessageType.Error));
                }
            }
        }
    }





    public class NamedPipeServer
    {
        private NamedPipeServerStream   _pipeServer;
        private StreamReader            _streamReader;
        private StreamWriter            _streamWriter;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private string namedPipeName;

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
        /// <param name="commandProcessor_dependencyInjection">Is called every time when new message added to "inputMessagesQueue"</param>
        public NamedPipeServer(string namedPipeName, ObservableConcurrentQueue<string>.ProcessItemDelegate commandProcessor_dependencyInjection)
        {
            this.namedPipeName = namedPipeName;

            inputMessagesQueue = new(commandProcessor_dependencyInjection);
            outputMessagesQueue = new(SendCommandAsync);
            innerMessagesQueue = new(ShowNotification);
        }

        public void Start()
        {
            _pipeServer = new NamedPipeServerStream(namedPipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            _pipeServer.WaitForConnection();

            _streamReader = new StreamReader(_pipeServer);
            _streamWriter = new StreamWriter(_pipeServer);

            ReadMessages();
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
        /// Is almost always ON and waits for new messages from the server
        /// </summary>
        public async Task<string> ReadCommandAsync()
        {
            string message = await _streamReader.ReadLineAsync();

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
        public async void ShowNotification((string messageText, InnerMessageType messageType) notification)
        {
            if (notification.messageType == InnerMessageType.Error)
                Console.ForegroundColor = ConsoleColor.Red;
            else
                Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine(notification.messageText);

            Console.ForegroundColor = ConsoleColor.White;
        }

        public async void ShowNotification(string messageText)
        {
            Console.WriteLine(messageText);
        }





        public void Stop()
        {
            _cts.Cancel();
            Destroy();
        }

        private void Destroy()
        {
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
        Info
    }

}














/*using System.Collections.Concurrent;
using System.IO.Pipes;
using System.IO;
using System;
using System.Threading;
using System.Threading.Tasks;
using static UnityEditor.Progress;

namespace InterprocessCommunication
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



        public NamedPipeClient(string namedPipeName)
        {
            _pipeClient = new NamedPipeClientStream(".", namedPipeName, PipeDirection.InOut, PipeOptions.Asynchronous);    // '.' means this PC, not via LAN
            _pipeClient.Connect();

            _streamReader = new StreamReader(_pipeClient);
            _streamWriter = new StreamWriter(_pipeClient);

            inputMessagesQueue = new();
            outputMessagesQueue = new();
            innerMessagesQueue = new();

            ReadMessagesAsync();
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

                        innerMessagesQueue.Enqueue(($"command sent: {message}", InnerMessageType.Info));
                    }
                    catch
                    {
                        innerMessagesQueue.Enqueue(($"error when sending: {message}", InnerMessageType.Error));
                    }
                }
            }

            _isProcessingWriting = false;
        }

        public void SendCommandAsync(string command)
        {
            outputMessagesQueue.Enqueue(command);

            if (!_isProcessingWriting)
            {
                ProcessMessages();
            }
        }

        private async void ReadMessagesAsync()
        {
            while (_pipeClient.IsConnected)
            {
                string response = await _streamReader.ReadLineAsync();
                if (response != null)
                {
                    inputMessagesQueue.Enqueue(response);
                }
                else
                {
                    innerMessagesQueue.Enqueue(($"response == null", InnerMessageType.Error));
                }
            }
        }
    }





    public class NamedPipeServer
    {
        private NamedPipeServerStream _pipeServer;
        private StreamReader _streamReader;
        private StreamWriter _streamWriter;
        private CancellationTokenSource _cts = new CancellationTokenSource();

        private ICommandProcessor _commandProcessor;
        private bool _isProcessingWriting = false;

        /// <summary>
        /// Occurs when an error is detected in the NamedPipeServer and handles by parrent Class
        /// </summary>
        public event Action<string> ErrorOccurred;

        public event Action<string> InnerMessageSent;

        public ConcurrentQueue<string> inputMessagesQueue;
        public ConcurrentQueue<string> outputMessagesQueue;
        public ConcurrentQueue<(string messageText, InnerMessageType messageType)> innerMessagesQueue;  // for cross-class connection



        public NamedPipeServer(string namedPipeName, ICommandProcessor commandProcessor_dependencyInjection)
        {
            _pipeServer = new NamedPipeServerStream(namedPipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            _pipeServer.WaitForConnection();

            _streamReader = new StreamReader(_pipeServer);
            _streamWriter = new StreamWriter(_pipeServer);

            inputMessagesQueue = new();
            outputMessagesQueue = new();
            innerMessagesQueue = new();

            _commandProcessor = commandProcessor_dependencyInjection;

            
            ReadMessages();
        }

        private async void ReadMessages()
        {
            while (!_cts.Token.IsCancellationRequested && _pipeServer.IsConnected)
            {
                try
                {
                    string message = await ReadCommandAsync();  // 99.99% of time will be here waiting for new message
                    Task.Run(() => ProcessResponse(message));   // on purpose launches as a new task so that the next message will be read immediately

                }
                catch (IOException ex)
                {
                    OnErrorOccurred($"ReadMessages-StartAsync-IO exception :: {ex.Message}");
                }
                catch (OperationCanceledException)
                {
                    OnErrorOccurred("ReadMessages :: Operation canceled");
                }
            }
        }

        public async Task<string> ReadCommandAsync()
        {
            string message = await _streamReader.ReadLineAsync();

            if (message == null) throw new Exception("ReadCommandAsync :: message == null");

            return message;
        }

        private void ProcessResponse(string message)
        {
            try
            {
                var response = _commandProcessor.ProcessCommand(message);
                SendCommandAsync(response);

                innerMessagesQueue.Enqueue(($"COMMAND GOT: {message}\nRESPONSE SENT: {response}\n\n", InnerMessageType.Info));
            }
            catch
            {
                innerMessagesQueue.Enqueue(($"error when reading: {message}", InnerMessageType.Error));
            }
        }






        public void SendCommandAsync(string command)
        {
            outputMessagesQueue.Enqueue(command);

            if (!_isProcessingWriting)
            {
                ProcessMessagesWriting();
            }
        }

        private async void ProcessMessagesWriting()
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

                        innerMessagesQueue.Enqueue(($"command sent: {message}", InnerMessageType.Info));
                    }
                    catch
                    {
                        innerMessagesQueue.Enqueue(($"error when sending: {message}", InnerMessageType.Error));
                    }
                }
            }

            _isProcessingWriting = false;
        }

        



        public void Stop()
        {
            _cts.Cancel();
            Destroy();
        }

        private void Destroy()
        {
            try { _streamReader?.Dispose(); } catch { }
            try { _streamWriter?.Dispose(); } catch { }
            try { _pipeServer?.Dispose();   } catch { }
        }

        protected virtual void OnErrorOccurred(string message)
        {
            Stop();
            ErrorOccurred?.Invoke(message);
        }

        protected virtual void OnInnerMessageSent(string message)
        {
            InnerMessageSent?.Invoke(message);
        }
    }



    public enum InnerMessageType
    {
        Error,
        Info
    }


    public interface ICommandProcessor
    {
        public string ProcessCommand(string jsonCommand);
    }


}

*/