using UnityEngine;
using UnityEngine.UIElements;

public class ExperimentTabHandler : MonoBehaviour
{
    private UiHandler _uiHandler;
    private UiReferences _uiReference;

    private TextElement _infoField;
    private TextElement _warningsField;



    void Awake()
    {
        _uiHandler = GetComponent<UiHandler>();
    }

    void Start()
    {
        Debug.Log("Entered 'ExperimentTabHandler' Start");
        _uiReference = _uiHandler.mainTabScreen;
        _infoField = (TextElement)_uiReference.GetElement("info-module-textbox");
        _warningsField = (TextElement)_uiReference.GetElement("warnings-module-textbox");
        Debug.Log("Exited 'ExperimentTabHandler' Start");
    }

    void Update()
    {
    }




    public void PrintToInfo(string message, bool clearTextElement = false)
    {
        if (clearTextElement) _infoField.text = "";

        _infoField.text += message;
    }

    public void PrintToWarnings(string message, bool clearTextElement = false)
    {
        if (clearTextElement) _warningsField.text = "";

        _warningsField.text += message;
    }
}
