namespace AweDev.LevelSequence
{
    public readonly struct LevelNavigationFailure
    {
        public LevelNavigationFailure(LevelDefinition level, string requestedLevelId, string reason)
        {
            this.level = level;
            this.requestedLevelId = requestedLevelId;
            this.reason = reason;
        }

        public readonly LevelDefinition level;
        public readonly string requestedLevelId;
        public readonly string reason;
    }
}
