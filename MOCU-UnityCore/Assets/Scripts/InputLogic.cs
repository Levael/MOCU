using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InputLogic : MonoBehaviour
{
    private UiHandler _uiHandler;
    private AnswerHandler _answerHandler;
    private AudioHandler _audioHandler;

    public event Action startIncomingIntercomStream;
    public event Action startOutgoingIntercomStream;
    public event Action stopIncomingIntercomStream;
    public event Action stopOutgoingIntercomStream;

    private void Awake()
    {
        _uiHandler = GetComponent<UiHandler>();
        _answerHandler = GetComponent<AnswerHandler>();
        _audioHandler = GetComponent<AudioHandler>();
    }

    public void TestMethod(string text)
    {
        //_uiHandler.PrintToWarnings(text);
    }

    


    public void IntercomFromResearcherStarted()
    {
        if (_audioHandler.stateTracker.Status != StateTracker.DeviceConnectionStatus.Connected) return;

        startOutgoingIntercomStream?.Invoke();
        _uiHandler.mainTabScreen.GetElement("controls-intercom-part").AddToClassList("isActive");
        _uiHandler.mainTabScreen.GetElement("controls-intercom-part").AddToClassList("isOutcomming");
    }
    public void IntercomFromResearcherStopped()
    {
        if (_audioHandler.stateTracker.Status != StateTracker.DeviceConnectionStatus.Connected) return;

        stopOutgoingIntercomStream?.Invoke();
        if (!_uiHandler.mainTabScreen.GetElement("controls-intercom-part").ClassListContains("isIncomming"))
        {
            // the check is necessary so that the highlight does not disappear completely when pressed simultaneously
            _uiHandler.mainTabScreen.GetElement("controls-intercom-part").RemoveFromClassList("isActive");
        }
        _uiHandler.mainTabScreen.GetElement("controls-intercom-part").RemoveFromClassList("isOutcomming");
    }

    public void IntercomFromParticipantStarted()
    {
        if (_audioHandler.stateTracker.Status != StateTracker.DeviceConnectionStatus.Connected) return;

        startIncomingIntercomStream?.Invoke();
        _uiHandler.mainTabScreen.GetElement("controls-intercom-part").AddToClassList("isActive");
        _uiHandler.mainTabScreen.GetElement("controls-intercom-part").AddToClassList("isIncomming");
    }
    public void IntercomFromParticipantStopped()
    {
        if (_audioHandler.stateTracker.Status != StateTracker.DeviceConnectionStatus.Connected) return;

        stopIncomingIntercomStream?.Invoke();
        if (!_uiHandler.mainTabScreen.GetElement("controls-intercom-part").ClassListContains("isOutcomming"))
        {
            // the check is necessary so that the highlight does not disappear completely when pressed simultaneously
            _uiHandler.mainTabScreen.GetElement("controls-intercom-part").RemoveFromClassList("isActive");
        }
        _uiHandler.mainTabScreen.GetElement("controls-intercom-part").RemoveFromClassList("isIncomming");
    }



    public void GotPressSignalFromInputSystem(SignalFromParticipant signalFromParticipant)
    {
        // All this is temp and dirty

        switch (signalFromParticipant)
        {
            case SignalFromParticipant.Up:
                AnswerUpPressed(source: "inputSystem");
                break;
            case SignalFromParticipant.Left:
                AnswerLeftPressed(source: "inputSystem");
                break;
            case SignalFromParticipant.Center:
                AnswerCenterPressed(source: "inputSystem");
                break;
            case SignalFromParticipant.Right:
                AnswerRightPressed(source: "inputSystem");
                break;
            case SignalFromParticipant.Down:
                AnswerDownPressed(source: "inputSystem");
                break;
        }
        _answerHandler.AddAnswer(signalFromParticipant);
    }

    public void GotReleaseSignalFromInputSystem(SignalFromParticipant signalFromParticipant)
    {
        // All this is temp and dirty

        switch (signalFromParticipant)
        {
            case SignalFromParticipant.Up:
                AnswerUpReleased();
                break;
            case SignalFromParticipant.Left:
                AnswerLeftReleased();
                break;
            case SignalFromParticipant.Center:
                AnswerCenterReleased();
                break;
            case SignalFromParticipant.Right:
                AnswerRightReleased();
                break;
            case SignalFromParticipant.Down:
                AnswerDownReleased();
                break;
        }
    }

    public void GotAnswerFromCedrus(SignalFromParticipant signalFromParticipant)
    {
        // All this is temp and dirty

        switch (signalFromParticipant)
        {
            case SignalFromParticipant.Up:
                AnswerUpPressed(source: "cedrus");
                break;
            case SignalFromParticipant.Left:
                AnswerLeftPressed(source: "cedrus");
                break;
            case SignalFromParticipant.Center:
                AnswerCenterPressed(source: "cedrus");
                break;
            case SignalFromParticipant.Right:
                AnswerRightPressed(source: "cedrus");
                break;
            case SignalFromParticipant.Down:
                AnswerDownPressed(source: "cedrus");
                break;
        }
        _answerHandler.AddAnswer(signalFromParticipant);
    }




    public void AnswerLeftPressed(string source)
    {
        _uiHandler.ControllerButtonWasPressed("controller-left-btn");
        if (source == "ui" || source == "cedrus") { Invoke("AnswerLeftReleased", 0.1f); }       // with delay 0.5sec if clicked with mouse    // todo later to all
    }
    public void AnswerLeftReleased()
    {
        _uiHandler.ControllerButtonWasReleased("controller-left-btn");
    }
    public void AnswerRightPressed(string source)
    {
        _uiHandler.ControllerButtonWasPressed("controller-right-btn");
        if (source == "ui" || source == "cedrus") { Invoke("AnswerRightReleased", 0.1f); }
    }
    public void AnswerRightReleased()
    {
        _uiHandler.ControllerButtonWasReleased("controller-right-btn");
    }
    public void AnswerUpPressed(string source)
    {
        _uiHandler.ControllerButtonWasPressed("controller-up-btn");
        if (source == "ui" || source == "cedrus") { Invoke("AnswerUpReleased", 0.1f); }
    }
    public void AnswerUpReleased()
    {
        _uiHandler.ControllerButtonWasReleased("controller-up-btn");
    }
    public void AnswerDownPressed(string source)
    {
        _uiHandler.ControllerButtonWasPressed("controller-down-btn");
        if (source == "ui" || source == "cedrus") { Invoke("AnswerDownReleased", 0.1f); }
    }
    public void AnswerDownReleased()
    {
        _uiHandler.ControllerButtonWasReleased("controller-down-btn");
    }
    public void AnswerCenterPressed(string source)
    {
        _uiHandler.ControllerButtonWasPressed("controller-center-btn");
        if (source == "ui" || source == "cedrus") { Invoke("AnswerCenterReleased", 0.1f); }
    }
    public void AnswerCenterReleased()
    {
        _uiHandler.ControllerButtonWasReleased("controller-center-btn");
    }

}
