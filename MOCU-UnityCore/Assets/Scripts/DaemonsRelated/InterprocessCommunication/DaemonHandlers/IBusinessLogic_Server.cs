namespace InterprocessCommunication
{
    public interface IBusinessLogic_Server
    {
        void ProcessCommand(UnifiedCommandFrom_Client command);
    }
}
