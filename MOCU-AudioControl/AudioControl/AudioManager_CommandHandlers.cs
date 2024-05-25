using UnityDeamonsCommon;
using DeamonsNamespace.Common;

namespace AudioControl
{
    // Commands Handlers part

    public partial class AudioManager
    {
        /// <summary>
        /// Doesn't return response imidiately, it happend from "audioDevicesParameters.Update" method, which call to "RespondToCommand_UpdateDevicesParameters"
        /// </summary>
        private void UpdateDevicesParameters_CommandHandler(string jsonCommand)
        {
            try
            {
                DeamonsUtilities.ConsoleWarning(jsonCommand);
                var command = CommonUtilities.DeserializeJson<UpdateDevicesParameters_Command>(jsonCommand);
                audioDevicesParameters.Update(command.audioDevicesInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in 'UpdateDevicesParameters_CommandHandler': {ex}");
            }
        }
        private void RespondToCommand_UpdateDevicesParameters(bool errorHasOccured, object? extraData)
        {
            var fullJsonResponse = CommonUtilities.SerializeJson(new ResponseFromServer(receivedCommand: "UpdateDevicesParameters_Command", hasError: errorHasOccured, extraData: extraData));
            RespondToCommand(fullJsonResponse);
            DeamonsUtilities.ConsoleWarning(fullJsonResponse);
        }


        private void SendConfigs_CommandHandler(string jsonCommand)
        {
            try
            {
                var command = CommonUtilities.DeserializeJson<SendConfigs_Command>(jsonCommand);
                pathToAudioFiles = command.UnityAudioDirectory;

                LoadAudioFiles();   // todo: move to other place

                var fullJsonResponse = CommonUtilities.SerializeJson(new ResponseFromServer(receivedCommand: "SendConfigs_Command", hasError: false));
                RespondToCommand(fullJsonResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in 'SendConfigs_CommandHandler': {ex}");
                var fullJsonResponse = CommonUtilities.SerializeJson(new ResponseFromServer(receivedCommand: "SendConfigs_Command", hasError: true, errorMessage: ex.ToString()));
                RespondToCommand(fullJsonResponse);
            }
        }

        private void PlayAudioFile_CommandHanler(string jsonCommand)
        {
            try
            {
                var command = CommonUtilities.DeserializeJson<PlayAudioFile_Command>(jsonCommand);
                if (command == null)
                    throw new Exception("deserialization of 'PlayAudioFile_Command' got 'null'");

                Task.Run(() => PlayAudioFile(command));
                // No response (for now at least. I don't think there should be one)
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in 'SendConfigs_PlayAudioFile_CommandHanlerCommandHandler': {ex}");
                var fullJsonResponse = CommonUtilities.SerializeJson(new ResponseFromServer(receivedCommand: "PlayAudioFile_Command", hasError: true, errorMessage: ex.ToString()));
                RespondToCommand(fullJsonResponse);
            }
        }

        private void GetAudioDevices_CommandHandler(string jsonCommand)
        {
            try
            {
                var command = CommonUtilities.DeserializeJson<GetAudioDevices_Command>(jsonCommand);
                if (command.DoUpdate) UpdateMMDeviceCollections();

                var extraData = new AudioDevicesLists()
                {
                    InputDevices = inputDevices.Select(device => device.FriendlyName).ToList(),
                    OutputDevices = outputDevices.Select(device => device.FriendlyName).ToList()
                };

                var fullJsonResponse = CommonUtilities.SerializeJson(new ResponseFromServer(receivedCommand: "GetAudioDevices_Command", hasError: false, extraData: extraData));
                RespondToCommand(fullJsonResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in 'GetAudioDevices_CommandHandler': {ex}");
                var fullJsonResponse = CommonUtilities.SerializeJson(new ResponseFromServer(receivedCommand: "GetAudioDevices_Command", hasError: true, errorMessage: ex.ToString()));
                RespondToCommand(fullJsonResponse);
            }
        }

    }
}
