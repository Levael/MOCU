using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AudioModule
{
    public class AudioDeviceData
    {
        public string id { get; set; }
        public float volume { get; set; }   // from 0 to 100
        public AudioDeviceType type { get; set; }
        public AudioDeviceConnectionStatus connectionStatus { get; set; }
    }
}