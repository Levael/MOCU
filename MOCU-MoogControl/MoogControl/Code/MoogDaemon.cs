using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading.Tasks;
using DaemonsRelated.DaemonPart;


namespace MoogModule.Daemon
{
    public class MoogDaemon : IDaemonLogic
    {
        public event Action<string> TerminateDaemon;

        private MoogDaemonSideBridge _hostAPI;
        private IntervalExecutor _intervalExecutor;
        private MoogMachineCommunicator _moogMachineCommunicator;

        private ConcurrentQueue<CommandPacket> _commandsForMoog;

        private DofParameters _startPosition;
        private DofParameters _lastPosition;
        private double _maxAcceleration;

        public MoogDaemon(MoogDaemonSideBridge hostAPI)
        {
            _commandsForMoog = new();
            _hostAPI = hostAPI;
            _moogMachineCommunicator = new MoogMachineCommunicator();
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
                _startPosition      = parameters.StartPosition;
                _lastPosition       = _startPosition;
                _maxAcceleration    = parameters.MaxAcceleration;

                _moogMachineCommunicator.Connect(parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ucurred in 'HandleConnectCommand': {ex}");
            }
        }

        private void HandleMoveByTrajectoryCommand(MoveByTrajectoryParameters parameters)
        {
            throw new NotImplementedException();
        }

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
                        _commandsForMoog.Enqueue(CommandPackets.NewPosition(parameters.Coordinate));
                    });
                }
                else
                    _commandsForMoog.Enqueue(CommandPackets.NewPosition(parameters.Coordinate));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ucurred in 'HandleMoveToPointCommand': {ex}");
            }
        }

        private void HandleStopReceivingFeedbackCommand()
        {
            throw new NotImplementedException();
        }

        private void HandleStartReceivingFeedbackCommand()
        {
            throw new NotImplementedException();
        }

        private void HandleResetCommand()
        {
            try
            {
                _commandsForMoog.Enqueue(CommandPackets.Reset());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ucurred in 'HandleResetCommand': {ex}");
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
                Console.WriteLine($"Error ucurred in 'HandleDisengageCommand': {ex}");
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
                Console.WriteLine($"Error ucurred in 'HandleEngageCommand': {ex}");
            }
        }



        private void HandleReceivedPacket(byte[] data)
        {
            try
            {
                var parsedMessage = PacketSerializer.Deserialize(data);
                ResponsePacketParser.Test(parsedMessage);
            }
            catch { }
        }

        private void ExecuteEveryTick()
        {
            while (_commandsForMoog.TryDequeue(out CommandPacket command))
            {
                var packet = PacketSerializer.Serialize(command);
                _moogMachineCommunicator.SendPacket(packet);
                _lastPosition = command.Parameters;
            }

            var KeepAlivePacket = PacketSerializer.Serialize(CommandPackets.NewPosition(_lastPosition));
            _moogMachineCommunicator.SendPacket(KeepAlivePacket);
        }
    }
}