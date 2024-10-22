using InterprocessCommunication;


namespace DaemonsRelated.Audio
{
    public class AudioHostBridge : IHostAPI
    {
        private IInterprocessCommunicator _communicator;

        public AudioHostBridge(IInterprocessCommunicator communicator)
        {
            _communicator = communicator;
        }

        public void StartCommunication()
        {
            // method is async, but there is no need to use 'await'
            _communicator.Start();
        }

        public void TestMethod()
        {

        }

        private void SendMessage()
        {

        }
    }
}