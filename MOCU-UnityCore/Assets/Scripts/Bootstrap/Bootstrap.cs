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


    private void Awake()
    {
        _components = new MonoBehaviour[]
        {
            // MAIN
            EnsureComponent<GeneralScript>(),
            EnsureComponent<UnityMainThreadDispatcher>(),

            // UI
            EnsureComponent<UiHandler>(),
            EnsureComponent<ExperimentTabHandler>(),
            EnsureComponent<DebugTabHandler>(),
                EnsureComponent<Daemons_DebugUiModuleHandler>(),
                EnsureComponent<DifferentInfo_DebugUiModuleHandler>(),
                EnsureComponent<Console_DebugUiModuleHandler>(),
                EnsureComponent<Fps_DebugUiModuleHandler>(),
            EnsureComponent<SettingsTabHandler>(),
                EnsureComponent<Devices_SettingsUiModuleHandler>(),

            // MODULES
            EnsureComponent<ConfigHandler>(),       // important to be as high as possible
            EnsureComponent<StatusesHandler>(),
            EnsureComponent<AudioHandler>(),
            EnsureComponent<DaemonsHandler>(),
            EnsureComponent<AnswerHandler>(),
            EnsureComponent<InputHandler>(),
                EnsureComponent<CedrusHandler>(),
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



    private T EnsureComponent<T>() where T : MonoBehaviour
    {
        T component = GetComponent<T>();

        if (component == null)
            component = gameObject.AddComponent<T>();

        return component;
    }
}