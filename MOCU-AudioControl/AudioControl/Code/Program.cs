// dotnet publish -c Release

using InterprocessCommunication;
using DaemonsRelated;
using DaemonsRelated.Audio;

class Program
{
    public void Main(string[] args)
    {
        try
        {
            // ============================================================================================
            // Common for every daemon part
            // ============================================================================================

            var daemonSupervisor = new DaemonSupervisor(args);

            if (!daemonSupervisor.IsValid())
                daemonSupervisor.CloseProgram();

            daemonSupervisor.SubscribeForParentProcessTermination();

            // ============================================================================================
            // Exclusive part for this particular daemon
            // ============================================================================================

            var communicator    = new InterprocessCommunicator_Client(daemonSupervisor.DaemonName);
            var hostAPI         = new AudioHostBridge(communicator);
            var daemonLogic     = new AudioDaemon(hostAPI);

            daemonLogic.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine("error: " + ex.ToString());
            Console.ReadLine();
        }
    }
}