namespace AweDev.LevelSequence
{
    public interface ILevelSceneLoader
    {
        bool TryLoadLevel(LevelDefinition level, out string failureReason);
    }
}
