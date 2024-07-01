using System;

namespace InterprocessCommunication
{
    public interface IBusinessLogic_Client
    {
        event Action<UnifiedCommandFrom_Client> SendCommand;
        void ProcessResponse(UnifiedResponseFrom_Server response);
    }
}
