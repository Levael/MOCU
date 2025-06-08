using System;

using DaemonsRelated.DaemonPart;
using InterprocessCommunication;


namespace ChartsModule.Daemon
{
    public class ChartsDaemonSideBridge : ChartsHost_API, IHostAPI
    {
        public event Action<string> TerminateDaemon;

        private IInterprocessCommunicator _communicator;

        public ChartsDaemonSideBridge(IInterprocessCommunicator communicator)
        {
            _communicator = communicator;

            _communicator.MessageReceived       += message => HandleIncomingMessage(message);
            _communicator.MessageSent           += message => Console.WriteLine($"Sent message to host: {message}");
            _communicator.ConnectionEstablished += message => Console.WriteLine($"Connection established. {message}");
            _communicator.ConnectionBroked      += message => Console.WriteLine($"Connection broked. {message}");
            _communicator.ErrorOccurred         += message => Console.WriteLine($"Occurred an error in communicator. {message}");
        }

        public void StartCommunication()
        {
            _communicator.Start();
        }

        // ........

        private void HandleIncomingMessage(string message)
        {
            Console.WriteLine($"Got message from host: {message}");
        }
    }
}