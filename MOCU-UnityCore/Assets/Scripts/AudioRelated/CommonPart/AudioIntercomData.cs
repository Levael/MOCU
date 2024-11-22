using System;


namespace AudioModule
{
    public class AudioIntercomData
    {
        public Guid fromDevice { get; set; }
        public Guid toDevice { get; set; }
        public bool isOn { get; set; }
    }
}