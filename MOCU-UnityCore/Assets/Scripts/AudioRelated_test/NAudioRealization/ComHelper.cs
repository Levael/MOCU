using System;
using System.Threading;


namespace AudioModule_NAudio
{
    
    public static class ComHelper
    {
        public static void RunInSTAThread(Action action)
        {
            Thread staThread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    UnityMainThreadDispatcher.Enqueue(() => {
                        throw ex;
                    });
                }
            });

            // Назначаем поток как STA
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join(); // Ждём завершения
        }
    }

}
