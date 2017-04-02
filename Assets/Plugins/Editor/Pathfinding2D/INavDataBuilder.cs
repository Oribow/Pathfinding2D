namespace NavGraph.Build
{
    public interface INavDataBuilder
    {
        BuildProcessSave GlobalBuildContainer { get; set; }
        void TriggerRepaint();
    }
}
