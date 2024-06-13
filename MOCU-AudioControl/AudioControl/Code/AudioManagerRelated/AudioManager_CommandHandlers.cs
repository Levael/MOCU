using UnityDaemonsCommon;
using DaemonsNamespace.InterprocessCommunication;

namespace AudioControl
{
    public partial class AudioManager
    {
        private void UpdateDevicesParameters_CommandHandler(UnifiedCommandFromClient command)
        {
            try
            {
                audioDevicesParameters.UpdateParameters(updatedParameters: command.GetExtraData<AudioDevicesInfo>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in 'UpdateDevicesParameters_CommandHandler': {ex}");
                var fullJsonResponse = CommonUtilities.SerializeJson(new UnifiedResponseFromServer(name: "AudioDataHasBeenUpdated_Response", errorOccurred: true, errorIsFatal: false, errorMessage: ex.ToString()));
                RespondToCommand(fullJsonResponse);
            }
        }

        private void SendDataToClient()
        {
            Console.WriteLine($"Sending AudioData to the client");

            var fullJsonResponse = CommonUtilities.SerializeJson(new UnifiedResponseFromServer(name: "AudioDataHasBeenUpdated_Response", errorOccurred: false, extraData: audioDevicesParameters.GetAudioData()));
            RespondToCommand(fullJsonResponse);
        }

        private void SendConfigs_CommandHandler(UnifiedCommandFromClient command)
        {
            try
            {
                Console.WriteLine(command);
                Console.WriteLine(command.GetExtraData<SetConfigurations_CommandDetails>());
                pathToAudioFiles = command.GetExtraData<SetConfigurations_CommandDetails>().unityAudioDirectory;
                Console.WriteLine(pathToAudioFiles);

                LoadAudioFiles();   // todo: move to other place

                var fullJsonResponse = CommonUtilities.SerializeJson(new UnifiedResponseFromServer(name: "SetConfigurations_Response", errorOccurred: false));
                RespondToCommand(fullJsonResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in 'SendConfigs_CommandHandler': {ex}");
                var fullJsonResponse = CommonUtilities.SerializeJson(new UnifiedResponseFromServer(name: "SetConfigurations_Response", errorOccurred: true, errorIsFatal: true, errorMessage: ex.ToString()));
                RespondToCommand(fullJsonResponse);
            }
        }

        private void PlayAudioFile_CommandHanler(UnifiedCommandFromClient command)
        {
            try
            {
                Task.Run(() => PlayAudioFile(command.GetExtraData<PlayAudioFile_CommandDetails>()));
                // No response (for now at least. I don't think there should be one)
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in 'PlayAudioFile_CommandDetails': {ex}");
                var fullJsonResponse = CommonUtilities.SerializeJson(new UnifiedResponseFromServer(name: "PlayAudioFile_Command", errorOccurred: true, errorIsFatal: false, errorMessage: ex.ToString()));
                RespondToCommand(fullJsonResponse);
            }
        }

        private void StartIntercomStream_R2P_CommandHandler(UnifiedCommandFromClient command)
        {
            try
            {
                outgoingStream.StartStream();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in 'StartIntercomStream_R2P_CommandHandler': {ex}");
                var fullJsonResponse = CommonUtilities.SerializeJson(new UnifiedResponseFromServer(name: "StartIntercomStream_ResearcherToParticipant_Command", errorOccurred: true, errorIsFatal: false, errorMessage: ex.ToString()));
                RespondToCommand(fullJsonResponse);
            }
        }
        private void StartIntercomStream_P2R_CommandHandler(UnifiedCommandFromClient command)
        {
            try
            {
                incomingStream.StartStream();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in 'StartIntercomStream_P2R_CommandHandler': {ex}");
                var fullJsonResponse = CommonUtilities.SerializeJson(new UnifiedResponseFromServer(name: "StartIntercomStream_ParticipantToResearcher_Command", errorOccurred: true, errorIsFatal: false, errorMessage: ex.ToString()));
                RespondToCommand(fullJsonResponse);
            }
        }
        private void StopIntercomStream_R2P_CommandHandler(UnifiedCommandFromClient command)
        {
            try
            {
                outgoingStream.StopStream();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in 'StopIntercomStream_R2P_CommandHandler': {ex}");
                var fullJsonResponse = CommonUtilities.SerializeJson(new UnifiedResponseFromServer(name: "StopIntercomStream_ResearcherToParticipant_Command", errorOccurred: true, errorIsFatal: false, errorMessage: ex.ToString()));
                RespondToCommand(fullJsonResponse);
            }
        }
        private void StopIntercomStream_P2R_CommandHandler(UnifiedCommandFromClient command)
        {
            try
            {
                incomingStream.StopStream();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in 'StopIntercomStream_P2R_CommandHandler': {ex}");
                var fullJsonResponse = CommonUtilities.SerializeJson(new UnifiedResponseFromServer(name: "StopIntercomStream_ParticipantToResearcher_Command", errorOccurred: true, errorIsFatal: false, errorMessage: ex.ToString()));
                RespondToCommand(fullJsonResponse);
            }
        }

    }
}
