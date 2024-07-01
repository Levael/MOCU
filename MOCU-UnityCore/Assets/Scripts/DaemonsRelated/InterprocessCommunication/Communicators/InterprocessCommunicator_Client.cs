using System.IO.Pipes;
using System.IO;
using System.Threading.Tasks;

namespace InterprocessCommunication
{
    public class InterprocessCommunicator_Client : InterprocessCommunicator_Base
    {
        private NamedPipeClientStream readPipe;
        private NamedPipeClientStream writePipe;

        public InterprocessCommunicator_Client(string pipeName) : base(pipeName)
        {
            readPipe = new NamedPipeClientStream(".", readPipeName, PipeDirection.In);
            writePipe = new NamedPipeClientStream(".", writePipeName, PipeDirection.Out);

            reader = new StreamReader(readPipe);
            writer = new StreamWriter(writePipe);
        }

        public async Task StartAsync()
        {
            await readPipe.ConnectAsync();
            await writePipe.ConnectAsync();
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

