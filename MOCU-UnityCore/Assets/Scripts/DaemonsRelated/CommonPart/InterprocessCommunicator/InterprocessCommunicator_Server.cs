using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;


namespace InterprocessCommunication
{
    public class InterprocessCommunicator_Server : InterprocessCommunicator_Base
    {
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
            var readConnectionTask = readPipe.WaitForConnectionAsync();
            var writeConnectionTask = writePipe.WaitForConnectionAsync();

            await Task.WhenAll(readConnectionTask, writeConnectionTask);
            base.Start();
        }

        public override void Dispose()
        {
            base.Dispose();
            readPipe.Dispose();
            writePipe.Dispose();
        }
    }
}