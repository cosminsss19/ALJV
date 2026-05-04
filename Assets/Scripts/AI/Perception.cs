using UnityEngine;

namespace AI
{
    [RequireComponent(typeof(Blackboard))]
    public class Perception : MonoBehaviour
    {
        [Header("Sight")]
        public float SightRange = 12f;
        public float SightConeHalfAngle = 60f;
        public float EyeHeight = 1.6f;
        public LayerMask SightObstacleMask = ~0;

        [Header("Hearing (passive)")]
        public float HearingRange = 10f;
        public float HearingSpeedThreshold = 1.5f;
        public float HearingSpeedRangeBonus = 1f;

        [Header("Debug")]
        public bool LogStateChanges = false;

        [Header("Detection Sound")]
        public float HearingFreshnessWindow = 0.2f;

        private Blackboard _bb;
        private Vector3 _lastPlayerPos;
        private bool _hasLastPlayerPos;
        private bool _lastSeen;
        private bool _wasHearing;

        private void Awake()
        {
            _bb = GetComponent<Blackboard>();
        }

        private void OnEnable()
        {
            NoiseChannel.OnNoise += HandleNoise;
        }

        private void OnDisable()
        {
            NoiseChannel.OnNoise -= HandleNoise;
        }

        private void Update()
        {
            if (_bb == null) return;

            bool sees = ComputeCanSee();
            _bb.CanSeePlayer = sees;
            if (sees)
            {
                _bb.LastSeenTime = Time.time;
                _bb.LastKnownPlayerPosition = _bb.Player.transform.position;
            }

            CheckHearingFromMovement();

            if (sees && !_lastSeen)
            {
                SoundManager.Instance.PlaySpotted();
            }

            bool isHearing = (Time.time - _bb.LastHeardTime) <= HearingFreshnessWindow;
            if (isHearing && !_wasHearing)
            {
                SoundManager.Instance.PlayHeard();
            }
            _wasHearing = isHearing;

            if (LogStateChanges && sees != _lastSeen)
            {
                Debug.Log($"{name} sight={(sees ? "VISIBLE" : "lost")} dist={(_bb.Player != null ? Vector3.Distance(transform.position, _bb.Player.transform.position).ToString("F2") : "n/a")}");
            }
            _lastSeen = sees;
        }

        private bool ComputeCanSee()
        {
            if (_bb.Player == null) return false;

            Vector3 eye = transform.position + Vector3.up * EyeHeight;
            Vector3 playerEye = _bb.Player.transform.position + Vector3.up * EyeHeight;
            Vector3 toPlayer = playerEye - eye;
            float dist = toPlayer.magnitude;
            if (dist > SightRange) return false;

            Vector3 forwardFlat = transform.forward; forwardFlat.y = 0f;
            Vector3 dirFlat = toPlayer; dirFlat.y = 0f;
            if (forwardFlat.sqrMagnitude < 0.0001f || dirFlat.sqrMagnitude < 0.0001f) return false;
            float angle = Vector3.Angle(forwardFlat.normalized, dirFlat.normalized);
            if (angle > SightConeHalfAngle) return false;

            Transform selfRoot = transform.root;
            Transform playerRoot = _bb.Player.transform.root;

            RaycastHit[] hits = Physics.RaycastAll(eye, toPlayer.normalized, dist, SightObstacleMask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hits.Length; i++)
            {
                Transform hitRoot = hits[i].collider.transform.root;
                if (hitRoot == selfRoot) continue;
                if (hitRoot == playerRoot) continue;
                return false;
            }
            return true;
        }

        private void CheckHearingFromMovement()
        {
            if (_bb.Player == null) return;
            Vector3 cur = _bb.Player.transform.position;

            if (!_hasLastPlayerPos)
            {
                _lastPlayerPos = cur;
                _hasLastPlayerPos = true;
                return;
            }

            float dt = Time.deltaTime;
            if (dt < 0.0001f) { _lastPlayerPos = cur; return; }

            Vector3 delta = cur - _lastPlayerPos;
            delta.y = 0f;
            float speed = delta.magnitude / dt;
            _lastPlayerPos = cur;

            if (speed < HearingSpeedThreshold) return;

            float effectiveRange = HearingRange + (speed - HearingSpeedThreshold) * HearingSpeedRangeBonus;
            Vector3 self = transform.position; self.y = 0f;
            Vector3 src = cur; src.y = 0f;
            if (Vector3.Distance(self, src) <= effectiveRange)
            {
                _bb.LastHeardTime = Time.time;
                _bb.LastHeardPosition = cur;
            }
        }

        private void HandleNoise(Vector3 position, float loudness)
        {
            float dist = Vector3.Distance(transform.position, position);
            if (dist <= HearingRange + loudness)
            {
                _bb.LastHeardTime = Time.time;
                _bb.LastHeardPosition = position;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 eye = transform.position + Vector3.up * EyeHeight;
            Gizmos.color = new Color(1f, 1f, 0f, 0.6f);
            Vector3 left = Quaternion.Euler(0f, -SightConeHalfAngle, 0f) * transform.forward;
            Vector3 right = Quaternion.Euler(0f, SightConeHalfAngle, 0f) * transform.forward;
            Gizmos.DrawLine(eye, eye + left * SightRange);
            Gizmos.DrawLine(eye, eye + right * SightRange);
            Gizmos.DrawLine(eye, eye + transform.forward * SightRange);

            Gizmos.color = new Color(0f, 0.6f, 1f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, HearingRange);
        }
    }
}
