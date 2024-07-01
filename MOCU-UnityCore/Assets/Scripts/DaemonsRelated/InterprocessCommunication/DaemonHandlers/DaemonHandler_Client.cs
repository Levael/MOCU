using System.Collections.Concurrent;
using System.Threading;
using System;
using System.Threading.Tasks;

using DaemonsNamespace.Common;


namespace InterprocessCommunication
{
    public class DaemonHandler_Client
    {
        private InterprocessCommunicator_Client _communicator;
        private IBusinessLogic_Client _responseProcessor;

        private BlockingCollection<Action> _responseQueue;
        private BlockingCollection<string> _outputMessageQueue;
        private Task _commandSendingTask;
        private CancellationTokenSource _cancellationTokenSource;

        public DaemonHandler_Client(string pipeName, IBusinessLogic_Client responseProcessor)
        {
            _communicator = new InterprocessCommunicator_Client(pipeName);
            _communicator.MessageReceived += HandleInputMessage;

            _responseProcessor = responseProcessor;
            _responseProcessor.SendCommand += HandleOutputMessage;

            _responseQueue = new BlockingCollection<Action>();
            _outputMessageQueue = new BlockingCollection<string>();

            _cancellationTokenSource = new CancellationTokenSource();
        }



        public async void StartDaemon()
        {
            await _communicator.StartAsync();

            _commandSendingTask = Task.Run(() => SendCommands(_cancellationTokenSource.Token));
        }

        public void StopDaemon()
        {
            _cancellationTokenSource.Cancel();

            try { _communicator.Dispose(); }                catch { }
            try { _responseQueue.CompleteAdding(); }        catch { }
            try { _outputMessageQueue.CompleteAdding(); }   catch { }
            try { _commandSendingTask.Wait(); }             catch { }
        }

        private void HandleInputMessage(string message)
        {
            try
            {
                var response = UnityDaemonsCommon.CommonUtilities.DeserializeJson<UnifiedResponseFrom_Server>(message);
                UnityMainThreadDispatcher.Enqueue(() =>
                {
                    _responseProcessor.ProcessResponse(response);
                });
            }
            catch (Exception ex)
            {
                UnityUtilities.ConsoleError($"Error deserializing response: {ex.Message}");
            }
        }

        private void HandleOutputMessage(UnifiedCommandFrom_Client command)
        {
            try
            {
                var message = UnityDaemonsCommon.CommonUtilities.SerializeJson(command);
                _outputMessageQueue.Add(message);
            }
            catch (Exception ex)
            {
                UnityUtilities.ConsoleError($"Error serializing command: {ex.Message}");
            }
        }

        private void SendCommands(CancellationToken token)
        {
            foreach (var message in _outputMessageQueue.GetConsumingEnumerable(token))
                _communicator.SendMessage(message);
        }
    }
}