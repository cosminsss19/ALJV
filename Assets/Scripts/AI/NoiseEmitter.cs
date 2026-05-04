using UnityEngine;

namespace AI
{
    public class NoiseEmitter : MonoBehaviour
    {
        [Tooltip("Loudness when moving: any listener within HearingRange + MovementLoudness can hear it.")]
        public float MovementLoudness = 4f;

        [Tooltip("Minimum interval between noise emissions, in seconds.")]
        public float Interval = 0.25f;

        [Tooltip("Minimum distance moved between emissions for the movement to count as noise.")]
        public float MinMovementToEmit = 0.05f;

        private Vector3 _lastEmitPosition;
        private float _nextEmitTime;

        private void Start()
        {
            _lastEmitPosition = transform.position;
        }

        private void Update()
        {
            if (Time.time < _nextEmitTime) return;

            float moved = Vector3.Distance(transform.position, _lastEmitPosition);
            if (moved >= MinMovementToEmit)
            {
                NoiseChannel.Emit(transform.position, MovementLoudness);
            }

            _lastEmitPosition = transform.position;
            _nextEmitTime = Time.time + Interval;
        }
    }
}
