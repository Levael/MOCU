using DaemonsRelated.DaemonPart;
using System;


namespace MoogModule.Daemon
{
    public class MoogDaemon : IDaemonLogic
    {
        public event Action<string> TerminateDaemon;

        private MoogDaemonSideBridge _hostAPI;

        public MoogDaemon(MoogDaemonSideBridge hostAPI)
        {
            _hostAPI = hostAPI;
        }

        public void DoBeforeExit()
        {
            throw new NotImplementedException();
        }

        public void Run()
        {
            _hostAPI.StartCommunication();
        }
    }
}