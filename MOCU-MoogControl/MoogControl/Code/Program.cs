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

            var UdpReadThread = new Thread(() => StartListener(5000));


            SystemOptimizer.Optimize();
            flag = true;
            UdpReadThread.Start();
            inputInterface.Start();
            intervalExecutor.Start();
            Console.WriteLine("Got here. Is it okay?");
        }



        static void OnExit()
        {
            SystemOptimizer.ResetToDefault();
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

            while (queue.TryDequeue(out Action action))
            {
                action.Invoke();
            }
        }

        static void InputGetter(ref bool flag, ref ConcurrentQueue<Action> queue)
        {
            while (flag)
            {
                ConsoleKeyInfo key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.I)
                {
                    Console.WriteLine("You pressed 'I'!");
                    queue.Enqueue(() => SendMessage("Hello, UDP!", "192.168.2.1", 16384));
                }

                if (key.Key == ConsoleKey.O)
                {
                    Console.WriteLine("You pressed 'O'!");
                    queue.Enqueue(() => Console.WriteLine("You pressed 'O' inside!"));
                }
            }
        }

        static void StartListener(int port)
        {
            using var udpClient = new UdpClient(port);
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, port);

            Console.WriteLine($"Listening on port {port}...");

            while (true)
            {
                byte[] receivedData = udpClient.Receive(ref remoteEndPoint);
                string message = Encoding.UTF8.GetString(receivedData);

                Console.WriteLine($"Received from {remoteEndPoint}: {message}");
            }
        }

        static void SendMessage(string message, string ip, int port)
        {
            using var udpClient = new UdpClient();
            byte[] data = Encoding.UTF8.GetBytes(message);

            udpClient.Send(data, data.Length, new IPEndPoint(IPAddress.Parse(ip), port));
            Console.WriteLine($"Sent: {message} to {ip}:{port}");
        }
    }
}