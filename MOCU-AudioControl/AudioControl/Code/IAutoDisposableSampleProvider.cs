using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioControl.Daemon
{
    public interface IAutoDisposableSampleProvider
    {
        event Action PlaybackCompleted;
    }
}
