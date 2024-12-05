using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DaemonsRelated;
using DaemonsRelated.DaemonPart;


/*
 * Currently 'PlayClips' has no feedback. Maybe later add event 'ClipPlayed' or smth like that
 * 'HandleIntercoms' always returns all currently running intercoms so host side is always up to date
 */


namespace AudioModule.Daemon
{
    public class AudioDaemon : IDaemonLogic
    {
        public event Action<string> TerminateDaemon;

        private AudioDaemonSideBridge _hostAPI;
        private Dictionary<(Guid fromDevice, Guid toDevice), AudioIntercomData> _intercoms;

        public AudioDaemon(AudioDaemonSideBridge hostAPI)
        {
            _hostAPI = hostAPI;

            _hostAPI.PlayClips += PlayAudioClips;     // (input) => { Console.WriteLine("PlayClips logic"); };
            _hostAPI.UpdateIntercomStates += HandleIntercoms;     // (input) => { Console.WriteLine("PlayClips logic"); };
            _hostAPI.UpdateDevicesData += (input) => { Console.WriteLine("UpdateDevicesData logic"); };
            _hostAPI.UpdateClipsData   += (input) => { Console.WriteLine("UpdateClipsData logic"); };
        }

        public void Run()
        {
            _hostAPI.StartCommunication();
        }

        public void DoBeforeExit()
        {
            // todo: undo every changes made to OS (audio devices volume etc)
        }

        // ########################################################################################

        private void PlayAudioClips(IEnumerable<PlayAudioClipCommand> clips)
        {
            foreach (var clip in clips)
                PlayAudioClip(clip);
        }

        private void HandleIntercoms(IEnumerable<AudioIntercomData> intercoms)
        {
            foreach (var intercom in intercoms)
                HandleIntercom(intercom);

            _hostAPI.IntercomStatesChanged(_intercoms.Values);
        }

        private void PlayAudioClip(PlayAudioClipCommand clipData)
        {
            // temp
            Console.WriteLine($"Played clip '{clipData.ClipData.name}'");
        }

        private void HandleIntercom(AudioIntercomData intercom)
        {
            // temp
            Console.WriteLine($"Handled intercom. It's now is ON: {intercom.isOn}");
        }
    }
}