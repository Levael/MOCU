// add: AbstractManager, command, response, processor

using DaemonsNamespace.Common;
using System.Collections.Generic;
using System;
using UnityDaemonsCommon;

namespace DaemonsNamespace.InterprocessCommunication
{
    public abstract class AbstractDaemonProgramManager
    {
        public ObservableConcurrentQueue<string> outputMessagesQueue = null;
        public Dictionary<string, Action<string>> commandsHandlers;

        /// <summary>
        /// Processes a given JSON command by identifying its type and executing the corresponding handler.
        /// If the command is not recognized or an error occurs, an error response is generated and logged.
        /// </summary>
        /// <param name="jsonCommand">The JSON string representing the command to be processed.</param>
        public void ProcessCommand(string jsonCommand)
        {
            try
            {
                var commandName = CommonClientServerMethods.GetSerializedObjectType(jsonCommand);

                if (commandName == null || !commandsHandlers.ContainsKey(commandName))
                    throw new Exception($"Command name is 'null' or not in 'commandsHandlers' dictionary. Command name: {commandName}");

                commandsHandlers[commandName].Invoke(jsonCommand);
                DaemonsUtilities.ConsoleInfo($"Command '{commandName}' was executed");
            }
            catch (Exception ex)
            {
                DaemonsUtilities.ConsoleError($"Error while 'ProcessCommand': {ex}");

                var fullJsonResponse = CommonUtilities.SerializeJson(new ResponseFromServer(receivedCommand: "Unknown_Command", hasError: true, errorMessage: ex.ToString()));
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
