/// <summary>
/// Interface for components that require controlled initialization order within the Unity lifecycle.
/// Implementing classes will have their initialization methods explicitly called by a controlling class,
/// such as Bootstrap, allowing for precise control over the sequence of Awake and Start operations.
/// </summary>
public interface IControllableComponent
{
    /// <summary>
    /// Method to be called during the controlled Awake phase.
    /// This method is intended to perform any initialization that would normally occur in Unity's Awake.
    /// </summary>
    void ControllableAwake();

    /// <summary>
    /// Method to be called during the controlled Start phase.
    /// This method is intended to perform any initialization that would normally occur in Unity's Start.
    /// </summary>
    void ControllableStart();

    bool IsComponentReady { get; }
}