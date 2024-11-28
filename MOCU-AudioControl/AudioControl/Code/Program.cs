// dotnet publish -c Release

using InterprocessCommunication;
using DaemonsRelated;
using AudioModule.Daemon;


class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            // ============================================================================================
            // Common for every daemon part
            // ============================================================================================

            var daemonSupervisor = new DaemonSupervisor(args);

            if (!daemonSupervisor.IsValid())
                daemonSupervisor.CloseProgram("Init parameters are not valid");

            //daemonSupervisor.SubscribeForParentProcessTermination();

            // ============================================================================================
            // Exclusive part for this particular daemon
            // ============================================================================================

            var communicator    = new InterprocessCommunicator_Client(daemonSupervisor.DaemonName);
            var daemonLogic     = new AudioDaemon(communicator);

            communicator.ConnectionBroked += daemonSupervisor.CloseProgram;
            daemonLogic.TerminateDaemon += daemonSupervisor.CloseProgram;

            daemonLogic.Run();
            var terminationReason = await communicator.WaitForFirstError();
            daemonSupervisor.CloseProgram(terminationReason);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unhandled error: " + ex.ToString());
            Console.ReadKey();
        }
    }
}