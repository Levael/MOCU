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
        public event Action<MachineSettings> Connect;
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

        public void Feedback(MoogFeedback feedback)
        {
            var DTO = new MoogDataTransferObject() { Feedback = feedback };
            var json = JsonHelper.SerializeJson(DTO);
            _communicator.SendMessage(json);
        }

        public void State(MoogRealTimeState state)
        {
            var DTO = new MoogDataTransferObject() { State = state };
            var json = JsonHelper.SerializeJson(DTO);
            _communicator.SendMessage(json);
        }

        // ########################################################################################

        private void HandleIncomingMessage(string message)
        {
            Console.WriteLine($"Got message: {message}");

            try
            {
                var DTO = JsonHelper.DeserializeJson<MoogDataTransferObject>(message);

                // ................................................................................

                // CUSTOM MESSAGE
                if (!String.IsNullOrEmpty(DTO.CustomMessage))
                    Console.WriteLine($"Custom message in 'HandleIncomingMessage': {DTO.CustomMessage}");

                // TERMINATION COMMAND
                if (DTO.DoTerminateTheDaemon)
                    TerminateDaemon?.Invoke("Got command from host to terminate the daemon");

                // ................................................................................

                // CONNECT
                if (DTO.ConnectCommand)
                    Connect?.Invoke(DTO.ConnectParameters);

                // ENGAGE
                if (DTO.EngageCommand)
                    Engage?.Invoke();

                // DISENGAGE
                if (DTO.DisengageCommand)
                    Disengage?.Invoke();

                // RESET
                if (DTO.ResetCommand)
                    Reset?.Invoke();

                // DO RECEIVE FEEDBACK
                if (DTO.DoReceiveFeedback)
                    StartReceivingFeedback?.Invoke();
                else
                    StopReceivingFeedback?.Invoke();

                // MOVE TO POINT
                if (DTO.MoveToPointCommand)
                    MoveToPoint?.Invoke(DTO.MoveToPointParameters);

                // MOVE BY TRAJECTORY
                if (DTO.MoveByTrajectoryCommand)
                    MoveByTrajectory?.Invoke(DTO.MoveByTrajectoryParameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in 'HandleIncomingMessage': {ex.Message}");
            }
        }
    }
}