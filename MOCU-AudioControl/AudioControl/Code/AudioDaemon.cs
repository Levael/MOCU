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
            _hostAPI = new AudioDaemonSideBridge(communicator);
        }

        public void Run()
        {
            _hostAPI.StartCommunication();
        }

        private void Terminate(string reason)
        {
            TerminateDaemon?.Invoke(reason);
        }
    }
}