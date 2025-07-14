using System;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Concurrent;


// Simplified version (maybe, refactor later on the manner of 'InterprocessCommunicator')


namespace MoogModule.Daemon
{
    public class MachineCommunicator
    {
        public event Action<(byte[] data, DateTime timestamp)> PacketReceived;
        public event Action<(byte[] data, DateTime timestamp)> PacketSent;

        private IPEndPoint HOST;
        private IPEndPoint MBC;
        private UdpClient _udpCommunicator;

        //private readonly BlockingCollection<byte[]> _toBeSentPacketsQueue;
        private readonly BlockingCollection<(byte[] data, DateTime timestamp)> _receivedPacketsQueue;
        private readonly BlockingCollection<(byte[] data, DateTime timestamp)> _sentPacketsQueue;

        private CancellationTokenSource _cancelationTokenSource;

        public MachineCommunicator()
        {
            //_toBeSentPacketsQueue = new();
            _receivedPacketsQueue = new();
            _sentPacketsQueue = new();
            _cancelationTokenSource = new();
        }

        public bool Connect(MachineSettings parameters)
        {
            try
            {
                HOST = new IPEndPoint(IPAddress.Parse(parameters.HOST_IP), Convert.ToInt32(parameters.HOST_PORT));
                MBC = new IPEndPoint(IPAddress.Parse(parameters.MBC_IP), Convert.ToInt32(parameters.MBC_PORT));

                _udpCommunicator = new UdpClient(HOST);
                _udpCommunicator.Client.ReceiveBufferSize = 1024 * 1024; // 1MB

                Task.Run(() => ProcessingLoop());
                Task.Run(() => FeedbackLoop());
                Task.Run(() => ListenLoop());
                //Task.Run(() => SendLoop());

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Handled error in 'Connect' method: {ex}");
                Disconnect();

                return false;
            }
        }

        public void Disconnect()
        {
            Console.WriteLine("Disconnecting 'MachineCommunicator'");

            _cancelationTokenSource?.Cancel();
            _udpCommunicator?.Close();
            _udpCommunicator?.Dispose();
            _cancelationTokenSource?.Dispose();

            //_toBeSentPacketsQueue?.CompleteAdding();
            _receivedPacketsQueue?.CompleteAdding();
        }

        public void SendPacket(byte[] data)
        {
            //_toBeSentPacketsQueue.Add(data);

            try
            {
                _udpCommunicator.Send(data, data.Length, MBC);
                _sentPacketsQueue.Add((data: data, timestamp: DateTime.UtcNow));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sending packet caused problem. Handled error: {ex}");
            }
        }

        //private void SendLoop()
        //{
        //    foreach (var data in _toBeSentPacketsQueue.GetConsumingEnumerable(_cancelationTokenSource.Token))
        //    {
        //        try
        //        {
        //            _udpCommunicator.Send(data, data.Length, MBC);
        //            _sentPacketsQueue.Add((data: data, timestamp: DateTime.UtcNow));
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"Sending packet caused problem. Handled error: {ex}");
        //        }
        //    }
        //}

        private void ProcessingLoop()
        {
            foreach (var content in _receivedPacketsQueue.GetConsumingEnumerable(_cancelationTokenSource.Token))
            {
                try
                {
                    PacketReceived?.Invoke(content);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Processing of received packet caused problem. Handled error: {ex}");
                }
            }   
        }

        private void FeedbackLoop()
        {
            foreach (var content in _sentPacketsQueue.GetConsumingEnumerable(_cancelationTokenSource.Token))
            {
                try
                {
                    PacketSent?.Invoke(content);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Processing of feedback of sent packet caused problem. Handled error: {ex}");
                }
            }
        }

        private async Task ListenLoop()
        {
            try
            {
                while (!_cancelationTokenSource.Token.IsCancellationRequested)
                {
                    var result = await _udpCommunicator.ReceiveAsync(_cancelationTokenSource.Token);

                    if (result.RemoteEndPoint.Address.Equals(MBC.Address))
                        _receivedPacketsQueue.Add((data: result.Buffer, timestamp: DateTime.UtcNow));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ListenLoop error: {ex}");
            }
        }
    }
}