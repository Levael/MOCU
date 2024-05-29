using UnityDaemonsCommon;
using DaemonsNamespace.InterprocessCommunication;

namespace AudioControl
{
    // Commands Handlers part

    public partial class AudioManager
    {
        private void UpdateDevicesParameters_CommandHandler(string jsonCommand)
        {
            try
            {
                //DaemonsUtilities.ConsoleWarning(jsonCommand);
                var command = CommonUtilities.DeserializeJson<UpdateDevicesParameters_Command>(jsonCommand);
                if (command == null)
                    throw new Exception("deserialization of 'UpdateDevicesParameters_CommandHandler' got 'null'");

                var report = audioDevicesParameters.Update(command.audioDevicesInfo);

                var fullJsonResponse = CommonUtilities.SerializeJson(new ResponseFromServer(receivedCommand: "UpdateDevicesParameters_Command", hasError: report.errorOccured, extraData: report.extraData));
                RespondToCommand(fullJsonResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in 'UpdateDevicesParameters_CommandHandler': {ex}");
                var fullJsonResponse = CommonUtilities.SerializeJson(new ResponseFromServer(receivedCommand: "UpdateDevicesParameters_Command", hasError: true, errorMessage: ex.ToString()));
                RespondToCommand(fullJsonResponse);
            }
        }

        private void SendConfigs_CommandHandler(string jsonCommand)
        {
            try
            {
                var command = CommonUtilities.DeserializeJson<SendConfigs_Command>(jsonCommand);
                if (command == null)
                    throw new Exception("deserialization of 'SendConfigs_Command' got 'null'");

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
                if (command == null)
                    throw new Exception("deserialization of 'GetAudioDevices_Command' got 'null'");

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


        private void StartIntercomStream_R2P_CommandHandler(string jsonCommand)
        {
            try
            {
                outgoingStream.StartStream();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in 'StartIntercomStream_R2P_CommandHandler': {ex}");
                var fullJsonResponse = CommonUtilities.SerializeJson(new ResponseFromServer(receivedCommand: "StartIntercomStream_ResearcherToParticipant_Command", hasError: true, errorMessage: ex.ToString()));
                RespondToCommand(fullJsonResponse);
            }
        }
        private void StartIntercomStream_P2R_CommandHandler(string jsonCommand)
        {
            try
            {
                incomingStream.StartStream();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in 'StartIntercomStream_P2R_CommandHandler': {ex}");
                var fullJsonResponse = CommonUtilities.SerializeJson(new ResponseFromServer(receivedCommand: "StartIntercomStream_ParticipantToResearcher_Command", hasError: true, errorMessage: ex.ToString()));
                RespondToCommand(fullJsonResponse);
            }
        }
        private void StopIntercomStream_R2P_CommandHandler(string jsonCommand)
        {
            try
            {
                outgoingStream.StopStream();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in 'StopIntercomStream_R2P_CommandHandler': {ex}");
                var fullJsonResponse = CommonUtilities.SerializeJson(new ResponseFromServer(receivedCommand: "StopIntercomStream_ResearcherToParticipant_Command", hasError: true, errorMessage: ex.ToString()));
                RespondToCommand(fullJsonResponse);
            }
        }
        private void StopIntercomStream_P2R_CommandHandler(string jsonCommand)
        {
            try
            {
                incomingStream.StopStream();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in 'StopIntercomStream_P2R_CommandHandler': {ex}");
                var fullJsonResponse = CommonUtilities.SerializeJson(new ResponseFromServer(receivedCommand: "StopIntercomStream_ParticipantToResearcher_Command", hasError: true, errorMessage: ex.ToString()));
                RespondToCommand(fullJsonResponse);
            }
        }

    }
}
