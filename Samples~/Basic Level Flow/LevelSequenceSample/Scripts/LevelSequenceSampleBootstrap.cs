using UnityEngine;

namespace AweDev.LevelSequence.Samples
{
    public class LevelSequenceSampleBootstrap : MonoBehaviour
    {
        public const string MenuLevelId = "sample_menu";
        public const string LevelAId = "sample_level_a";
        public const string LevelBId = "sample_level_b";

        private static LevelSequenceSampleBootstrap _instance;

        [SerializeField] private LevelSequenceNavigatorBehaviour _navigator;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            if (_navigator == null)
            {
                _navigator = GetComponent<LevelSequenceNavigatorBehaviour>();
            }
        }

        public void LoadMenu()
        {
            LoadLevelById(MenuLevelId);
        }

        public void LoadLevelA()
        {
            LoadLevelById(LevelAId);
        }

        public void LoadLevelB()
        {
            LoadLevelById(LevelBId);
        }

        public void LoadNextLevel()
        {
            if (_navigator == null) return;

            _navigator.MoveToNextLevel();
        }

        public void RestartCurrentLevel()
        {
            if (_navigator == null || _navigator.currentLevel == null) return;

            _navigator.SetLevel(_navigator.currentLevel);
        }

        private void LoadLevelById(string levelId)
        {
            EnsureNavigator();
            if (_navigator == null) return;

            _navigator.SetLevelById(levelId);
        }

        private void EnsureNavigator()
        {
            if (_navigator != null) return;

            _navigator = GetComponent<LevelSequenceNavigatorBehaviour>();
        }
    }
}
