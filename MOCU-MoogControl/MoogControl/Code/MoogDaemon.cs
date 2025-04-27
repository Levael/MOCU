using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using DaemonsRelated.DaemonPart;


namespace MoogModule.Daemon
{
    public class MoogDaemon : IDaemonLogic
    {
        public event Action<string> TerminateDaemon;

        private readonly MoogDaemonSideBridge _hostAPI;
        private readonly IntervalExecutor _intervalExecutor;
        private readonly MoogMachineCommunicator _moogMachineCommunicator;

        private readonly ConcurrentQueue<CommandPacket> _commandsForMoog;
        private readonly MoogRealTimeState _moogRealTimeState;

        private DofParameters _startPosition;
        private double _maxAcceleration;
        private bool _doSendFeedback = false;

        public MoogDaemon(MoogDaemonSideBridge hostAPI)
        {
            _commandsForMoog = new();
            _hostAPI = hostAPI;
            _moogMachineCommunicator = new MoogMachineCommunicator();
            _moogRealTimeState = new MoogRealTimeState();
            _intervalExecutor = new IntervalExecutor(TimeSpan.FromMilliseconds(1));

            _hostAPI.TerminateDaemon += message => Console.WriteLine("Got command to terminate the daemon.");
            _moogMachineCommunicator.PacketReceived += HandleReceivedPacket;
            _intervalExecutor.OnTick += ExecuteEveryTick;

            _hostAPI.Connect += HandleConnectCommand;
            _hostAPI.Engage += HandleEngageCommand;
            _hostAPI.Disengage += HandleDisengageCommand;
            _hostAPI.Reset += HandleResetCommand;
            _hostAPI.StartReceivingFeedback += HandleStartReceivingFeedbackCommand;
            _hostAPI.StopReceivingFeedback += HandleStopReceivingFeedbackCommand;
            _hostAPI.MoveToPoint += HandleMoveToPointCommand;
            _hostAPI.MoveByTrajectory += HandleMoveByTrajectoryCommand;
        }

        

        public void DoBeforeExit()
        {
            throw new NotImplementedException();
        }

        public void Run()
        {
            _hostAPI.StartCommunication();
            _intervalExecutor.Start();
        }

        // ########################################################################################

        private void HandleConnectCommand(ConnectParameters parameters)
        {
            try
            {
                _startPosition                          = parameters.StartPosition;
                //_moogRealTimeState.LastCommandPosition  = _startPosition;
                _maxAcceleration                        = parameters.MaxAcceleration;

                _moogMachineCommunicator.Connect(parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Handled error ucurred in 'HandleConnectCommand': {ex}");
            }
        }

        private void HandleMoveByTrajectoryCommand(MoveByTrajectoryParameters parameters)
        {
            throw new NotImplementedException();
        }

        // todo: report an error if Queue is not empty when adding with delay (safety reason)
        private void HandleMoveToPointCommand(MoveToPointParameters parameters)
        {
            try
            {
                DateTime now = DateTime.UtcNow;
                TimeSpan delay = parameters.ScheduledTime - now;

                if (delay > TimeSpan.Zero)
                {
                    Task.Run(async () =>
                    {
                        await Task.Delay(delay);

                        if (_commandsForMoog.Any())
                            throw new Exception("Can't add new position with delay -- queue is not empty");
                        else
                            _commandsForMoog.Enqueue(CommandPackets.NewPosition(parameters.Coordinate));
                    });
                }
                else
                    _commandsForMoog.Enqueue(CommandPackets.NewPosition(parameters.Coordinate));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Handled error ucurred in 'HandleMoveToPointCommand': {ex}");
            }
        }

        private void HandleStopReceivingFeedbackCommand()
        {
            _doSendFeedback = false;
        }

        private void HandleStartReceivingFeedbackCommand()
        {
            _doSendFeedback = true;
        }

        private void HandleResetCommand()
        {
            try
            {
                _commandsForMoog.Enqueue(CommandPackets.Reset());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Handled error ucurred in 'HandleResetCommand': {ex}");
            }
        }

        private void HandleDisengageCommand()
        {
            try
            {
                _commandsForMoog.Enqueue(CommandPackets.Disengage());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Handled error ucurred in 'HandleDisengageCommand': {ex}");
            }
        }

        private void HandleEngageCommand()
        {
            try
            {
                _commandsForMoog.Enqueue(CommandPackets.Engage(_startPosition));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Handled error ucurred in 'HandleEngageCommand': {ex}");
            }
        }



        private void HandleReceivedPacket(byte[] data)
        {
            try
            {
                var parsedMessage = PacketSerializer.Deserialize(data);

                _moogRealTimeState.EncodedMachineState = parsedMessage.EncodedMachineState;
                _moogRealTimeState.Position = parsedMessage.Parameters;

                // TODO:
                // - faults
                // - calculate velocity and acceleration
                // - maybe add safety checks

                if (_doSendFeedback)
                    _hostAPI.SingleFeedback(_moogRealTimeState);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Handled error ucurred in 'HandleReceivedPacket': {ex}");
            }
        }

        private void ExecuteEveryTick()
        {
            if (_commandsForMoog.TryDequeue(out CommandPacket command))
            {
                var packet = PacketSerializer.Serialize(command);
                _moogMachineCommunicator.SendPacket(packet);
                //_moogRealTimeState.LastCommandPosition = command.Parameters;
            }
            else
            {
                var KeepAlivePacket = PacketSerializer.Serialize(CommandPackets.NewPosition(_moogRealTimeState.Position));
                _moogMachineCommunicator.SendPacket(KeepAlivePacket);
            }  
        }
    }
}