using UnityEngine;
using UnityEngine.UIElements;


public class Console_DebugUiModuleHandler : MonoBehaviour
{
    private UiHandler _uiHandler;
    private UiReferences _uiReference;

    private TextElement _console;


    void Awake()
    {
        _uiHandler = GetComponent<UiHandler>();
    }

    void Start()
    {
        _uiReference = _uiHandler.secondaryUiScreen;

        _console = _uiReference.elements.debugTab.consoleModule.console;
    }

    private void Update() { }



    public void PrintToConsole(string message, bool clearTextElement = false)
    {
        if (clearTextElement) _console.text = "";
        _console.text += $"{message}\n";
    }
}
