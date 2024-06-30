using System.IO.Pipes;
using System.IO;

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

        public override void Start()
        {
            readPipe.Connect();
            writePipe.Connect();
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

