using InterprocessCommunication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;


namespace DaemonsRelated
{
    public class HostSideDaemonContainer
    {
        public bool isRunning => _process?.HasExited == false && _communicator?.IsOperational == true;
        public DaemonType type;
        public string name;

        public event Action<string> MessageReceived;
        public event Action<string> MessageSent;

        private string _fullPath;
        private bool _isHidden;

        private Process _process;
        private IInterprocessCommunicator _communicator;

        public HostSideDaemonContainer(DaemonType type, string fullPath, bool isHidden)
        {
            this.type = type;
            _fullPath = fullPath;
            _isHidden = isHidden;
            name = $"{type}Daemon";

            var processInfo = new ProcessStartInfo()
            {
                FileName = _fullPath,
                Arguments = $"{Process.GetCurrentProcess().Id} {name} {_isHidden}",
                UseShellExecute = !_isHidden,
                RedirectStandardOutput = false,
                CreateNoWindow = _isHidden
            };

            _process = new Process() { StartInfo = processInfo };
            _communicator = new InterprocessCommunicator_UnityServer(name);

            // for debug purposes
            _communicator.MessageReceived += message => this.MessageReceived?.Invoke(message);
            _communicator.MessageSent += message => this.MessageSent?.Invoke(message);
        }

        public async void Start()
        {
            // 'communicator' must be before 'process' (because it's on the sever side. On the client, it's the opposite)

            try
            {
                var communicator = _communicator as InterprocessCommunicator_Server;
                _ = Task.Run(() => communicator.Start());
                await communicator.WaitForServerReadyForClientConnectionAsync();
                _ = Task.Run(() => _process.Start());
            }
            catch
            {
                Stop();
            }
        }

        public void Stop()
        {
            _communicator.Dispose();

            if (_isHidden)
            {
                _process.Kill();
                _process.Close();
            }

            // If not -- the console does not close itself and you can read the error message(s)
        }

        public IInterprocessCommunicator GetCommunicator()
        {
            return _communicator;
        }
    }
}