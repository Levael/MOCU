using UnityEngine;


public class ForTests : MonoBehaviour, IFullyControllable
{
    public bool IsComponentReady {  get; private set; }

    private DebugTabHandler _debugTabHandler;
    private AudioHandler _audioHandler;
    private CedrusHandler _cerusHandler;

    //private int temp_counter = 0;

    public void ControllableAwake()
    {
        _debugTabHandler = GetComponent<DebugTabHandler>();
        _audioHandler = GetComponent<AudioHandler>();
        //_cerusHandler = GetComponent<CedrusHandler>();
    }

    public void ControllableStart()
    {
        /*_debugTabHandler.testBtn1Clicked += (eventObj) =>
        {
            if (_cerusHandler.stateTracker.Status != ModuleStatus.FullyOperational)
                _cerusHandler.TryConnect();
        };*/

        // sending lots of commands simultaneously together (stresstest)
        /*_debugTabHandler.testBtn1Clicked += (eventObj) =>
        {
            print($"sended");
            _audioHandler.
            //_audioHandler.SendTestAudioSignalToDevice("Speakers (Realtek High Definition Audio)");
        };*/
        IsComponentReady = true;
    }

    public void ControllableUpdate()
    {
        /*if (_audioHandler.stateTracker.Status == ModuleStatus.FullyOperational && temp_counter++ < 300)
            _audioHandler.SendTestAudioSignalToDevice("Speakers (Realtek High Definition Audio)");*/
    }
}
