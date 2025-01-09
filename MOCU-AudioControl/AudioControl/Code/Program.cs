using InterprocessCommunication;
using DaemonsRelated;
using AudioModule.Daemon;


class Program
{
    public static void Main(string[] args)
    {
        try
        {
            var daemonSupervisor = new DaemonSupervisor(args);

            if (!daemonSupervisor.IsValid())
                daemonSupervisor.CloseProgram("Init parameters are not valid");

            // Exclusive part for this particular daemon ==================================================
            var communicator    = new InterprocessCommunicator_Client(daemonSupervisor.DaemonName);
            var hostAPI         = new AudioDaemonSideBridge(communicator);
            var daemonLogic     = new AudioDaemon(hostAPI);
            // ============================================================================================

            daemonSupervisor.RunProgram(communicator: communicator, hostAPI: hostAPI, daemonLogic: daemonLogic);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unhandled error: " + ex.ToString());
            Environment.Exit(2);
        }
    }
}