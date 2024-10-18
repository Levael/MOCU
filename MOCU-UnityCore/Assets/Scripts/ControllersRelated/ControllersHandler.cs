using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class ControllersHandler : MonoBehaviour, IFullyControllable
{
    // todo:
    private Dictionary<InputDevice, (HashSet<ActionMapName> supportedActionMaps, HashSet<ActionMapName> activeActionMaps)> _devices;

    // new()
    // Bind()
    // event StatusHasChanged +=




    public ModuleStatusHandler<ControllerDevice_SubStatuses> ControllerConnectionStatus;
    public bool IsComponentReady { get; private set; }
    //public IReadOnlyList<IControllerDevice> Devices { get => _devices; }


    //private List<IControllerDevice> _devices;
    private float _checkDeviceConnectionTimeInterval;   // sec


    public void ControllableAwake()
    {
        _checkDeviceConnectionTimeInterval = 0.1f;
        ControllerConnectionStatus = new();

        /*_devices = new List<IControllerDevice>
        {
            //new CedrusHandler()
        };*/
    }

    public async void ControllableStart()
    {
        /*foreach (var device in _devices)
        {
            await device.Init();
        }

        IsComponentReady = true;*/
    }
    

    public void ControllableUpdate()
    {
        /*foreach (var device in _devices)
        {
            if (device.ConnectionStatus == ModuleStatus.FullyOperational)
            {
                ControllerConnectionStatus.UpdateSubStatus(ControllerDevice_SubStatuses.isConnected, SubStatusState.Complete);
                return;
            }

            ControllerConnectionStatus.UpdateSubStatus(ControllerDevice_SubStatuses.isConnected, SubStatusState.Failed);
            //print($"{device.DisplayName}: {device.ConnectionStatus}");
        }*/
    }

    public bool CanDeviceTriggerActionMap(InputDevice device, ActionMapName actionMap)
    {
        var x = Gamepad.all;
        return _devices[device].supportedActionMaps.Contains(actionMap) && _devices[device].activeActionMaps.Contains(actionMap);
    }

    /*private IEnumerator CheckGamepadConnection(float checkConnectionTimeInterval)
    {
        while (true)
        {
            try
            {
                if (Gamepad.current?.enabled ?? false)
                    GamepadConnectionStatus.UpdateSubState(ControllerDevice_Statuses.isConnected, true);
                else
                    GamepadConnectionStatus.UpdateSubState(ControllerDevice_Statuses.isConnected, false);
            }
            catch
            {
                GamepadConnectionStatus.UpdateSubState(ControllerDevice_Statuses.isConnected, false);
            }

            yield return new WaitForSeconds(checkConnectionTimeInterval);
        }
    }*/
}