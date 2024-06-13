using System.Collections.Generic;
using System;

using DaemonsNamespace.Common;
using UnityDaemonsCommon;
using CustomDataStructures;


namespace DaemonsNamespace.InterprocessCommunication
{
    public abstract class AbstractDaemonProgramManager
    {
        public ObservableConcurrentQueue<string> outputMessagesQueue = null;
        public Dictionary<string, Action<UnifiedCommandFromClient>> commandsHandlers;

        public void ProcessCommand(string jsonCommand)
        {
            try
            {
                var command = CommonUtilities.DeserializeJson<UnifiedCommandFromClient>(jsonCommand);
                DaemonsUtilities.ConsoleWarning($"jsonCommand: {jsonCommand}");
                DaemonsUtilities.ConsoleWarning($"command: {command}");

                if (command == null || !commandsHandlers.ContainsKey(command.name))
                    throw new Exception($"Command from client is incorrect or unknown");

                commandsHandlers[command.name].Invoke(command);

                DaemonsUtilities.ConsoleInfo($"Command '{command.name}' was executed");
            }
            catch (Exception ex)
            {
                DaemonsUtilities.ConsoleError($"Error while 'ProcessCommand': {ex}");
                
                var fullJsonResponse = CommonUtilities.SerializeJson(new UnifiedResponseFromServer(name: "CommandProcessingError", errorOccurred: true, errorMessage: ex.ToString(), errorIsFatal: false));
                RespondToCommand(fullJsonResponse);
            }
        }

        /// <summary>
        /// Enqueues the given response message for processing. 
        /// Logs an error if the response is null, including the name of the calling method.
        /// </summary>
        /// <param name="response">The response message to be enqueued. If null, an error is logged.</param>
        public void RespondToCommand(string? response)
        {
            if (response == null)
            {
                string callerMethodName = CommonUtilities.GetCallerMethodName();
                DaemonsUtilities.ConsoleError($"To method 'RespondToCommand' has past 'null' from method '{callerMethodName}'");
                return;
            }

            outputMessagesQueue.Enqueue(response);
        }
    }
}
