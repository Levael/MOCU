using InterprocessCommunication;

namespace AudioControl
{
    public partial class AudioManager
    {
        private void UpdateDevicesParameters_CommandHandler(UnifiedCommandFrom_Client command)
        {
            try
            {
                audioDevicesParameters.UpdateParameters(updatedParameters: command.GetExtraData<AudioDevicesInfo>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in 'UpdateDevicesParameters_CommandHandler': {ex}");

                var response = new UnifiedResponseFrom_Server(name: "AudioDataHasBeenUpdated_Response", errorOccurred: true, errorIsFatal: false, errorMessage: ex.ToString());
                SendResponse?.Invoke(response);
            }
        }

        private void SendDataToClient()
        {
            Console.WriteLine($"Sending AudioData to the client");

            var response = new UnifiedResponseFrom_Server(name: "AudioDataHasBeenUpdated_Response", errorOccurred: false, extraData: audioDevicesParameters.GetAudioData());
            SendResponse?.Invoke(response);
        }

        private void SendConfigs_CommandHandler(UnifiedCommandFrom_Client command)
        {
            try
            {
                pathToAudioFiles = command.GetExtraData<SetConfigurations_CommandDetails>().unityAudioDirectory;

                Console.WriteLine(pathToAudioFiles);

                LoadAudioFiles();   // todo: move to other place

                var response = new UnifiedResponseFrom_Server(name: "SetConfigurations_Response", errorOccurred: false);
                SendResponse?.Invoke(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in 'SendConfigs_CommandHandler': {ex}");
                var response = new UnifiedResponseFrom_Server(name: "SetConfigurations_Response", errorOccurred: true, errorIsFatal: true, errorMessage: ex.ToString());
                SendResponse?.Invoke(response);
            }
        }

        private void PlayAudioFile_CommandHanler(UnifiedCommandFrom_Client command)
        {
            try
            {
                Task.Run(() => PlayAudioFile(command.GetExtraData<PlayAudioFile_CommandDetails>()));
                // No response (for now at least. I don't think there should be one)
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in 'PlayAudioFile_CommandDetails': {ex}");
                var response = new UnifiedResponseFrom_Server(name: "PlayAudioFile_Command", errorOccurred: true, errorIsFatal: false, errorMessage: ex.ToString());
                SendResponse?.Invoke(response);
            }
        }

        private void StartIntercomStream_R2P_CommandHandler(UnifiedCommandFrom_Client command)
        {
            try
            {
                outgoingStream.StartStream();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in 'StartIntercomStream_R2P_CommandHandler': {ex}");
                var response = new UnifiedResponseFrom_Server(name: "StartIntercomStream_ResearcherToParticipant_Command", errorOccurred: true, errorIsFatal: false, errorMessage: ex.ToString());
                SendResponse?.Invoke(response);
            }
        }
        private void StartIntercomStream_P2R_CommandHandler(UnifiedCommandFrom_Client command)
        {
            try
            {
                incomingStream.StartStream();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in 'StartIntercomStream_P2R_CommandHandler': {ex}");
                var response = new UnifiedResponseFrom_Server(name: "StartIntercomStream_ParticipantToResearcher_Command", errorOccurred: true, errorIsFatal: false, errorMessage: ex.ToString());
                SendResponse?.Invoke(response);
            }
        }
        private void StopIntercomStream_R2P_CommandHandler(UnifiedCommandFrom_Client command)
        {
            try
            {
                outgoingStream.StopStream();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in 'StopIntercomStream_R2P_CommandHandler': {ex}");
                var response = new UnifiedResponseFrom_Server(name: "StopIntercomStream_ResearcherToParticipant_Command", errorOccurred: true, errorIsFatal: false, errorMessage: ex.ToString());
                SendResponse?.Invoke(response);
            }
        }
        private void StopIntercomStream_P2R_CommandHandler(UnifiedCommandFrom_Client command)
        {
            try
            {
                incomingStream.StopStream();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in 'StopIntercomStream_P2R_CommandHandler': {ex}");
                var response = new UnifiedResponseFrom_Server(name: "StopIntercomStream_ParticipantToResearcher_Command", errorOccurred: true, errorIsFatal: false, errorMessage: ex.ToString());
                SendResponse?.Invoke(response);
            }
        }

    }
}
