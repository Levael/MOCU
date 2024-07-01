using System.IO.Pipes;


namespace InterprocessCommunication
{
    public class InterprocessCommunicator_Server : InterprocessCommunicator_Base
    {
        private NamedPipeServerStream readPipe;
        private NamedPipeServerStream writePipe;

        public InterprocessCommunicator_Server(string pipeName) : base(pipeName)
        {
            readPipe = new NamedPipeServerStream(readPipeName, PipeDirection.In);
            writePipe = new NamedPipeServerStream(writePipeName, PipeDirection.Out);

            reader = new StreamReader(readPipe);
            writer = new StreamWriter(writePipe);
        }

        public async Task StartAsync()
        {
            await readPipe.WaitForConnectionAsync();
            await writePipe.WaitForConnectionAsync();
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
