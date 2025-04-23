using System;
using System.Collections.Generic;

using MoogModule;
using DaemonsRelated.DaemonPart;
using InterprocessCommunication;
using DaemonsRelated;


namespace MoogModule.Daemon
{
    public class MoogDaemonSideBridge : MoogHost_API, IHostAPI
    {
        public event Action<ConnectParameters> Connect;
        public event Action Engage;
        public event Action Disengage;
        public event Action Reset;
        public event Action StartReceivingFeedback;
        public event Action StopReceivingFeedback;
        public event Action<MoveToPointParameters> MoveToPoint;
        public event Action<MoveByTrajectoryParameters> MoveByTrajectory;

        public event Action<string> TerminateDaemon;

        private IInterprocessCommunicator _communicator;

        public MoogDaemonSideBridge(IInterprocessCommunicator communicator)
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

        // ########################################################################################

        public void FeedbackForTimeRange(IEnumerable<DofParameters> parameters)
        {
            var moogDataTransferObject = new MoogDataTransferObject() { FeedbackCoordinates = parameters };
            var json = JsonHelper.SerializeJson(moogDataTransferObject);
            _communicator.SendMessage(json);
        }

        public void SingleFeedback(MoogRealTimeState state)
        {
            var moogDataTransferObject = new MoogDataTransferObject() { State = state };
            var json = JsonHelper.SerializeJson(moogDataTransferObject);
            _communicator.SendMessage(json);
        }

        // ########################################################################################

        private void HandleIncomingMessage(string message)
        {
            Console.WriteLine($"Got message: {message}");

            try
            {
                var dataTransferObject = JsonHelper.DeserializeJson<MoogDataTransferObject>(message);

                // ................................................................................

                // CUSTOM MESSAGE
                if (!String.IsNullOrEmpty(dataTransferObject.CustomMessage))
                    Console.WriteLine($"Custom message in 'HandleIncomingMessage': {dataTransferObject.CustomMessage}");

                // TERMINATION COMMAND
                if (dataTransferObject.DoTerminateTheDaemon)
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