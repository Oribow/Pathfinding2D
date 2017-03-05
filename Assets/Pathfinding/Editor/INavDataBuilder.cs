namespace Pathfinding2d.NavDataGeneration
{
    public interface INavDataBuilder
    {
        NavData2dBuildContainer GlobalBuildContainer { get; set; }
        void TriggerRepaint();
    }
}
