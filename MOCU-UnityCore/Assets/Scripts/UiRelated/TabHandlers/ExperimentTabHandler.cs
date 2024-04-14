using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Concurrent;
using System;


public class ExperimentTabHandler : MonoBehaviour
{
    private UiHandler _uiHandler;
    private UiReferences _uiReference;

    private TextElement _infoField;
    private TextElement _warningsField;

    private ConcurrentQueue<Action> _deferredActions = new ConcurrentQueue<Action>();   // todo: describe later
    private bool _classIsReady = false;




    void Awake()
    {
        _uiHandler = GetComponent<UiHandler>();
    }

    void Start()
    {
        _uiReference = _uiHandler.mainUiScreen;
        _infoField = (TextElement)_uiReference.GetElement("info-module-textbox");
        _warningsField = (TextElement)_uiReference.GetElement("warnings-module-textbox");

        _classIsReady = true;
    }

    void Update()
    {
        while (_deferredActions.TryDequeue(out Action action))
        {
            action.Invoke();
        }
    }




    public void PrintToInfo(string message, bool clearTextElement = false)
    {
        if (!_classIsReady)
        {
            _deferredActions.Enqueue(() => PrintToInfo(message, clearTextElement));
            return;
        }

        if (clearTextElement) _infoField.text = "";
        _infoField.text += message;
    }

    public void PrintToWarnings(string message, bool clearTextElement = false)
    {
        if (!_classIsReady)
        {
            _deferredActions.Enqueue(() => PrintToWarnings(message, clearTextElement));
            return;
        }

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
