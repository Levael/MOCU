using System;

using DaemonsRelated;
using DaemonsRelated.DaemonPart;
using InterprocessCommunication;


namespace ChartsModule.Daemon
{
    public class ChartsDaemonSideBridge : ChartsHost_API, IHostAPI
    {
        public event Action<string> TerminateDaemon;
        public event Action<ChartData> GenerateChartAsImage;
        public event Action<ChartData> GenerateChartAsForm;

        private IInterprocessCommunicator _communicator;

        public ChartsDaemonSideBridge(IInterprocessCommunicator communicator)
        {
            _communicator = communicator;

            _communicator.MessageReceived       += message => HandleIncomingMessage(message);
            _communicator.MessageSent           += message => Console.WriteLine($"Sent message to host: {message}");
            _communicator.ConnectionEstablished += message => Console.WriteLine($"Connection established. {message}");
            _communicator.ConnectionBroked      += message => Console.WriteLine($"Connection broked. {message}");
            _communicator.ErrorOccurred         += message => Console.WriteLine($"Occurred an error in communicator. {message}");
        }

        public void StartCommunication()
        {
            _communicator.Start();
        }

        // ........

        private void HandleIncomingMessage(string message)
        {
            Console.WriteLine($"Got message from host: {message}");
            try
            {
                var DTO = JsonHelper.DeserializeJson<ChartsDataTransferObject>(message);

                // ................................................................................

                // CUSTOM MESSAGE
                if (!String.IsNullOrEmpty(DTO.CustomMessage))
                    Console.WriteLine($"Custom message in 'HandleIncomingMessage': {DTO.CustomMessage}");

                // TERMINATION COMMAND
                if (DTO.DoTerminateTheDaemon)
                    TerminateDaemon?.Invoke("Got command from host to terminate the daemon");

                // ................................................................................

                // GENERATE IMAGE
                if (DTO.SaveAsImage)
                    GenerateChartAsImage?.Invoke(DTO.ChartData);

                // GENERATE FORM
                if (DTO.OpenAsForm)
                    GenerateChartAsForm?.Invoke(DTO.ChartData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in 'HandleIncomingMessage': {ex.Message}");
            }
        }

        public void ChartImageGenerated(string path)
        {
            var DTO = new ChartsDataTransferObject { ImageFullPath = path };
            var json = JsonHelper.SerializeJson(DTO);
            _communicator.SendMessage(json);
        }
    }
}