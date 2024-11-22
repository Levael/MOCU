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

            Console.Beep(1000, 500);

            Console.WriteLine("Daemon started");
            var terminationReason = await communicator.WaitForFirstError();
            Console.WriteLine($"Daemon ended: {terminationReason}");

            Console.Beep(2000, 500);
            //Thread.Sleep(Timeout.Infinite);
            /*Thread.Sleep(Timeout.Infinite);
            Console.WriteLine("Daemon ended");*/
            /*await communicator.WaitForFirstError();
            Thread.Sleep(5000);
            Console.WriteLine("Daemon ended?");*/
            //daemonSupervisor.WaitForProgramFinish();
        }
        catch (Exception ex)
        {
            // not 'daemonSupervisor.CloseProgram()' because it can be a problem itself
            Console.WriteLine("error: " + ex.ToString());
            Console.ReadKey();
        }
    }
}