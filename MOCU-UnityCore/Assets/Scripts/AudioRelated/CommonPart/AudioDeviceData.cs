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
        public Guid id { get; set; }
        public float volume { get; set; }
        public AudioDeviceType type { get; set; }
        public AudioDeviceConnectionStatus connectionStatus { get; set; }
    }
}