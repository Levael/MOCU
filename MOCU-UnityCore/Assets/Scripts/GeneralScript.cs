using UnityEngine;



public class GeneralScript : MonoBehaviour
{
    private IControllableComponent[] _components;


    private void Awake()
    {
        _components = new IControllableComponent[]
        {
            EnsureComponent<UiHandler>(),
            EnsureComponent<ConfigHandler>(),   // top
            EnsureComponent<StatusesHandler>(),
            EnsureComponent<AudioHandler>(),
            EnsureComponent<DaemonsHandler>(),
            EnsureComponent<InputLogic>(),
            EnsureComponent<AnswerHandler>(),
        };


        foreach (var component in _components)
            component?.ControllableAwake();
    }

    private void Start()
    {
        QualitySettings.vSyncCount = 0;         // Disable VSync
        Application.targetFrameRate = 60;       // Application fps (when VR is on -- automatically switchs to VR fps)


        foreach (var component in _components)
            component?.ControllableStart();
    }



    private T EnsureComponent<T>() where T : Component, IControllableComponent
    {
        T component = GetComponent<T>();

        if (component == null)
            component = gameObject.AddComponent<T>();

        return component;
    }
}