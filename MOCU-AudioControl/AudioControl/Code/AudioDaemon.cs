using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DaemonsRelated;
using DaemonsRelated.DaemonPart;


namespace AudioModule.Daemon
{
    public class AudioDaemon : IDaemonLogic
    {
        public event Action<string> TerminateDaemon;

        private AudioDaemonSideBridge _hostAPI;

        public AudioDaemon(AudioDaemonSideBridge hostAPI)
        {
            _hostAPI = hostAPI;

            _hostAPI.PlayAudioClips     += (input) => { Console.WriteLine("PlayAudioClips logic"); };
            _hostAPI.UpdateAudioDevices += (input) => { Console.WriteLine("UpdateAudioDevices logic"); };
            _hostAPI.UpdateAudioClips   += (input) => { Console.WriteLine("UpdateAudioClips logic"); };
            _hostAPI.StartIntercoms     += (input) => { Console.WriteLine("StartIntercoms logic"); };
            _hostAPI.StopIntercoms      += (input) => { Console.WriteLine("StopIntercoms logic"); };
        }

        public void Run()
        {
            _hostAPI.StartCommunication();
        }

        public void DoBeforeExit()
        {

        }
    }
}