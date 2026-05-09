using System.Collections.Generic;
using System.Linq;

namespace AweDev.LevelSequence.Editor
{
    public static class LevelSequenceValidator
    {
        public static List<DuplicateLevelIdGroup> FindDuplicateLevelIds(LevelSequence levelSequence)
        {
            Dictionary<string, List<LevelDefinition>> levelsById = new();

            if (levelSequence == null) return new List<DuplicateLevelIdGroup>();

            foreach (LevelDefinition level in levelSequence.levels)
            {
                if (level == null || string.IsNullOrWhiteSpace(level.levelId)) continue;

                if (!levelsById.TryGetValue(level.levelId, out List<LevelDefinition> levels))
                {
                    levels = new List<LevelDefinition>();
                    levelsById.Add(level.levelId, levels);
                }

                levels.Add(level);
            }

            return levelsById
                .Where(pair => pair.Value.Count > 1)
                .Select(pair => new DuplicateLevelIdGroup(pair.Key, pair.Value))
                .ToList();
        }

        public static string FormatDuplicateIds(IEnumerable<DuplicateLevelIdGroup> duplicateGroups)
        {
            return string.Join(", ", duplicateGroups.Select(group => $"'{group.levelId}'"));
        }

        public static string FormatDuplicateGroup(DuplicateLevelIdGroup duplicateGroup)
        {
            string assetNames = string.Join(", ", duplicateGroup.levels.Select(level => level.name));
            return $"{duplicateGroup.levelId}: {assetNames}";
        }
    }

    public class DuplicateLevelIdGroup
    {
        public DuplicateLevelIdGroup(string levelId, IReadOnlyList<LevelDefinition> levels)
        {
            this.levelId = levelId;
            this.levels = levels;
        }

        public readonly string levelId;
        public readonly IReadOnlyList<LevelDefinition> levels;
    }
}
