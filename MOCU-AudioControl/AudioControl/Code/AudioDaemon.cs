using DaemonsRelated.DaemonPart;


/*
 * Currently 'PlayClips' has no feedback. Maybe later add event 'ClipPlayed' or smth like that.
 * 'HandleIntercoms', 'UpdateDevicesData' and 'UpdateClipsData' always return all currently running intercoms so host side is always up to date. 
 * Even though the request may contain multiple operations of the same type, the response will always be singular for each part.
 */


namespace AudioModule.Daemon
{
    public class AudioDaemon : IDaemonLogic
    {
        public event Action<string> TerminateDaemon;

        private DevicesManager _devicesManager;
        private IntercomsManager _intercomsManager;
        private ClipsManager _clipsManager;
        private AudioDaemonSideBridge _hostAPI;

        public AudioDaemon(AudioDaemonSideBridge hostAPI)
        {
            _devicesManager     = new();
            _intercomsManager   = new(_devicesManager);
            _clipsManager       = new(_devicesManager);

            _devicesManager.ChangesOccurred     += OnDevicesChanged;
            _intercomsManager.ChangesOccurred   += OnIntercomsChanged;
            _clipsManager.ChangesOccurred       += OnClipsChanged;

            _hostAPI = hostAPI;

            _hostAPI.PlayClips              += PlayAudioClips;
            _hostAPI.UpdateIntercomStates   += HandleIntercoms;
            _hostAPI.UpdateDevicesData      += UpdateDevicesData;
            _hostAPI.UpdateClipsData        += UpdateClipsData;
        }

        public void Run()
        {
            _hostAPI.StartCommunication();
        }

        public void DoBeforeExit()
        {
            // todo: undo every changes made to OS (audio devices Volume etc)
        }

        // ########################################################################################

        private void PlayAudioClips(IEnumerable<PlayAudioClipCommand> clips)
        {
            foreach (var clip in clips)
                _clipsManager.PlayAudioClip(clip);
        }

        private void HandleIntercoms(IEnumerable<AudioIntercomData> intercoms)
        {
            foreach (var intercom in intercoms)
                _intercomsManager.HandleIntercomCommand(intercom);
        }

        private void UpdateDevicesData(IEnumerable<AudioDeviceData> devices)
        {
            // todo (request from Unity)
            Console.WriteLine("UpdateDevicesData logic");
        }

        private void UpdateClipsData(IEnumerable<AudioClipData> clips)
        {
            foreach (var clip in clips)
                _clipsManager.UpdateClipData(clip);
        }

        private void OnDevicesChanged()
        {
            Console.WriteLine("Some device(s) changed");
            _hostAPI.DevicesDataChanged(_devicesManager.GetInputDevicesData().Concat(_devicesManager.GetOutputDevicesData()));
        }

        private void OnIntercomsChanged()
        {
            Console.WriteLine("Some intercom(s) changed");
            _hostAPI.IntercomStatesChanged(_intercomsManager.GetIntercomsData());
        }

        private void OnClipsChanged()
        {
            Console.WriteLine("Some clip(s) changed");
            _hostAPI.ClipsDataChanged(_clipsManager.GetClipsData());
        }
    }
}