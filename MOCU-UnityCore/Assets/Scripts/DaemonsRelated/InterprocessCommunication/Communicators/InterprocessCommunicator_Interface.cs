using System;

namespace InterprocessCommunication
{
    public interface IInterprocessCommunicator : IDisposable
    {
        void Start();

        void SendMessage(string message);

        event Action<string> MessageReceived;
    }
}