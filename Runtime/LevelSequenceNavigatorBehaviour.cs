using UnityEngine;
using UnityEngine.SceneManagement;

namespace AweDev.LevelSequence
{
    public class LevelSequenceNavigatorBehaviour : MonoBehaviour
    {
        [SerializeField] private LevelSequence _levelSequence;

        private LevelSequenceNavigator _navigator;

        public LevelSequence levelSequence => _levelSequence;
        public LevelSequenceNavigator navigator => EnsureNavigator();
        public LevelDefinition currentLevel => navigator.currentLevel;

        private void Awake()
        {
            EnsureNavigator();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void Start()
        {
            navigator.TrySetCurrentLevelBySceneName(SceneManager.GetActiveScene().name, out _, true);
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        public void SetLevelSequence(LevelSequence levelSequence)
        {
            _levelSequence = levelSequence;
            navigator.SetLevelSequence(_levelSequence);
        }

        public void SetSceneLoader(ILevelSceneLoader sceneLoader)
        {
            navigator.SetSceneLoader(sceneLoader);
        }

        public bool TryGetLevelById(string levelId, out LevelDefinition level)
        {
            return navigator.TryGetLevelById(levelId, out level);
        }

        public int GetLevelIndex(LevelDefinition level)
        {
            return navigator.GetLevelIndex(level);
        }

        public int GetLevelIndex(string levelId)
        {
            return navigator.GetLevelIndex(levelId);
        }

        public bool SetLevel(LevelDefinition level)
        {
            return navigator.SetLevel(level);
        }

        public bool SetLevelById(string levelId)
        {
            return navigator.SetLevelById(levelId);
        }

        public bool MoveToNextLevel()
        {
            return navigator.MoveToNextLevel();
        }

        private LevelSequenceNavigator EnsureNavigator()
        {
            _navigator ??= new LevelSequenceNavigator(_levelSequence);
            return _navigator;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            navigator.TrySetCurrentLevelBySceneName(scene.name, out _, true);
        }
    }
}
