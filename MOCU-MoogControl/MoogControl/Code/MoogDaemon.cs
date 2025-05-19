using System;
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
        private readonly MachineCommunicator _moogMachineCommunicator;
        private readonly MoogRealTimeState _moogRealTimeState;
        private MachineSettings _machineSettings;

        private readonly ConcurrentQueue<CommandPacket> _commandsForMoog;
        private readonly ConcurrentQueue<CommandPacket> _atomicCommandsForMoog;
        private readonly ConcurrentQueue<TrajectoryManager> _complexCommandsForMoog;
        private readonly object __commandsForMoogLock;

        private DofParameters _startPosition;
        private bool _doSendFeedback = false;

        public MoogDaemon(MoogDaemonSideBridge hostAPI)
        {
            _commandsForMoog = new();
            _atomicCommandsForMoog = new();
            _complexCommandsForMoog = new();

            __commandsForMoogLock = new();
            _hostAPI = hostAPI;
            _moogMachineCommunicator = new MachineCommunicator();
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
            //throw new NotImplementedException();
        }

        public void Run()
        {
            _hostAPI.StartCommunication();
            _intervalExecutor.Start();
        }

        // ########################################################################################

        private void HandleConnectCommand(MachineSettings parameters)
        {
            try
            {
                _machineSettings = parameters;
                _moogMachineCommunicator.Connect(_machineSettings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Handled error ucurred in 'HandleConnectCommand': {ex}");
            }
        }

        // todo: refactor later
        private void HandleMoveByTrajectoryCommand(MoveByTrajectoryParameters parameters)
        {
            try
            {
                /*DateTime now = DateTime.UtcNow;
                TimeSpan delay = parameters.ScheduledTime - now;

                if (delay > TimeSpan.Zero)
                {
                    Task.Run(async () =>
                    {
                        await Task.Delay(delay);

                        lock (__commandsForMoogLock)
                        {
                            foreach (var coordinate in parameters.Coordinates)
                                _commandsForMoog.Enqueue(CommandPackets.NewPosition(coordinate));
                        }
                    });
                }
                else
                {
                    lock (__commandsForMoogLock)
                    {
                        foreach (var coordinate in parameters.Coordinates)
                            _commandsForMoog.Enqueue(CommandPackets.NewPosition(coordinate));
                    }
                } */

                try
                {
                    DateTime now = DateTime.UtcNow;
                    TimeSpan delay = parameters.ScheduledStartTime - now;

                    if (delay > TimeSpan.Zero)
                    {
                        Task.Run(async () =>
                        {
                            await Task.Delay(delay);
                            // todo: maybe insert here safety checks
                            _complexCommandsForMoog.Enqueue(new TrajectoryManager(parameters, ));
                        });
                    }
                    else
                        _complexCommandsForMoog.Enqueue(CommandPackets.NewPosition(parameters.Coordinate));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Handled error ucurred in 'HandleMoveToPointCommand': {ex}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Handled error ucurred in 'HandleMoveToPointCommand': {ex}");
            }
        }

        // todo: refactor later
        // todo: report an error if Queue is not empty when adding with delay (safety reason)
        private void HandleMoveToPointCommand(MoveToPointParameters parameters)
        {
            try
            {
                DateTime now = DateTime.UtcNow;
                TimeSpan delay = parameters.ScheduledStartTime - now;

                if (delay > TimeSpan.Zero)
                {
                    Task.Run(async () =>
                    {
                        await Task.Delay(delay);
                        // todo: maybe insert here safety checks
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
                _commandsForMoog.Enqueue(CommandPackets.Engage(_machineSettings.StartPosition));
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
                // - add safety checks

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
            CommandPacket command = RequestNextCommand() ?? CommandPackets.NewPosition(_moogRealTimeState.Position);
            byte[] packet = PacketSerializer.Serialize(command);
            _moogMachineCommunicator.SendPacket(packet);
        }

        private CommandPacket? RequestNextCommand()
        {
            // for every other commands
            if (_atomicCommandsForMoog.TryDequeue(out CommandPacket command))
                return command;

            // for trajectories. Does not delete element untill it hasn't been fully read
            else if (_complexCommandsForMoog.TryPeek(out TrajectoryManager? manager))
            {
                if (manager is null)
                    throw new Exception("RequestNextCommand -> TryPeek -> null");

                if (manager.TooEarly())
                    return null;

                if (manager.TooLate())
                    _complexCommandsForMoog.TryDequeue(out _);

                if (manager.GetNextPosition() is DofParameters position)
                    return CommandPackets.NewPosition(position);

                return null;
            }

            // to send a 'keep alive' packet
            else
                return null;
        }
    }
}