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
        protected StreamWriter writer;
        protected StreamReader reader;
        protected CancellationTokenSource cancellationTokenSource;
        protected BlockingCollection<string> inputMessagesQueue;
        protected BlockingCollection<string> outputMessagesQueue;

        protected string pipeName_clientWritesServerReads;
        protected string pipeName_serverWritesClientReads;

        public event Action<string> MessageReceived;

        public InterprocessCommunicator_Base(string pipeName)
        {
            cancellationTokenSource = new CancellationTokenSource();
            outputMessagesQueue = new BlockingCollection<string>();

            pipeName_clientWritesServerReads = $"{pipeName}C2S";
            pipeName_serverWritesClientReads = $"{pipeName}S2C";
        }

        public virtual void Start()
        {
            Task.Run(() => ReadLoop(cancellationTokenSource.Token));
            Task.Run(() => WriteLoop(cancellationTokenSource.Token));
            Task.Run(() => ExecutionLoop(cancellationTokenSource.Token));
        }

        public void SendMessage(string message)
        {
            try
            {
                outputMessagesQueue.Add(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while trying to send (API) a message: {ex}");
                outputMessagesQueue.CompleteAdding();
            }
            
        }

        private async Task ReadLoop(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var message = await reader.ReadLineAsync();

                    if (message != null)
                        inputMessagesQueue.Add(message, token);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while trying to read a message: {ex}");
                inputMessagesQueue.CompleteAdding();
            }
        }

        private async Task WriteLoop(CancellationToken token)
        {
            try
            {
                foreach (var message in outputMessagesQueue.GetConsumingEnumerable(token))
                {
                    await writer.WriteLineAsync(message);
                    await writer.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while trying to send (write) a message: {ex}");
                outputMessagesQueue.CompleteAdding();
            }
        }

        private void ExecutionLoop(CancellationToken token)
        {
            foreach (var message in inputMessagesQueue.GetConsumingEnumerable(token))
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
            cancellationTokenSource.Cancel();

            inputMessagesQueue.CompleteAdding();
            outputMessagesQueue.CompleteAdding();

            inputMessagesQueue.Dispose();
            outputMessagesQueue.Dispose();

            writer?.Dispose();
            reader?.Dispose();

            cancellationTokenSource.Dispose();
        }
    }
}