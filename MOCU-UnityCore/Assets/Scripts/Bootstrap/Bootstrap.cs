/*
 * The Bootstrap class manages the lifecycle of various components in a Unity project by controlling the invocation of initialization and update methods.
 * Specifically, it handles the ControllableAwake, ControllableStart, and ControllableUpdate
 * methods of components implementing IControllableInitiation and IFullyControllable interfaces.
 * The ControllableUpdate method is particularly useful for centralizing the check of IsComponentReady,
 * ensuring that each component only updates when it's ready, thereby avoiding redundant checks within individual components.
 */


using UnityEngine;


public class Bootstrap : MonoBehaviour
{
    private MonoBehaviour[] _components;
    private GameObject _gameObject_scripts;
    private GameObject _gameObject_guiMainMonitor;
    private GameObject _gameObject_guiSecondaryMonitor;


    private void Awake()
    {
        _gameObject_scripts = GameObject.Find("Scripts");
        _gameObject_guiMainMonitor = GameObject.Find("GUI_main_monitor");
        _gameObject_guiSecondaryMonitor = GameObject.Find("GUI_second_monitor");

        _components = new MonoBehaviour[]
        {
            // MAIN
            EnsureComponent<GeneralScript>(),
            EnsureComponent<UnityMainThreadDispatcher>(),

            // UI
            EnsureComponent<UiReferences>(_gameObject_guiMainMonitor),
            EnsureComponent<UiReferences>(_gameObject_guiSecondaryMonitor),
            EnsureComponent<UiHandler>(),
            EnsureComponent<ExperimentTabHandler>(),
            EnsureComponent<DebugTabHandler>(),
                EnsureComponent<Daemons_DebugUiModuleHandler>(),
                EnsureComponent<DifferentInfo_DebugUiModuleHandler>(),
                EnsureComponent<Console_DebugUiModuleHandler>(),
                EnsureComponent<Fps_DebugUiModuleHandler>(),
            EnsureComponent<SettingsTabHandler>(),
                /*EnsureComponent<Devices_SettingsUiModuleHandler>(),*/

            // MODULES
            EnsureComponent<ConfigHandler>(),       // important to be as high as possible
            EnsureComponent<StatusesHandler>(),
            /*EnsureComponent<AudioHandler>(),*/    // TODO: working on it
            /*EnsureComponent<DaemonsHandler>(),*/
            EnsureComponent<AnswerHandler>(),
            EnsureComponent<ControllersHandler>(),
            EnsureComponent<InputHandler>(),
            EnsureComponent<VrHandler>(),
            EnsureComponent<InputLogic>(),

            // TESTS
            EnsureComponent<ForTests>(),
        };


        foreach (var component in _components)
            if (component is IControllableInitiation controllableComponent)
                controllableComponent.ControllableAwake();
    }

    private void Start()
    {
        foreach (var component in _components)
            if (component is IControllableInitiation controllableComponent)
                controllableComponent.ControllableStart();
    }

    private void Update()
    {
        foreach (var component in _components)
            if (component is IFullyControllable controllableComponent && controllableComponent.IsComponentReady)
                controllableComponent.ControllableUpdate();   
    }



    private T EnsureComponent<T>(GameObject GO = null) where T : MonoBehaviour
    {
        GO ??= _gameObject_scripts;
        T component = GetComponent<T>();

        if (component == null)
            component = GO.AddComponent<T>();

        return component;
    }
}