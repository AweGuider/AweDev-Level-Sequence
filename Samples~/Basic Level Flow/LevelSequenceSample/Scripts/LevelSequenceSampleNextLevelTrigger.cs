using UnityEngine;

namespace AweDev.LevelSequence.Samples
{
    public class LevelSequenceSampleNextLevelTrigger : MonoBehaviour
    {
        private enum TriggerAction
        {
            MoveToNextLevel,
            LoadLevel
        }

        [SerializeField] private TriggerAction _action = TriggerAction.MoveToNextLevel;
        [SerializeField] private LevelDefinition _level;
        [SerializeField] private LevelSequenceNavigatorBehaviour _navigator;

        private void OnTriggerEnter(Collider other)
        {
            RunTriggerAction();
        }

        private void RunTriggerAction()
        {
            if (_navigator == null)
            {
                _navigator = FindObjectOfType<LevelSequenceNavigatorBehaviour>();
            }

            if (_navigator == null) return;

            if (_action == TriggerAction.LoadLevel)
            {
                _navigator.SetLevel(_level);
                return;
            }

            _navigator.MoveToNextLevel();
        }
    }
}
