using UnityEngine;
using UnityEngine.InputSystem;

public class InputLogic : MonoBehaviour
{
    private UiHandler _uiHandler;

    private void Awake()
    {
        _uiHandler = GetComponent<UiHandler>();
    }

    public void TestMethod(string text)
    {
        _uiHandler.PrintToWarnings(text);
    }



    public void IntercomFromResearcherPressed(string source)
    {
        _uiHandler.mainScreen.GetElement("controls-intercom-part").AddToClassList("isActive");
        _uiHandler.mainScreen.GetElement("controls-intercom-part").AddToClassList("isOutcomming");
    }

    public void IntercomFromResearcherReleased()
    {
        if (!_uiHandler.mainScreen.GetElement("controls-intercom-part").ClassListContains("isIncomming"))
        {
            // the check is necessary so that the highlight does not disappear completely when pressed simultaneously
            _uiHandler.mainScreen.GetElement("controls-intercom-part").RemoveFromClassList("isActive");
        }
        _uiHandler.mainScreen.GetElement("controls-intercom-part").RemoveFromClassList("isOutcomming");
    }

    public void IntercomFromParticipantPressed(string source)
    {
        _uiHandler.mainScreen.GetElement("controls-intercom-part").AddToClassList("isActive");
        _uiHandler.mainScreen.GetElement("controls-intercom-part").AddToClassList("isIncomming");
    }

    public void IntercomFromParticipantReleased()
    {
        if (!_uiHandler.mainScreen.GetElement("controls-intercom-part").ClassListContains("isOutcomming"))
        {
            // the check is necessary so that the highlight does not disappear completely when pressed simultaneously
            _uiHandler.mainScreen.GetElement("controls-intercom-part").RemoveFromClassList("isActive");
        }
        _uiHandler.mainScreen.GetElement("controls-intercom-part").RemoveFromClassList("isIncomming");
    }

    public void AnswerLeftPressed(string source)
    {
        _uiHandler.ControllerButtonWasPressed("controller-left-btn");     // for UI/UX purpose only
        if (source == "ui") { Invoke("AnswerLeftReleased", 0.5f); }       // with delay 0.5sec if clicked with mouse    // todo later to all
    }
    public void AnswerLeftReleased()
    {
        _uiHandler.ControllerButtonWasReleased("controller-left-btn");    // for UI/UX purpose only
    }
    public void AnswerRightPressed(string source)
    {
        _uiHandler.ControllerButtonWasPressed("controller-right-btn");     // for UI/UX purpose only
        if(source == "ui") { AnswerRightReleased(); }
    }
    public void AnswerRightReleased()
    {
        _uiHandler.ControllerButtonWasReleased("controller-right-btn");    // for UI/UX purpose only
    }
    public void AnswerUpPressed(string source)
    {
        _uiHandler.ControllerButtonWasPressed("controller-up-btn");     // for UI/UX purpose only
        if(source == "ui") { AnswerUpReleased(); }
    }
    public void AnswerUpReleased()
    {
        _uiHandler.ControllerButtonWasReleased("controller-up-btn");    // for UI/UX purpose only
    }
    public void AnswerDownPressed(string source)
    {
        _uiHandler.ControllerButtonWasPressed("controller-down-btn");     // for UI/UX purpose only
        if(source == "ui") { AnswerDownReleased(); }
    }
    public void AnswerDownReleased()
    {
        _uiHandler.ControllerButtonWasReleased("controller-down-btn");    // for UI/UX purpose only
    }
    public void AnswerCenterPressed(string source)
    {
        _uiHandler.ControllerButtonWasPressed("controller-center-btn");     // for UI/UX purpose only
        if(source == "ui") { AnswerCenterReleased(); }
    }
    public void AnswerCenterReleased()
    {
        _uiHandler.ControllerButtonWasReleased("controller-center-btn");    // for UI/UX purpose only
    }



    // Method overloads (if called from InputAction -- have to get "context" parameter)
    public void IntercomFromResearcherPressed   (InputAction.CallbackContext context)   { IntercomFromResearcherPressed(source: "keyboard");}
    public void IntercomFromResearcherReleased  (InputAction.CallbackContext context)   { IntercomFromResearcherReleased();}
    public void IntercomFromParticipantPressed  (InputAction.CallbackContext context)   { IntercomFromParticipantPressed(source: "controller");}
    public void IntercomFromParticipantReleased (InputAction.CallbackContext context)   { IntercomFromParticipantReleased();}
    public void AnswerLeftPressed               (InputAction.CallbackContext context)   { AnswerLeftPressed(source: "controller");}
    public void AnswerLeftReleased              (InputAction.CallbackContext context)   { AnswerLeftReleased();}
    public void AnswerRightPressed              (InputAction.CallbackContext context)   { AnswerRightPressed(source: "controller");}
    public void AnswerRightReleased             (InputAction.CallbackContext context)   { AnswerRightReleased();}
    public void AnswerUpPressed                 (InputAction.CallbackContext context)   { AnswerUpPressed(source: "controller");}
    public void AnswerUpReleased                (InputAction.CallbackContext context)   { AnswerUpReleased();}
    public void AnswerDownPressed               (InputAction.CallbackContext context)   { AnswerDownPressed(source: "controller");}
    public void AnswerDownReleased              (InputAction.CallbackContext context)   { AnswerDownReleased();}
    public void AnswerCenterPressed             (InputAction.CallbackContext context)   { AnswerCenterPressed(source: "controller");}
    public void AnswerCenterReleased            (InputAction.CallbackContext context)   { AnswerCenterReleased();}
}
