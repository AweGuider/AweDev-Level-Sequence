using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AweDev.LevelSequence
{
    [CreateAssetMenu(fileName = "Level Definition", menuName = "AweDev/Level Sequence/Level Definition")]
    public class LevelDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _levelId;

        [Header("Display")]
        [SerializeField] private string _displayName;
        [TextArea]
        [SerializeField] private string _description;
#if UNITY_EDITOR
        [SerializeField] private Texture2D _referenceImage;
#endif

        [Header("Scene")]
#if UNITY_EDITOR
        [SerializeField] private SceneAsset _sceneAsset;
#endif
        [SerializeField] private string _sceneName;
        [SerializeField] private string _scenePath;

        public string levelId => _levelId;
        public string displayName => _displayName;
        public string description => _description;
        public string sceneName => _sceneName;
        public string scenePath => _scenePath;

#if UNITY_EDITOR
        public Texture2D referenceImage => _referenceImage;
        public SceneAsset sceneAsset => _sceneAsset;

        protected virtual void OnValidate()
        {
            if (!_sceneAsset) return;

            _sceneName = _sceneAsset.name;
            _scenePath = AssetDatabase.GetAssetPath(_sceneAsset);
        }
#endif
    }
}
