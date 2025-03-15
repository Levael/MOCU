


using System;
using System.Collections.Concurrent;


namespace MoogModule.Daemon
{
    public class MoogCommunicator
    {
        private IntervalExecutor _ticker;
        private ConcurrentQueue<CommandPacket> _commandsQueue;

        public MoogCommunicator()
        {
            _ticker = new IntervalExecutor(interval: TimeSpan.FromMilliseconds(1));
            _ticker.OnTick += HandleTick;
        }

        public void StartCommunication()
        {

        }

        private void HandleTick()
        {

        }
    }
}



/*
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace MoogModule.Daemon
{
    public class MoogCommunicator
    {
        protected CancellationTokenSource cancellationTokenSource;

        protected BlockingCollection<string> receivedMessagesQueue;
        protected BlockingCollection<string> toBeSentMessagesQueue;
        protected BlockingCollection<string> sentMessagesQueue;
        protected BlockingCollection<string> debugMessagesQueue;

        public event Action<string> MessageReceived;
        public event Action<string> MessageSent;
        public event Action<string> ErrorOccurred;

        private Task _readLoop;
        private Task _writeLoop;
        private Task _processReceivedMessagesLoop;
        private Task _processSentMessagesLoop;
        private Task _processDebugMessagesLoop;

        public InterprocessCommunicator_Base(string pipeName)
        {
            receivedMessagesQueue   = new BlockingCollection<string>();
            toBeSentMessagesQueue   = new BlockingCollection<string>();
            sentMessagesQueue       = new BlockingCollection<string>();
            debugMessagesQueue      = new BlockingCollection<string>();

            cancellationTokenSource = new CancellationTokenSource();
        }

        public void Start()
        {
            _readLoop                       = Task.Run(() => ReadLoop());
            _writeLoop                      = Task.Run(() => WriteLoop());
            _processReceivedMessagesLoop    = Task.Run(() => ProcessReceivedMessagesLoop());
            _processSentMessagesLoop        = Task.Run(() => ProcessSentMessagesLoop());
            _processDebugMessagesLoop       = Task.Run(() => ProcessDebugMessagesLoop());
        }

        public void Stop(string reason = "unknown")
        {
            cancellationTokenSource?.Cancel();

            receivedMessagesQueue?.Dispose();
            toBeSentMessagesQueue?.Dispose();

            cancellationTokenSource?.Dispose();
        }

        public void SendMessage(string message)
        {
            try
            {
                toBeSentMessagesQueue.Add(message);   
            }
            catch (Exception ex)
            {
                debugMessagesQueue.Add($"Error occurred while trying to execute 'SendMessage(message)', handled without termination: {ex}");
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
                        receivedMessagesQueue.Add(UnprotectString(message), token);
                    else
                        throw new IOException("Got 'null' in 'ReadLoop'");
                }
            }
            catch (Exception ex)
            {
                Stop($"Error occurred while trying to read a message: {ex}");
            }
        }

        private async Task WriteLoop()
        {
            try
            {
                await _connectionEstablished.Task;  // to be able to add messages to queue even before the communicator is ready

                foreach (var message in toBeSentMessagesQueue.GetConsumingEnumerable(cancellationTokenSource.Token))
                {
                    await writer.WriteLineAsync(ProtectString(message));
                    await writer.FlushAsync();

                    sentMessagesQueue.Add(message);
                }
            }
            catch (Exception ex)
            {
                Stop($"Error occurred while trying to write a message: {ex}");
            }
        }

        private void ProcessReceivedMessagesLoop()
        {
            foreach (var message in receivedMessagesQueue.GetConsumingEnumerable(cancellationTokenSource.Token))
            {
                try
                {
                    MessageReceived?.Invoke(message);
                }
                catch (Exception ex)
                {
                    debugMessagesQueue.Add($"Error occurred while trying to execute 'MessageReceived?.Invoke(message)', handled without termination: {ex}");
                }
            }
        }

        private void ProcessSentMessagesLoop()
        {
            foreach (var message in sentMessagesQueue.GetConsumingEnumerable(cancellationTokenSource.Token))
            {
                try
                {
                    MessageSent?.Invoke(message);
                }
                catch (Exception ex)
                {
                    debugMessagesQueue.Add($"Error occurred while trying to execute 'MessageSent?.Invoke(message)', handled without termination: {ex}");
                }
            }
        }

        private void ProcessDebugMessagesLoop()
        {
            foreach (var message in debugMessagesQueue.GetConsumingEnumerable(cancellationTokenSource.Token))
            {
                try
                {
                    ErrorOccurred?.Invoke(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occurred while trying to execute 'ProcessDebugMessagesLoop', handled without termination: {ex}");
                }
            }
        }
    }
}
*/