using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;


// Simplified version (maybe, refactor later on the manner of 'InterprocessCommunicator')


namespace MoogModule.Daemon
{
    public class MachineCommunicator
    {
        public event Action<byte[]> PacketReceived;

        private IPEndPoint HOST;
        private IPEndPoint MBC;
        private UdpClient _udpCommunicator;


        public void Connect(MachineSettings parameters)
        {
            HOST = new IPEndPoint(IPAddress.Parse(parameters.HOST_IP), Convert.ToInt32(parameters.HOST_PORT));
            MBC = new IPEndPoint(IPAddress.Parse(parameters.MBC_IP), Convert.ToInt32(parameters.MBC_PORT));

            _udpCommunicator = new UdpClient(HOST);
            _udpCommunicator.Client.ReceiveBufferSize = 1024 * 1024; // 1MB

            Task.Run(() => Listener());
        }

        public void SendPacket(byte[] data)
        {
            if (_udpCommunicator is not null)
            {
                // 2 times cause that's what Moog documentation says
                _udpCommunicator.Send(data, data.Length, MBC);
                //_udpCommunicator.Send(data, data.Length);
            }
        }


        private async void Listener()
        {
            try
            {
                while (true)
                {
                    var result = await _udpCommunicator.ReceiveAsync();

                    if (result.RemoteEndPoint.Address.Equals(MBC.Address))
                        PacketReceived?.Invoke(result.Buffer);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Listener error: {ex}");
            }
        }
    }
}