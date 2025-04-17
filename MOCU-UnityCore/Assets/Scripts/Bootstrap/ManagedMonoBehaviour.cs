using UnityEngine;


public class ManagedMonoBehaviour : MonoBehaviour
{
    /// <summary>
    /// Method 'ManagedUpdate' woun't be called unless this property is set to true.
    /// </summary>
    public bool IsComponentReady { get; protected set; } = false;

    public virtual void ManagedAwake() { }
    public virtual void ManagedStart() { }
    public virtual void ManagedUpdate() { }
}