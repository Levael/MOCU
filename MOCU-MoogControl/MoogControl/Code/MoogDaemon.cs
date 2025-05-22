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

        private readonly ConcurrentQueue<CommandPacket> _atomicCommandsForMoog;
        private readonly ConcurrentQueue<TrajectoryManager> _complexCommandsForMoog;

        private bool _doSendFeedback = false;

        // test
        private ConcurrentQueue<(DateTime time, DofParameters position)> _logs;

        public MoogDaemon(MoogDaemonSideBridge hostAPI)
        {
            _logs = new();


            _atomicCommandsForMoog = new();
            _complexCommandsForMoog = new();

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

            Task.Run(() => RunChartWindow());
        }

        private void RunChartWindow()
        {
            var formsPlot = new ScottPlot.WinForms.FormsPlot
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
            };

            ScottPlot.Plot plt = formsPlot.Plot;
            ScottPlot.Plottables.DataLogger logger = plt.Add.DataLogger();

            plt.Axes.Bottom.Label.Text = "Time [ms] (from start)";
            plt.Axes.Left.Label.Text = "Surge [m]";

            var form = new System.Windows.Forms.Form
            {
                Text = "ScottPlot Live",
                Width = 800,
                Height = 400
            };
            form.Controls.Add(formsPlot);

            form.FormClosed += (s, e) =>
            {
                System.Windows.Forms.Application.ExitThread();
            };

            DateTime startTime = DateTime.Now;

            var renderTimer = new System.Windows.Forms.Timer { Interval = 50 };
            renderTimer.Tick += (s, e) =>
            {
                while (_logs.TryDequeue(out var point))
                    logger.Add((point.time - startTime).TotalMilliseconds, point.position.Surge);

                if (logger.HasNewData)
                    formsPlot.Refresh();
            };
            renderTimer.Start();

            form.Show();
            System.Windows.Forms.Application.Run();
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
                DateTime now = DateTime.UtcNow;
                TimeSpan delay = parameters.ScheduledStartTime - now;
                TrajectoryManager trajectoryManager = new(parameters, _machineSettings, _moogRealTimeState);

                if (delay > TimeSpan.Zero)
                {
                    Task.Run(async () =>
                    {
                        await Task.Delay(delay);
                        // todo: maybe insert here safety checks
                        _complexCommandsForMoog.Enqueue(trajectoryManager);
                    });
                }
                else
                    _complexCommandsForMoog.Enqueue(trajectoryManager);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Handled error ucurred in 'HandleMoveByTrajectoryCommand': {ex}");
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
                        _atomicCommandsForMoog.Enqueue(CommandPackets.NewPosition(parameters.Coordinate));
                    });
                }
                else
                    _atomicCommandsForMoog.Enqueue(CommandPackets.NewPosition(parameters.Coordinate));
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
                _atomicCommandsForMoog.Enqueue(CommandPackets.Reset());
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
                _atomicCommandsForMoog.Enqueue(CommandPackets.Disengage());
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
                _atomicCommandsForMoog.Enqueue(CommandPackets.Engage(_machineSettings.StartPosition));
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

            /*var log = (time: DateTime.UtcNow, position: command.Parameters);
            _logs.Enqueue(log);*/   // show position right now


            /*byte[] packet = PacketSerializer.Serialize(command);
            _moogMachineCommunicator.SendPacket(packet);*/
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

                /*if (manager.TooLate())
                    _complexCommandsForMoog.TryDequeue(out _);*/

                Console.WriteLine("just before 'manager.GetNextPosition()'");

                if (manager.GetNextPosition() is DofParameters position)
                {
                    _logs.Enqueue((time: DateTime.UtcNow, position: command.Parameters));   // show only movements
                    return CommandPackets.NewPosition(position);
                }
                    
                else
                    _complexCommandsForMoog.TryDequeue(out _);

                return null;
            }

            // to send a 'keep alive' packet
            else
                return null;
        }
    }
}