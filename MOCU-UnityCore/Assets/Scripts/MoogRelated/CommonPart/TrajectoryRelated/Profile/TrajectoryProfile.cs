namespace MoogModule
{
    /// <summary>
    /// Specifies the motion profile that defines how the distance along a trajectory evolves over time,
    /// independently of the spatial shape of the path. While <see cref="TrajectoryType"/> determines the geometry
    /// of the path (e.g., linear, circular), <see cref="TrajectoryProfile"/> determines the timing of the movement
    /// along that path (e.g., uniform speed, acceleration curve).
    /// </summary>
    public enum TrajectoryProfile
    {
        None,
        Linear,
        CDF

        // Bezier
        // Smooth
        // ...
    }
}