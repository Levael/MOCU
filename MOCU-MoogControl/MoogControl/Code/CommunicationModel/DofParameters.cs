/**
 * DOF (Degrees of Freedom) Parameters:
 * 
 * 
 * Coordinate system and rotations based on the Moog 6DOF2000E Motion System:
 * 
 * - 'Front' and 'Rear' orientations are consistent with the documentation and our setup (e.g. 'rear' = closer to the exit).
 * - All directions (+/-) are relative to a view from the REAR or from ABOVE (top view).
 * - All parameters are in meters (linear) and radians (angular). Default values: 0.
 *
 *
 * Parameters:                                                                      Displacement:       Velocity:       Acceleration:
 * 
 * Roll  - Rotation around longitudinal axis (tilting right/left sides).            (+-21deg)           (+-30deg/s)     (+-500deg/s^2)
 * Pitch - Rotation around lateral axis (tilting backward/forward, front up/down).  (+-22deg)           (+-30deg/s)     (+-500deg/s^2)
 * Yaw   - Rotation around vertical axis (horizontal rotation right/left).          (+-22deg)           (+-40deg/s)     (+-400deg/s^2)
 *
 * Heave - Vertical linear movement (down/up).                                      (+-0.178m)          (+-0.3m/s)      (-0.5G,+0.7G)
 * Surge - Longitudinal linear movement (forward/backward).                         (+0.259m,-0.241m)   (+-0.5m/s)      (+-0.6G)
 * Sway  - Lateral linear movement (right/left).                                    (+-0.259m)          (+-0.5m/s)      (+-0.6G)
 */


using System.Runtime.InteropServices;


namespace MoogModule.Daemon
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DofParameters
    {
        public float Roll;
        public float Pitch;
        public float Yaw;
        public float Surge;
        public float Sway;
        public float Heave;
    }
}