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
        }

        public void SendMessage(string message)
        {
            outputMessagesQueue.Add(message);
        }

        protected async Task WriteLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var message = outputMessagesQueue.Take(token);
                    await writer.WriteLineAsync(message);
                    await writer.FlushAsync();
                }
                catch (OperationCanceledException)
                {
                    // Operation was canceled, exit the loop
                    break;
                }
            }
        }

        protected async Task ReadLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    string message = await reader.ReadLineAsync();
                    if (message != null)
                    {
                        MessageReceived?.Invoke(message);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Operation was canceled, exit the loop
                    break;
                }
            }
        }

        public virtual void Dispose()
        {
            cancellationTokenSource.Cancel();
            outputMessagesQueue.Dispose();
        }
    }
}