using System.Collections.Concurrent;
using System.IO.Pipes;
using System.IO;
using System;
using System.Threading;
using System.Threading.Tasks;
using CommonUtilitiesNamespace;
using System.Reflection;

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
        private string _audioOutputDeviceName_Researcher;
        private string _audioOutputDeviceName_Participant;
        private string _audioInputDeviceName_Researcher;
        private string _audioInputDeviceName_Participant;

        // Volume levels adjusted according to a specific protocol
        private float _currentAudioOutputDeviceVolume_Researcher;
        private float _currentAudioOutputDeviceVolume_Participant;
        private float _currentAudioInputDeviceVolume_Researcher;
        private float _currentAudioInputDeviceVolume_Participant;

        // Volume levels before the program was initiated
        private float _previousAudioOutputDeviceVolume_Researcher;
        private float _previousAudioOutputDeviceVolume_Participant;
        private float _previousAudioInputDeviceVolume_Researcher;
        private float _previousAudioInputDeviceVolume_Participant;





        public string audioOutputDeviceName_Researcher
        {
            get => _audioOutputDeviceName_Researcher;
            set
            {
                _audioOutputDeviceName_Researcher = value;
            }
        }

        
        public string audioOutputDeviceName_Participant
        {
            get => _audioOutputDeviceName_Participant;
            set
            {
                // Кастомная логика для этого свойства.
                _audioOutputDeviceName_Participant = value;
            }
        }

        
        public string audioInputDeviceName_Researcher
        {
            get => _audioInputDeviceName_Researcher;
            set
            {
                // Кастомная логика для этого свойства.
                _audioInputDeviceName_Researcher = value;
            }
        }

        
        public string audioInputDeviceName_Participant
        {
            get => _audioInputDeviceName_Participant;
            set
            {
                // Кастомная логика для этого свойства.
                _audioInputDeviceName_Participant = value;
            }
        }

        
        public float currentAudioOutputDeviceVolume_Researcher
        {
            get => _currentAudioOutputDeviceVolume_Researcher;
            set
            {
                // Кастомная логика для этого свойства.
                _currentAudioOutputDeviceVolume_Researcher = value;
            }
        }

        
        public float currentAudioOutputDeviceVolume_Participant
        {
            get => _currentAudioOutputDeviceVolume_Participant;
            set
            {
                // Кастомная логика для этого свойства.
                _currentAudioOutputDeviceVolume_Participant = value;
            }
        }

        
        public float currentAudioInputDeviceVolume_Researcher
        {
            get => _currentAudioInputDeviceVolume_Researcher;
            set
            {
                // Кастомная логика для этого свойства.
                _currentAudioInputDeviceVolume_Researcher = value;
            }
        }

        
        public float currentAudioInputDeviceVolume_Participant
        {
            get => _currentAudioInputDeviceVolume_Participant;
            set
            {
                // Кастомная логика для этого свойства.
                _currentAudioInputDeviceVolume_Participant = value;
            }
        }

        
        public float previousAudioOutputDeviceVolume_Researcher
        {
            get => _previousAudioOutputDeviceVolume_Researcher;
            set
            {
                // Кастомная логика для этого свойства.
                _previousAudioOutputDeviceVolume_Researcher = value;
            }
        }

        
        public float previousAudioOutputDeviceVolume_Participant
        {
            get => _previousAudioOutputDeviceVolume_Participant;
            set
            {
                // Кастомная логика для этого свойства.
                _previousAudioOutputDeviceVolume_Participant = value;
            }
        }

        
        public float previousAudioInputDeviceVolume_Researcher
        {
            get => _previousAudioInputDeviceVolume_Researcher;
            set
            {
                // Кастомная логика для этого свойства.
                _previousAudioInputDeviceVolume_Researcher = value;
            }
        }

        
        public float previousAudioInputDeviceVolume_Participant
        {
            get => _previousAudioInputDeviceVolume_Participant;
            set
            {
                // Кастомная логика для этого свойства.
                _previousAudioInputDeviceVolume_Participant = value;
            }
        }






        /// <summary>
        /// Updates properties of the current instance with values from another instance if they are different.
        /// </summary>
        /// <param name="updatedParams">The instance of AudioDevicesParameters with updated values (got from client JSON)</param>
        /// <returns>Return status of operation. True if everything is ok, and false if any error uccurred</returns>
        public bool Update(AudioDevicesParameters updatedParameters)
        {
            try
            {
                foreach (PropertyInfo property in typeof(AudioDevicesParameters).GetProperties())
                {
                    var currentValue = property.GetValue(this);     // 'this' == current 'AudioDevicesParameters' class
                    var newValue = property.GetValue(updatedParameters);

                    if (!Equals(currentValue, newValue))
                    {
                        property.SetValue(this, newValue);
                    }
                }

                return true;
            }
            catch { return false; }
        }
    }


    

}