// dotnet publish -c Release

using AudioControl;
using InterprocessCommunication;

class Program
{
    public async static Task Main(string[] args)
    {
        try
        {
            await new DaemonHandler_Server(
                businessLogic: new AudioManager(),
                argsFromCaller: args
            //argsFromCaller: new string[] { "69", "AudioControl", "False" }    // for debug with manual start
            ).StartDaemon();
        }
        catch (Exception ex)
        {
            Console.WriteLine("error: " + ex.ToString());
            Console.ReadLine();
        }
    }
}