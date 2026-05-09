using System.Collections.Generic;
using UnityEngine;

namespace AweDev.LevelSequence
{
    [CreateAssetMenu(fileName = "Level Sequence", menuName = "AweDev/Level Sequence/Level Sequence")]
    public class LevelSequence : ScriptableObject
    {
        [SerializeField] private LevelDefinition _buildStartLevel;
        [SerializeField] private List<LevelDefinition> _levels = new();

        public LevelDefinition buildStartLevel => _buildStartLevel;
        public IReadOnlyList<LevelDefinition> levels => _levels;

        public LevelDefinition GetEffectiveBuildStartLevel()
        {
            if (_buildStartLevel) return _buildStartLevel;

            foreach (LevelDefinition level in _levels)
            {
                if (level) return level;
            }

            return null;
        }
    }
}
