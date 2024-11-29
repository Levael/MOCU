using System;


namespace InterprocessCommunication
{
    public interface IInterprocessCommunicator : IDisposable
    {
        bool IsOperational { get; }
        void Start();
        void SendMessage(string message);

        event Action<string> MessageReceived;
        event Action<string> MessageSent;
        event Action<string> ConnectionEstablished;
        event Action<string> ConnectionBroked;
    }
}