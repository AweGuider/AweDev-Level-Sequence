using UnityEngine;

namespace AweDev.LevelSequence.Samples
{
    [RequireComponent(typeof(Rigidbody))]
    public class LevelSequenceTriggerTesterMover : MonoBehaviour
    {
        [SerializeField] private float _speed = 4f;

        private Rigidbody _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.useGravity = false;
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        }

        private void FixedUpdate()
        {
            Vector3 direction = Vector3.zero;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) direction += Vector3.forward;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) direction += Vector3.back;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) direction += Vector3.left;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) direction += Vector3.right;

            if (direction.sqrMagnitude > 1f)
            {
                direction.Normalize();
            }

            _rigidbody.velocity = direction * _speed;
        }
    }
}
