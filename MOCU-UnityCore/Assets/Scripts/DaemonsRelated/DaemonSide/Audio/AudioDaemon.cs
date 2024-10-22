using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DaemonsRelated.Audio
{
    public class AudioDaemon : IDaemonLogic
    {
        private AudioHostBridge _hostAPI;

        public AudioDaemon(AudioHostBridge hostAPI)
        {
            _hostAPI = hostAPI;
        }

        public void Run()
        {
            _hostAPI.StartCommunication();
        }
    }
}
