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

        private UdpClient _udpCommunicator;


        public void Connect(MachineSettings parameters)
        {
            var HOST = new IPEndPoint(IPAddress.Parse(parameters.HOST_IP), Convert.ToInt32(parameters.HOST_PORT));
            var MBC = new IPEndPoint(IPAddress.Parse(parameters.MBC_IP), Convert.ToInt32(parameters.MBC_PORT));

            _udpCommunicator = new UdpClient(HOST);
            _udpCommunicator.Connect(MBC);

            var listener = new Task(() => Listener());
        }

        public void SendPacket(byte[] data)
        {
            if (_udpCommunicator is not null)
            {
                // 2 times cause that's what Moog documentation says
                _udpCommunicator.Send(data, data.Length);
                _udpCommunicator.Send(data, data.Length);
            }
        }


        private async void Listener()
        {
            try
            {
                while (true)
                {
                    var result = await _udpCommunicator.ReceiveAsync();
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