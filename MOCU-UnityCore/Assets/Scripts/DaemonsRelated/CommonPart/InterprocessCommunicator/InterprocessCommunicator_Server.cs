using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;


namespace InterprocessCommunication
{
    public class InterprocessCommunicator_Server : InterprocessCommunicator_Base
    {
        private readonly TaskCompletionSource<bool> _serverIsReadyForClientConnection = new();
        public Task WaitForServerReadyForClientConnectionAsync() => _serverIsReadyForClientConnection.Task;

        private NamedPipeServerStream readPipe;
        private NamedPipeServerStream writePipe;

        public InterprocessCommunicator_Server(string pipeName) : base(pipeName)
        {
            readPipe = new NamedPipeServerStream(pipeName_clientWritesServerReads, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances);
            writePipe = new NamedPipeServerStream(pipeName_serverWritesClientReads, PipeDirection.Out, NamedPipeServerStream.MaxAllowedServerInstances);

            reader = new StreamReader(readPipe);
            writer = new StreamWriter(writePipe);
        }

        public override async void Start()
        {
            try
            {
                if (IsOperational)
                    throw new InvalidOperationException("The communicator is already running.");

                var readConnectionTask = readPipe.WaitForConnectionAsync();
                var writeConnectionTask = writePipe.WaitForConnectionAsync();

                // Now client side may try to connect
                _serverIsReadyForClientConnection.SetResult(true);

                await Task.WhenAll(readConnectionTask, writeConnectionTask);
                base.Start();
            }
            catch
            {
                Dispose();
            }
        }

        public override void Dispose()
        {
            try { base.Dispose(); } catch { }
            try { readPipe.Dispose(); } catch { }
            try { writePipe.Dispose(); } catch { }
        }
    }
}