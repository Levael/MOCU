using System;
using System.Collections.Generic;


public class GamepadsHandler
{
    public Dictionary<ActionMapName, Dictionary<string, Enum>> actionMapControls;


    public GamepadsHandler()
    {
        actionMapControls = new ()
        {
            {
                ActionMapName.DPad, new ()
                {
                    { "<Gamepad>/leftTrigger",      DPadAction.Left },
                    { "<Gamepad>/leftShoulder",     DPadAction.Left },
                    { "<Gamepad>/rightTrigger",     DPadAction.Right },
                    { "<Gamepad>/rightShoulder",    DPadAction.Right },
                    { "<Gamepad>/dpad/up",          DPadAction.Center },
                    { "<Gamepad>/dpad/down",        DPadAction.Center },
                    { "<Gamepad>/dpad/left",        DPadAction.Center },
                    { "<Gamepad>/dpad/right",       DPadAction.Center },
                }
            },
            {
                ActionMapName.Joystick, new ()
                {
                    { "<Gamepad>/leftStick",        JoystickAction.LeftStickMove },
                    { "<Gamepad>/leftStickButton",  JoystickAction.LeftStickPress },
                    { "<Gamepad>/rightStick",       JoystickAction.RightStickMove },
                    { "<Gamepad>/rightStickButton", JoystickAction.RightStickPress },
                }
            },
            {
                ActionMapName.Intercom, new ()
                {
                    { "<Gamepad>/buttonNorth",      IntercomAction.Input },
                    { "<Gamepad>/buttonWest",       IntercomAction.Input },
                    { "<Gamepad>/buttonEast",       IntercomAction.Input },
                    { "<Gamepad>/buttonSouth",      IntercomAction.Input },
                }
            }
        };
    }
}