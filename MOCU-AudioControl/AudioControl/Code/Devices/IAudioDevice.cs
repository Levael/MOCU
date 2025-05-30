﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AudioModule.Daemon
{
    public interface IAudioDevice
    {
        Guid Id         { get; }
        float Volume    { get; set; }
        void Reinitialize();
    }
}