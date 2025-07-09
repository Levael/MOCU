using System;

using DaemonsRelated;
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
            try
            {
                var DTO = JsonHelper.DeserializeJson<ChartsDataTransferObject>(message);

                // ................................................................................

                // CUSTOM MESSAGE
                if (!String.IsNullOrEmpty(DTO.CustomMessage))
                    Console.WriteLine($"Custom message in 'HandleIncomingMessage': {DTO.CustomMessage}");

                // TERMINATION COMMAND
                if (DTO.DoTerminateTheDaemon)
                    TerminateDaemon?.Invoke("Got command from host to terminate the daemon");

                // ................................................................................

                // CONNECT
                if (dataTransferObject.ConnectCommand)
                    Connect?.Invoke(dataTransferObject.ConnectParameters);

                // ENGAGE
                if (dataTransferObject.EngageCommand)
                    Engage?.Invoke();

                // DISENGAGE
                if (dataTransferObject.DisengageCommand)
                    Disengage?.Invoke();

                // RESET
                if (dataTransferObject.ResetCommand)
                    Reset?.Invoke();

                // DO RECEIVE FEEDBACK
                if (dataTransferObject.DoReceiveFeedback)
                    StartReceivingFeedback?.Invoke();
                else
                    StopReceivingFeedback?.Invoke();

                // MOVE TO POINT
                if (dataTransferObject.MoveToPointCommand)
                    MoveToPoint?.Invoke(dataTransferObject.MoveToPointParameters);

                // MOVE BY TRAJECTORY
                if (dataTransferObject.MoveByTrajectoryCommand)
                    MoveByTrajectory?.Invoke(dataTransferObject.MoveByTrajectoryParameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in 'HandleIncomingMessage': {ex.Message}");
            }

        }
    }
}