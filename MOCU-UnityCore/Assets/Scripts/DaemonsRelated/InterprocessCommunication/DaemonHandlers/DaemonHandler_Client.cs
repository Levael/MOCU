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
            _communicator.MessageReceived += HandleReceivedMessage;

            _responseProcessor = responseProcessor;

            _responseQueue = new BlockingCollection<Action>();
            _outputMessageQueue = new BlockingCollection<string>();

            _cancellationTokenSource = new CancellationTokenSource();
        }



        public void StartDaemon()
        {
            _communicator.Start();

            _commandSendingTask = Task.Run(() => SendCommands(_cancellationTokenSource.Token));
        }

        public void StopDaemon()
        {
            _cancellationTokenSource.Cancel();

            try { _communicator.Dispose(); } catch { }
            try { _responseQueue.CompleteAdding(); } catch { }
            try { _outputMessageQueue.CompleteAdding(); } catch { }
            try { _commandSendingTask.Wait(); } catch { }
        }

        public void SendMessage(UnifiedResponseFrom_Server response)
        {
            try
            {
                var message = UnityDaemonsCommon.CommonUtilities.SerializeJson(response);
                _outputMessageQueue.Add(message);
            }
            catch (Exception ex)
            {
                UnityUtilities.ConsoleError($"Error serializing response: {ex.Message}");
            }
        }

        private void HandleReceivedMessage(string message)
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

        private void SendCommands(CancellationToken token)
        {
            foreach (var message in _outputMessageQueue.GetConsumingEnumerable(token))
                _communicator.SendMessage(message);
        }
    }
}