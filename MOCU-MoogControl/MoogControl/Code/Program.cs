using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;


namespace MoogModule.Daemon
{
    public class Program
    {
        private static UdpClient udpClient = new UdpClient(new IPEndPoint(IPAddress.Parse("192.168.2.3"), 16386));
        private static IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse("192.168.2.1"), 16384);
        private static IPEndPoint anyEndPoint = new IPEndPoint(IPAddress.Any, 0);
        private static DofParameters startPosition = new DofParameters { Roll = 0f, Pitch = 0f, Heave = -0.1f, Surge = 0f, Yaw = 0f, Sway = 0f };

        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += (s, e) => OnExit();

            Console.WriteLine("Press any key to start...");
            Console.ReadKey();
            Console.WriteLine("Started");

            long index = 0;
            bool flag = false;
            ConcurrentQueue<Action> queue = new();

            var intervalExecutor = new IntervalExecutor(TimeSpan.FromMilliseconds(1));
            intervalExecutor.OnTick += () => ExecuteEveryTick(ref index, ref queue, ref flag, intervalExecutor);

            var inputInterface = new Thread(() => InputGetter(ref flag, ref queue));

            var UdpReadThread = new Thread(() => StartListener());


            //SystemOptimizer.Optimize();
            flag = true;
            UdpReadThread.Start();
            inputInterface.Start();
            intervalExecutor.Start();
        }



        static void OnExit()
        {
            //SystemOptimizer.ResetToDefault();
            udpClient.Dispose();
        }

        static void ExecuteEveryTick(ref long index, ref ConcurrentQueue<Action> queue, ref bool flag, IntervalExecutor intervalExecutor)
        {
            /*if ((index % (10 * 1_000) == 0) && (index != 0)) // every 10 sec
                Console.WriteLine($"AverageInterval = {intervalExecutor.AverageInterval.TotalMilliseconds}ms");*/

            if (++index > (1 * 60 * 1_000)) // after 1 min
            {
                intervalExecutor.Stop();
                flag = false;
                Console.WriteLine("Process finished. Press any key to exit...");
                Console.ReadKey(intercept: true);
            };

            SendMessage(PacketSerializer.Serialize(CommandPackets.NewPosition(startPosition)));

            /*while (queue.TryDequeue(out Action action))
            {
                action.Invoke();
            }*/
        }

        static void InputGetter(ref bool flag, ref ConcurrentQueue<Action> queue)
        {
            while (flag)
            {
                ConsoleKeyInfo key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.D1)
                {
                    Console.WriteLine("You pressed '1': sent Engage command");
                    SendMessage(PacketSerializer.Serialize(CommandPackets.Engage(startPosition)));
                }

                if (key.Key == ConsoleKey.D2)
                {
                    Console.WriteLine("You pressed '2': sent NewPosition command");
                    SendMessage(PacketSerializer.Serialize(CommandPackets.NewPosition(startPosition)));
                }

                if (key.Key == ConsoleKey.D3)
                {
                    Console.WriteLine("You pressed '3': sent Disengage command");
                    SendMessage(PacketSerializer.Serialize(CommandPackets.Disengage()));
                }
            }
        }

        static void StartListener()
        {
            while (true)
            {
                byte[] receivedData = udpClient.Receive(ref anyEndPoint);

                try
                {
                    var parsedMessage = PacketSerializer.Deserialize(receivedData);
                    Console.WriteLine($"\nReceived message:\n" +
                        $"PacketLength - {parsedMessage.PacketLength}. Should be - 52\n" +
                        $"PacketSequenceCount - {parsedMessage.PacketSequenceCount}\n" +
                        $"ReservedForFutureUse - {parsedMessage.ReservedForFutureUse}. Should be - 0\n" +
                        $"MessageId - {parsedMessage.MessageId}. Should be - 200\n" +

                        $"MachineStatusWord - {parsedMessage.MachineStatusWord}\n" +
                        $"DiscreteIOWord - {parsedMessage.DiscreteIOWord}\n" +
                        $"FaultPart1 - {parsedMessage.FaultPart1}\n" +
                        $"FaultPart2 - {parsedMessage.FaultPart2}\n" +
                        $"FaultPart3 - {parsedMessage.FaultPart3}\n" +
                        $"OptionalStatusData - {parsedMessage.OptionalStatusData}. Should be - 1\n" +
                        $"Parameters (heave) - {parsedMessage.Parameters.Heave}\n" +
                        $"OptionalStatusDataAgain - {parsedMessage.OptionalStatusDataAgain}. Should be - 0\n");
                }
                catch { }
            }
        }

        static void SendMessage(byte[] data, string ip = "192.168.2.1", int port = 16384)
        {
            udpClient.Send(data, data.Length, remoteEndPoint);
            Console.WriteLine($"Sent: {data} to {ip}:{port}");
        }
    }
}