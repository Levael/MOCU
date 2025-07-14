using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

using MoogModule;
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

        private volatile bool _doSaveLogs = false;
        private volatile bool _moogMachineIsConnected = false;

        private ConcurrentQueue<(DateTime time, DofParameters position)> _logsCommand;
        private ConcurrentQueue<(DateTime time, MoogRealTimeState state)> _logsResponses;

        public MoogDaemon(MoogDaemonSideBridge hostAPI)
        {
            _logsCommand = new();
            _logsResponses = new();

            _atomicCommandsForMoog = new();
            _complexCommandsForMoog = new();

            _hostAPI = hostAPI;
            _moogMachineCommunicator = new MachineCommunicator();
            _moogRealTimeState = new MoogRealTimeState();
            _intervalExecutor = new IntervalExecutor(TimeSpan.FromMilliseconds(1));

            _hostAPI.TerminateDaemon += message => Console.WriteLine("Got command to terminate the daemon.");
            _moogMachineCommunicator.PacketReceived += HandleReceivedPacket;
            _moogMachineCommunicator.PacketSent += HandleSentPacket;
            _intervalExecutor.OnTick += ExecuteEveryTick;

            _hostAPI.Connect += HandleConnectCommand;
            _hostAPI.Engage += HandleEngageCommand;
            _hostAPI.Disengage += HandleDisengageCommand;
            _hostAPI.Reset += HandleResetCommand;
            _hostAPI.StartReceivingFeedback += HandleStartReceivingFeedbackCommand;
            _hostAPI.StopReceivingFeedback += HandleStopReceivingFeedbackCommand;
            _hostAPI.MoveToPoint += HandleMoveToPointCommand;
            _hostAPI.MoveByTrajectory += HandleMoveByTrajectoryCommand;
            _hostAPI.Test += HandleTestCommand;
        }

        public void DoBeforeExit()
        {
            SystemOptimizer.ResetToDefault();
        }

        public void Run()
        {
            SystemOptimizer.Optimize();

            _hostAPI.StartCommunication();
            _intervalExecutor.Start();

            //Task.Run(() => RunChartWindow());
        }

        /*private void RunChartWindow()
        {
            var formsPlot = new ScottPlot.WinForms.FormsPlot
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
            };

            ScottPlot.Plot plt = formsPlot.Plot;
            ScottPlot.Plottables.DataLogger loggerCommand = plt.Add.DataLogger();
            ScottPlot.Plottables.DataLogger loggerFeedback = plt.Add.DataLogger();

            plt.Axes.Rules.Clear();
            plt.Axes.Bottom.Label.Text = "Time [ms] (from start)";
            plt.Axes.Left.Label.Text = "Surge [m]";
            plt.Axes.SetLimitsX(0, 10_000);
            plt.Axes.SetLimitsY(-0.5, 0.5);

            loggerCommand.MarkerSize = 3;
            loggerCommand.MarkerShape = ScottPlot.MarkerShape.FilledCircle;
            loggerFeedback.MarkerSize = 3;
            loggerFeedback.MarkerShape = ScottPlot.MarkerShape.OpenSquare;

            plt.Grid.XAxisStyle.MajorLineStyle.Width = 1;
            plt.Grid.XAxisStyle.MinorLineStyle.Width = 0.01f;

            loggerCommand.ManageAxisLimits = false;
            loggerFeedback.ManageAxisLimits = false;

            var form = new System.Windows.Forms.Form
            {
                Text = "ScottPlot Live",
                Width = 800,
                Height = 400
            };
            form.Controls.Add(formsPlot);

            form.FormClosed += (s, e) => System.Windows.Forms.Application.ExitThread();

            DateTime startTime = DateTime.UtcNow;

            var renderTimer = new System.Windows.Forms.Timer { Interval = 100 };
            var maxTicks = 2000;

            renderTimer.Tick += (s, e) =>
            {
                if (maxTicks-- <= 0)
                    return;

                while (_logsCommand.TryDequeue(out var point))
                    loggerCommand.Add((point.time - startTime).TotalMilliseconds, point.position.Surge);

                while (_logsResponses.TryDequeue(out var point))
                    loggerFeedback.Add((point.time - startTime).TotalMilliseconds, point.position.Surge);

                if ((loggerCommand.HasNewData || loggerFeedback.HasNewData) && (maxTicks-- >= 0))
                    formsPlot.Refresh();
            };
            renderTimer.Start();

            form.Show();
            System.Windows.Forms.Application.Run();
        }*/

        // ########################################################################################

        private void HandleConnectCommand(MachineSettings parameters)
        {
            try
            {
                _machineSettings = parameters;
                _moogMachineIsConnected = _moogMachineCommunicator.Connect(_machineSettings);

                // test ?
                _moogRealTimeState.Position = _machineSettings.StartPosition;
                _moogRealTimeState.DesiredPosition = _machineSettings.StartPosition;
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

        // todo: looks like it needs to be reworked
        private void HandleStopReceivingFeedbackCommand()
        {
            if (_doSaveLogs == false)
                return;

            _doSaveLogs = false;

            var feedback = new MoogFeedback
            {
                Commands = _logsCommand,
                Responses = _logsResponses,
            };

            _hostAPI.Feedback(feedback);

            _logsCommand.Clear();
            _logsResponses.Clear();
        }

        private void HandleStartReceivingFeedbackCommand()
        {
            _doSaveLogs = true;
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

        private void HandleReceivedPacket((byte[] data, DateTime timestamp) packet)
        {
            try
            {
                var parsedMessage = PacketSerializer.Deserialize(packet.data);

                _moogRealTimeState.EncodedMachineState = parsedMessage.EncodedMachineState;
                _moogRealTimeState.Position = parsedMessage.Parameters;

                if (_doSaveLogs)
                    _logsResponses.Enqueue((time: packet.timestamp, state: _moogRealTimeState));

                // TODO:
                // - faults and state changes (if yes - make and send MoogRealTimeState + add to log if needed)
                // - add safety checks
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Handled error ucurred in 'HandleReceivedPacket': {ex}");
            }
        }

        private void HandleSentPacket((byte[] data, DateTime timestamp) packet)
        {
            try
            {
                if (_doSaveLogs)
                {
                    var parsedMessage = PacketSerializer.Test(packet.data);
                    _logsCommand.Enqueue((time: packet.timestamp, position: parsedMessage.Parameters));
                }  
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Handled error ucurred in 'HandleSentPacket': {ex}");
            }
        }

        private void HandleTestCommand(object? input = null)
        {
            var feedback = new MoogFeedback
            {
                Commands = new List<(DateTime timestamp, DofParameters position)>
                {
                    (new DateTime(2025, 7, 14, 13, 5, 0, 0, DateTimeKind.Utc), new DofParameters { Surge = 0.1f }),
                    (new DateTime(2025, 7, 14, 13, 5, 0, 1, DateTimeKind.Utc), new DofParameters { Surge = 0.2f }),
                    (new DateTime(2025, 7, 14, 13, 5, 0, 2, DateTimeKind.Utc), new DofParameters { Surge = 0.15f })
                },
                
                Responses = new List<(DateTime timestamp, MoogRealTimeState feedback)>
                {
                    (new DateTime(2025, 7, 14, 13, 5, 0, 5, DateTimeKind.Utc), new MoogRealTimeState
                    {
                        EncodedMachineState = EncodedMachineState.Engaged,
                        Position = new DofParameters { Surge = 0.1f }
                    }),
                    (new DateTime(2025, 7, 14, 13, 5, 0, 6, DateTimeKind.Utc), new MoogRealTimeState
                    {
                        EncodedMachineState = EncodedMachineState.Engaged,
                        Position = new DofParameters { Surge = 0.2f }
                    }),
                    (new DateTime(2025, 7, 14, 13, 5, 0, 7, DateTimeKind.Utc), new MoogRealTimeState
                    {
                        EncodedMachineState = EncodedMachineState.Engaged,
                        Position = new DofParameters { Surge = 0.3f }
                    })
                }
            };

            _hostAPI.Feedback(feedback);
        }

        private void ExecuteEveryTick()
        {
            if (!_moogMachineIsConnected) return;

            CommandPacket command = RequestNextCommand() ?? CommandPackets.NewPosition(_moogRealTimeState.DesiredPosition);
            _moogRealTimeState.DesiredPosition = command.Parameters;

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

                /*if (manager.TooLate())
                    _complexCommandsForMoog.TryDequeue(out _);*/

                if (manager.GetNextPosition() is DofParameters position)
                    return CommandPackets.NewPosition(position);
                else
                {
                    // end of trajectory. todo: maybe use it somehow
                    _complexCommandsForMoog.TryDequeue(out _);
                }

                return null;
            }

            // to send a 'keep alive' packet
            else
                return null;
        }

        /*private void PlotTrajectoryParameters(string outputPath, IEnumerable<DofParameters> trajectory)
        {
            var plt = new ScottPlot.Plot();

            int count = trajectory.Count();
            double[] xs = Enumerable.Range(0, count).Select(i => (double)i).ToArray();

            var series = new Dictionary<string, double[]>
            {
                ["Roll"] = trajectory.Select(p => (double)p.Roll).ToArray(),
                ["Pitch"] = trajectory.Select(p => (double)p.Pitch).ToArray(),
                ["Yaw"] = trajectory.Select(p => (double)p.Yaw).ToArray(),
                ["Surge"] = trajectory.Select(p => (double)p.Surge).ToArray(),
                ["Sway"] = trajectory.Select(p => (double)p.Sway).ToArray(),
                ["Heave"] = trajectory.Select(p => (double)p.Heave).ToArray(),
            };

            foreach (var kvp in series)
            {
                var line = plt.Add.Scatter(xs, kvp.Value);
                line.LegendText = kvp.Key;
            }

            plt.Title("Trajectory Parameters");
            plt.Axes.Bottom.Label.Text = "Index";
            plt.Axes.Left.Label.Text = "Value";
            plt.Legend.IsVisible = true;

            plt.SavePng(outputPath, 800, 600);
        }*/
    }
}