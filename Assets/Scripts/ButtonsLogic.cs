using UnityEngine;
using UnityEngine.InputSystem;

public class ButtonsLogic : MonoBehaviour
{
    private UiHandler _uiHandler;

    private void Awake()
    {
        _uiHandler = GetComponent<UiHandler>();
    }



    public void IntercomFromResearcherPressed()
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

    public void IntercomFromParticipantPressed()
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

    public void AnswerLeftPressed()
    {
        _uiHandler.ControllerButtonWasPressed("controller-left-btn");     // for UI/UX purpose only
    }
    public void AnswerLeftReleased()
    {
        _uiHandler.ControllerButtonWasReleased("controller-left-btn");    // for UI/UX purpose only
    }
    public void AnswerRightPressed()
    {
        _uiHandler.ControllerButtonWasPressed("controller-right-btn");     // for UI/UX purpose only
    }
    public void AnswerRightReleased()
    {
        _uiHandler.ControllerButtonWasReleased("controller-right-btn");    // for UI/UX purpose only
    }
    public void AnswerUpPressed()
    {
        _uiHandler.ControllerButtonWasPressed("controller-up-btn");     // for UI/UX purpose only
    }
    public void AnswerUpReleased()
    {
        _uiHandler.ControllerButtonWasReleased("controller-up-btn");    // for UI/UX purpose only
    }
    public void AnswerDownPressed()
    {
        _uiHandler.ControllerButtonWasPressed("controller-down-btn");     // for UI/UX purpose only
    }
    public void AnswerDownReleased()
    {
        _uiHandler.ControllerButtonWasReleased("controller-down-btn");    // for UI/UX purpose only
    }
    public void AnswerCenterPressed()
    {
        _uiHandler.ControllerButtonWasPressed("controller-center-btn");     // for UI/UX purpose only
    }
    public void AnswerCenterReleased()
    {
        _uiHandler.ControllerButtonWasReleased("controller-center-btn");    // for UI/UX purpose only
    }



    // Method overloads (if called from InputAction -- have to get "context" parameter)
    public void IntercomFromResearcherPressed   (InputAction.CallbackContext context)   { IntercomFromResearcherPressed();      }
    public void IntercomFromResearcherReleased  (InputAction.CallbackContext context)   { IntercomFromResearcherReleased();     }
    public void IntercomFromParticipantPressed  (InputAction.CallbackContext context)   { IntercomFromParticipantPressed();     }
    public void IntercomFromParticipantReleased (InputAction.CallbackContext context)   { IntercomFromParticipantReleased();    }
    public void AnswerLeftPressed               (InputAction.CallbackContext context)   { AnswerLeftPressed();                  }
    public void AnswerLeftReleased              (InputAction.CallbackContext context)   { AnswerLeftReleased();                 }
    public void AnswerRightPressed              (InputAction.CallbackContext context)   { AnswerRightPressed();                 }
    public void AnswerRightReleased             (InputAction.CallbackContext context)   { AnswerRightReleased();                }
    public void AnswerUpPressed                 (InputAction.CallbackContext context)   { AnswerUpPressed();                    }
    public void AnswerUpReleased                (InputAction.CallbackContext context)   { AnswerUpReleased();                   }
    public void AnswerDownPressed               (InputAction.CallbackContext context)   { AnswerDownPressed();                  }
    public void AnswerDownReleased              (InputAction.CallbackContext context)   { AnswerDownReleased();                 }
    public void AnswerCenterPressed             (InputAction.CallbackContext context)   { AnswerCenterPressed();                }
    public void AnswerCenterReleased            (InputAction.CallbackContext context)   { AnswerCenterReleased();               }
}
