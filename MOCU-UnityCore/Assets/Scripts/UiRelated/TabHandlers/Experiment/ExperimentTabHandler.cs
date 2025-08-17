using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Concurrent;
using System;


public class ExperimentTabHandler : ManagedMonoBehaviour
{
    private UiHandler _uiHandler;
    private UiReferences _uiReference;

    private TextElement _infoField;
    private TextElement _warningsField;


    public override void ManagedAwake()
    {
        _uiHandler = GetComponent<UiHandler>();
    }

    public override void ManagedStart()
    {
        _uiReference = _uiHandler.mainUiScreen;
        _infoField = _uiReference.elements.experimentTab.outputsModule.info;
        _warningsField = _uiReference.elements.experimentTab.outputsModule.warnings;

        CanUseUpdateMethod = true;
    }



    // todo: rethink, make more abstract (_deferredActions)
    public void PrintToInfo(string message, bool clearTextElement = false)
    {
        /*if (!CanUseUpdateMethod)
        {
            _deferredActions.Enqueue(() => PrintToInfo(message, clearTextElement));
            return;
        }*/

        if (clearTextElement) _infoField.text = "";
        _infoField.text += message;
    }

    public void PrintToWarnings(string message, bool clearTextElement = false)
    {
        /*if (!CanUseUpdateMethod)
        {
            _deferredActions.Enqueue(() => PrintToWarnings(message, clearTextElement));
            return;
        }*/

        if (clearTextElement) _warningsField.text = "";
        _warningsField.text += message;
    }




    public void ControllerButtonWasPressed(string btn_name)
    {
        _uiReference.GetElement(btn_name).AddToClassList("isActive");
    }

    public void ControllerButtonWasReleased(string btn_name)
    {
        _uiReference.GetElement(btn_name).RemoveFromClassList("isActive");
    }
}
