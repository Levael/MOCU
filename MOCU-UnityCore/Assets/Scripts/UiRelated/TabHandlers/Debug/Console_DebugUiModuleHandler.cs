using UnityEngine;
using UnityEngine.UIElements;


public class Console_DebugUiModuleHandler : MonoBehaviour, IControllableInitiation
{
    public bool IsComponentReady { get; private set; }

    private UiHandler _uiHandler;
    private UiReferences _uiReference;

    private TextElement _console;


    public void ControllableAwake() { }

    public void ControllableStart()
    {
        _uiHandler = GetComponent<UiHandler>();
        _uiReference = _uiHandler.secondaryUiScreen;
        _console = _uiReference.elements.debugTab.consoleModule.console;
        IsComponentReady = true;
    }


    public void PrintToConsole(string message, bool clearTextElement = false)
    {
        if (clearTextElement) _console.text = "";
        _console.text += $"{message}\n";
    }
}
