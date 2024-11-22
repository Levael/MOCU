using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DaemonsRelated;
using InterprocessCommunication;


namespace AudioModule.Daemon
{
    public class AudioDaemon
    {
        public event Action<string> TerminateDaemon;

        private AudioDaemonSideBridge _hostAPI;

        public AudioDaemon(IInterprocessCommunicator communicator)
        {
            Console.WriteLine("start of AudioDaemon constructor");
            _hostAPI = new AudioDaemonSideBridge(communicator);
            Console.WriteLine("end of AudioDaemon constructor");
        }

        public void Run()
        {
            Console.WriteLine("start of AudioDaemon Run");
            _hostAPI.StartCommunication();
            Console.WriteLine("end of AudioDaemon Run");
        }

        private void Terminate(string reason)
        {
            TerminateDaemon?.Invoke(reason);
        }
    }
}
