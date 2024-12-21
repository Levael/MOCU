using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;


namespace InterprocessCommunication
{
    public abstract class InterprocessCommunicator_Base : IInterprocessCommunicator
    {
        private volatile bool _isOperational;
        public bool IsOperational
        {
            get => _isOperational;
            private set => _isOperational = value;
        }

        protected StreamWriter writer;
        protected StreamReader reader;
        protected CancellationTokenSource cancellationTokenSource;
        protected BlockingCollection<string> inputMessagesQueue;
        protected BlockingCollection<string> outputMessagesQueue;
        // todo: maybe add 2 more loops: for events 'MessageSent' and 'InternalMessage'

        private string _pipeName;
        protected string pipeName_clientWritesServerReads;
        protected string pipeName_serverWritesClientReads;

        public event Action<string> MessageReceived;
        public event Action<string> MessageSent;
        public event Action<string> ConnectionEstablished;
        public event Action<string> ConnectionBroked;

        private Task _readLoop;
        private Task _writeLoop;
        private Task _executionLoop;
        private TaskCompletionSource<bool> _connectionEstablished;

        public InterprocessCommunicator_Base(string pipeName)
        {
            _pipeName = pipeName;

            cancellationTokenSource = new CancellationTokenSource();
            inputMessagesQueue = new BlockingCollection<string>();
            outputMessagesQueue = new BlockingCollection<string>();

            pipeName_clientWritesServerReads = $"{_pipeName}C2S";
            pipeName_serverWritesClientReads = $"{_pipeName}S2C";

            _connectionEstablished = new TaskCompletionSource<bool>();
            IsOperational = false;
        }

        public virtual void Start()
        {
            _readLoop = Task.Run(() => ReadLoop());
            _writeLoop = Task.Run(() => WriteLoop());
            _executionLoop = Task.Run(() => ExecutionLoop());

            IsOperational = true;
            ConnectionEstablished?.Invoke($"Pipe name: {_pipeName}");
            _connectionEstablished.TrySetResult(true);
        }

        public async Task<string?> WaitForFirstError()
        {
            try
            {
                await _connectionEstablished.Task;
                var completedTask = await Task.WhenAny(_readLoop, _writeLoop, _executionLoop);

                if (completedTask.IsFaulted)
                    return completedTask.Exception?.Flatten().InnerException?.Message;

                return null;
            }
            catch (Exception ex)
            {
                Dispose();
                return $"Unhandled exception: {ex.Message}";
            }
        }

        public void OnConnectionBroked(string reason)
        {
            if (IsOperational)
            {
                IsOperational = false;
                ConnectionBroked?.Invoke(reason);
            }
                
            Dispose();
        }

        public void SendMessage(string message)
        {
            try
            {
                outputMessagesQueue.Add(message);

                // Notes about 'MessageSent' event:
                // 1) Happens in the calling method's thread.
                // 2) Notifies about sending even if the message is still in standby mode.
                MessageSent?.Invoke(message);      
            }
            catch (Exception ex)
            {
                OnConnectionBroked($"Error occurred while trying to send (API) a message: {ex}");
            }
            
        }

        private async Task ReadLoop()
        {
            try
            {
                var token = cancellationTokenSource.Token;

                while (!token.IsCancellationRequested)
                {
                    var message = await reader.ReadLineAsync();

                    if (message != null)
                        inputMessagesQueue.Add(message, token);
                    else
                        throw new IOException("Got 'null' in 'ReadLoop'");
                }
            }
            catch (Exception ex)
            {
                OnConnectionBroked($"Error occurred while trying to read a message: {ex}");
            }
        }

        private async Task WriteLoop()
        {
            try
            {
                await _connectionEstablished.Task;  // to be able to add messages to queue even before the communicator is ready

                foreach (var message in outputMessagesQueue.GetConsumingEnumerable(cancellationTokenSource.Token))
                {
                    await writer.WriteLineAsync(message);
                    await writer.FlushAsync();

                    // todo: Logically, this is the best place to report on sending a message,
                    // but I don't want anything other than the communicator to be in this thread
                }
            }
            catch (Exception ex)
            {
                OnConnectionBroked($"Error occurred while trying to send (write) a message: {ex}");
            }
        }

        private void ExecutionLoop()
        {
            foreach (var message in inputMessagesQueue.GetConsumingEnumerable(cancellationTokenSource.Token))
            {
                try
                {
                    MessageReceived?.Invoke(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occurred while trying to execute a message, but thread is not terminated: {ex}");
                }
            }            
        }

        public virtual void Dispose()
        {
            if (IsOperational)
                ConnectionBroked?.Invoke($"Called from 'InterprocessCommunicator_Base.Dispose'");

            IsOperational = false;
            cancellationTokenSource?.Cancel();

            reader?.Dispose();
            writer?.Dispose();

            inputMessagesQueue?.Dispose();
            outputMessagesQueue?.Dispose();

            cancellationTokenSource?.Dispose();
        }
    }
}