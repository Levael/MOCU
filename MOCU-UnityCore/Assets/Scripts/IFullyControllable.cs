/*
    This code defines a set of interfaces to enforce a controlled execution order of component lifecycle methods (Awake, Start, and Update) in Unity.
    A central MonoBehaviour class is responsible for invoking these lifecycle methods across various components in a consistent sequence.
    
    The central MonoBehaviour collects these components and invokes their Awake, Start, and Update equivalents in a predefined order,
    ensuring consistency and control over the lifecycle management.
 */


public interface IComponentReadiness
{
    bool IsComponentReady { get; }
}


public interface IControllableInitiation : IComponentReadiness
{
    void ControllableAwake();
    void ControllableStart();
}


public interface IFullyControllable : IControllableInitiation
{
    void ControllableUpdate();
}