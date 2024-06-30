using System;

namespace InterprocessCommunication
{
    public interface IInterprocessCommunicator : IDisposable
    {
        event Action<string> MessageReceived;

        void Start();
        void SendMessage(string message);
    }
}
