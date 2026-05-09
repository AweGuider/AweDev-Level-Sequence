using System;
using System.Collections.Generic;

namespace AweDev.LevelSequence
{
    public sealed class LevelSequenceNavigator
    {
        private readonly Dictionary<string, LevelDefinition> _levelsById = new();
        private readonly Dictionary<string, LevelDefinition> _levelsBySceneName = new();
        private LevelSequence _levelSequence;
        private ILevelSceneLoader _sceneLoader;
        private LevelDefinition _currentLevel;

        public LevelSequenceNavigator(LevelSequence levelSequence, ILevelSceneLoader sceneLoader = null)
        {
            _sceneLoader = sceneLoader ?? new UnitySceneLoader();
            SetLevelSequence(levelSequence);
        }

        public LevelSequence levelSequence => _levelSequence;
        public ILevelSceneLoader sceneLoader => _sceneLoader;
        public LevelDefinition currentLevel => _currentLevel;

        public event Action<LevelDefinition> CurrentLevelChanged;
        public event Action<LevelDefinition> LevelLoadRequested;
        public event Action<LevelDefinition> LevelSceneLoaded;
        public event Action<LevelNavigationFailure> NavigationFailed;

        public void SetLevelSequence(LevelSequence levelSequence)
        {
            _levelSequence = levelSequence;
            RebuildLookups();

            if (_currentLevel != null && GetLevelIndex(_currentLevel) < 0)
            {
                _currentLevel = null;
                CurrentLevelChanged?.Invoke(null);
            }
        }

        public void SetSceneLoader(ILevelSceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader ?? new UnitySceneLoader();
        }

        public void RebuildLookups()
        {
            _levelsById.Clear();
            _levelsBySceneName.Clear();

            if (_levelSequence == null) return;

            foreach (LevelDefinition level in _levelSequence.levels)
            {
                if (level == null) continue;

                if (!string.IsNullOrWhiteSpace(level.levelId) && !_levelsById.ContainsKey(level.levelId))
                {
                    _levelsById.Add(level.levelId, level);
                }

                if (!string.IsNullOrWhiteSpace(level.sceneName) && !_levelsBySceneName.ContainsKey(level.sceneName))
                {
                    _levelsBySceneName.Add(level.sceneName, level);
                }
            }
        }

        public bool TryGetLevelById(string levelId, out LevelDefinition level)
        {
            if (string.IsNullOrWhiteSpace(levelId))
            {
                level = null;
                return false;
            }

            return _levelsById.TryGetValue(levelId, out level);
        }

        public bool TryGetLevelBySceneName(string sceneName, out LevelDefinition level)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                level = null;
                return false;
            }

            return _levelsBySceneName.TryGetValue(sceneName, out level);
        }

        public int GetLevelIndex(string levelId)
        {
            return TryGetLevelById(levelId, out LevelDefinition level)
                ? GetLevelIndex(level)
                : -1;
        }

        public int GetLevelIndex(LevelDefinition level)
        {
            if (_levelSequence == null || level == null) return -1;

            for (int i = 0; i < _levelSequence.levels.Count; i++)
            {
                if (_levelSequence.levels[i] == level) return i;
            }

            return -1;
        }

        public bool TryGetNextLevel(out LevelDefinition level)
        {
            level = null;

            if (_levelSequence == null || _currentLevel == null) return false;

            int currentLevelIndex = GetLevelIndex(_currentLevel);
            if (currentLevelIndex < 0 || currentLevelIndex >= _levelSequence.levels.Count - 1) return false;

            level = _levelSequence.levels[currentLevelIndex + 1];
            return level != null;
        }

        public bool SetCurrentLevel(LevelDefinition level, bool notifyLoaded)
        {
            if (level == null)
            {
                PublishFailure(null, null, "Level is missing.");
                return false;
            }

            _currentLevel = level;
            CurrentLevelChanged?.Invoke(_currentLevel);

            if (notifyLoaded)
            {
                LevelSceneLoaded?.Invoke(_currentLevel);
            }

            return true;
        }

        public bool TrySetCurrentLevelBySceneName(string sceneName, out LevelDefinition level, bool notifyLoaded)
        {
            if (!TryGetLevelBySceneName(sceneName, out level))
            {
                PublishFailure(null, null, $"Scene '{sceneName}' was not found in the Level Sequence.");
                return false;
            }

            return SetCurrentLevel(level, notifyLoaded);
        }

        public bool SetLevelById(string levelId)
        {
            if (!TryGetLevelById(levelId, out LevelDefinition level))
            {
                PublishFailure(null, levelId, $"No level found for levelId '{levelId}'.");
                return false;
            }

            return SetLevel(level);
        }

        public bool SetLevel(LevelDefinition level)
        {
            if (level == null)
            {
                PublishFailure(null, null, "Level is missing.");
                return false;
            }

            if (_sceneLoader == null)
            {
                PublishFailure(level, level.levelId, "Scene loader is missing.");
                return false;
            }

            if (!_sceneLoader.TryLoadLevel(level, out string failureReason))
            {
                PublishFailure(level, level.levelId, failureReason);
                return false;
            }

            _currentLevel = level;
            CurrentLevelChanged?.Invoke(_currentLevel);
            LevelLoadRequested?.Invoke(level);
            return true;
        }

        public bool MoveToNextLevel()
        {
            if (!TryGetNextLevel(out LevelDefinition nextLevel))
            {
                string reason = _currentLevel == null
                    ? "Current level is not resolved."
                    : $"No next level available after '{_currentLevel.name}'.";

                PublishFailure(_currentLevel, _currentLevel != null ? _currentLevel.levelId : null, reason);
                return false;
            }

            return SetLevel(nextLevel);
        }

        private void PublishFailure(LevelDefinition level, string requestedLevelId, string reason)
        {
            NavigationFailed?.Invoke(new LevelNavigationFailure(level, requestedLevelId, reason));
        }
    }
}
