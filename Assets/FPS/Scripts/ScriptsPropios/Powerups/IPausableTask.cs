using Unity.FPS.Ours;

namespace Unity.FPS.Game
{
    public interface IPausableTask : IQueueTask
    {
        void Pause();   // must snapshot internal state and stop affecting gameplay
    }
}