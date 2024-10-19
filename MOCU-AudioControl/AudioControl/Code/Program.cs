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
            var daemonSupervisor = new DaemonSupervisor(args);

            if (!daemonSupervisor.IsValid())
                daemonSupervisor.CloseProgram();

            daemonSupervisor.SubscribeForParentProcessTermination();



            var communicator    = new InterprocessCommunicator_Client(daemonSupervisor.DaemonName);
            var hostAPI         = new AudioHostAPI(communicator);
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