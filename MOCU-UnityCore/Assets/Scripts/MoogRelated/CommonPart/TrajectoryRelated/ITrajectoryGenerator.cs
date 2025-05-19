namespace MoogModule
{
    public interface ITrajectoryGenerator
    {
        // 'MoveByTrajectoryParameters' gets from the constructor

        public DofParameters[] GetWholePath(int fps);
        public DofParameters? GetNextPosition();
    }
}