using System;
using System.Management;

namespace PortFinder
{
    public class Program
    {
        static void Main(string[] args)
        {
            string inputData = args.Length > 0 ? args[0] : null;    // input from Unity
            Console.Write(FindPortByDeviceId(inputData));           // output to Unity
        }

        private static string FindPortByDeviceId(string deviceId)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE ConfigManagerErrorCode = 0"))
                {
                    foreach (ManagementObject queryObj in searcher.Get())
                    {
                        if (queryObj["PNPDeviceID"].ToString().Contains(deviceId))
                        {
                            var nameProperty = queryObj["Name"].ToString(); // For example "USB Serial Port (COM6)"
                            if (nameProperty.Contains("(COM"))
                            {
                                // Get Port name. Will look like something like "COM6"
                                return nameProperty.Substring(nameProperty.LastIndexOf("(COM")).Replace("(", "").Replace(")", "");
                            }
                        }
                    }
                }
                return null;
            }
            catch (System.UnauthorizedAccessException ex)
            {
                Console.Beep(400, 1000);
                return null;
            }
            catch (Exception ex)
            {
                Console.Beep(1800, 1000);
                return null;
            }
        }
    }
}
