using System.Collections.Concurrent;
using System.IO.Pipes;
using System.IO;
using System;
using System.Threading;
using System.Threading.Tasks;
using CommonUtilitiesNamespace;

namespace AudioControl
{
    /// <summary>
    /// Holds the configuration and state of audio devices used within the application. 
    /// This includes both input and output devices for researchers and participants. 
    /// It tracks current and previous volume levels to ensure proper audio management 
    /// and restoration upon application closure. This class ensures that changes made 
    /// within the application do not affect external system settings permanently.
    /// </summary>
    public class AudioDevicesParameters
    {
        // Device names that don't affect anything outside the app
        public string audioOutputDeviceName_Researcher { get; set; }
        public string audioOutputDeviceName_Participant { get; set; }
        public string audioInputDeviceName_Researcher { get; set; }
        public string audioInputDeviceName_Participant { get; set; }

        // Volume levels adjusted according to a specific protocol
        public float currentAudioOutputDeviceVolume_Researcher { get; set; }
        public float currentAudioOutputDeviceVolume_Participant { get; set; }
        public float currentAudioInputDeviceVolume_Researcher { get; set; }
        public float currentAudioInputDeviceVolume_Participant { get; set; }

        // Volume levels before the program was initiated
        public float previousAudioOutputDeviceVolume_Researcher { get; set; }
        public float previousAudioOutputDeviceVolume_Participant { get; set; }
        public float previousAudioInputDeviceVolume_Researcher { get; set; }
        public float previousAudioInputDeviceVolume_Participant { get; set; }

    }

}