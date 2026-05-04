using UnityEngine;
using System.Collections.Generic;

namespace AI
{
    [RequireComponent(typeof(Blackboard))]
    [RequireComponent(typeof(Perception))]
    public class EnemyController : MonoBehaviour
    {
        public GridManager Grid;

        [Header("Movement")]
        public float PatrolSpeed = 1.4f;
        public float InvestigateSpeed = 2.2f;
        public float ChaseSpeed = 3.2f;
        public float StoppingDistance = 0.2f;

        [Header("Pathfinding")]
        public float ChaseRepathInterval = 0.25f;
        public float InvestigateRepathThreshold = 0.5f;

        [Header("Investigate")]
        public float LookAroundDuration = 2f;
        public float LookAroundSpeed = 60f;

        [Header("Animation")]
        public float SpeedDampTime = 0.1f;

        private Blackboard _bb;
        private int _currentPathIndex = 0;
        private float _nextRepathTime = 0f;
        private BTNode _root;

        private AIMode _previousMode;
        private Vector3 _lastInvestigateTarget;
        private float _lookAroundEndTime = -1f;

        private Animator _animator;
        private int _animSpeedHash;
        private int _animMotionSpeedHash;
        private int _animGroundedHash;
        private bool _hasSpeedParam;
        private bool _hasMotionSpeedParam;
        private bool _hasGroundedParam;
        private Vector3 _lastFramePos;

        private void Awake()
        {
            _bb = GetComponent<Blackboard>();
            if (_bb == null) _bb = gameObject.AddComponent<Blackboard>();
            if (GetComponent<Perception>() == null) gameObject.AddComponent<Perception>();
            BuildBehaviorTree();
            CacheAnimator();
        }

        private void CacheAnimator()
        {
            _animator = GetComponent<Animator>();
            if (_animator == null) return;

            _animSpeedHash = Animator.StringToHash("Speed");
            _animMotionSpeedHash = Animator.StringToHash("MotionSpeed");
            _animGroundedHash = Animator.StringToHash("Grounded");

            foreach (var p in _animator.parameters)
            {
                if (p.nameHash == _animSpeedHash) _hasSpeedParam = true;
                else if (p.nameHash == _animMotionSpeedHash) _hasMotionSpeedParam = true;
                else if (p.nameHash == _animGroundedHash) _hasGroundedParam = true;
            }

            _lastFramePos = transform.position;
        }

        private void LateUpdate()
        {
            if (_animator == null) return;

            float dt = Time.deltaTime;
            Vector3 delta = transform.position - _lastFramePos;
            delta.y = 0f;
            float speed = dt > 0.0001f ? delta.magnitude / dt : 0f;
            _lastFramePos = transform.position;

            if (_hasSpeedParam) _animator.SetFloat(_animSpeedHash, speed, SpeedDampTime, dt);
            if (_hasMotionSpeedParam) _animator.SetFloat(_animMotionSpeedHash, 1f);
            if (_hasGroundedParam) _animator.SetBool(_animGroundedHash, true);
        }

        private void Update()
        {
            if (_root == null || Grid == null) return;

            PerceptionDecisionTree.Evaluate(_bb);

            if (_bb.Mode != _previousMode)
            {
                if (_bb.CurrentPath != null) _bb.CurrentPath.Clear();
                _currentPathIndex = 0;
                _lookAroundEndTime = -1f;
                _nextRepathTime = 0f;
                _previousMode = _bb.Mode;
            }

            _root.Tick();
        }

        private void BuildBehaviorTree()
        {
            _root = new SelectorNode(
                new SequenceNode(
                    new ConditionNode(() => _bb.Mode == AIMode.Chase),
                    new ActionNode(ChasePlayer)
                ),
                new SequenceNode(
                    new ConditionNode(() => _bb.Mode == AIMode.Investigate),
                    new ActionNode(Investigate)
                ),
                new ActionNode(Patrol)
            );
        }

        private BTState ChasePlayer()
        {
            if (_bb == null) return BTState.Failure;

            _bb.TargetPosition = _bb.InterestPoint;

            if (Time.time >= _nextRepathTime || _bb.CurrentPath == null || _bb.CurrentPath.Count == 0)
            {
                _bb.CurrentPath = AStarPathfinder.FindPath(Grid, transform.position, _bb.TargetPosition);
                _currentPathIndex = 0;
                _nextRepathTime = Time.time + ChaseRepathInterval;
            }

            bool atEnd = _bb.CurrentPath == null || _bb.CurrentPath.Count == 0 || _currentPathIndex >= _bb.CurrentPath.Count;
            if (atEnd && !_bb.CanSeePlayer)
            {
                transform.Rotate(0f, LookAroundSpeed * Time.deltaTime, 0f);
                return BTState.Running;
            }

            FollowPath(_bb.CurrentPath, StoppingDistance, Time.deltaTime, ChaseSpeed, false);
            return BTState.Running;
        }

        private BTState Investigate()
        {
            if (_bb == null || Grid == null) return BTState.Failure;

            bool noPath = _bb.CurrentPath == null || _bb.CurrentPath.Count == 0;
            float threshold = InvestigateRepathThreshold * InvestigateRepathThreshold;
            bool targetMoved = (_bb.InterestPoint - _lastInvestigateTarget).sqrMagnitude > threshold;

            if (noPath || targetMoved)
            {
                _bb.TargetPosition = _bb.InterestPoint;
                _bb.CurrentPath = AStarPathfinder.FindPath(Grid, transform.position, _bb.TargetPosition);
                _currentPathIndex = 0;
                _lastInvestigateTarget = _bb.InterestPoint;
                _lookAroundEndTime = -1f;
            }

            bool atEnd = _bb.CurrentPath == null || _bb.CurrentPath.Count == 0 || _currentPathIndex >= _bb.CurrentPath.Count;
            if (atEnd)
            {
                if (_lookAroundEndTime < 0f) _lookAroundEndTime = Time.time + LookAroundDuration;
                transform.Rotate(0f, LookAroundSpeed * Time.deltaTime, 0f);
                return BTState.Running;
            }

            FollowPath(_bb.CurrentPath, StoppingDistance, Time.deltaTime, InvestigateSpeed, false);
            return BTState.Running;
        }

        private BTState Patrol()
        {
            if (_bb == null || Grid == null) return BTState.Failure;

            bool needNewTarget = _bb.CurrentPath == null || _bb.CurrentPath.Count == 0 || _currentPathIndex >= _bb.CurrentPath.Count;
            if (needNewTarget)
            {
                Node randomNode = Grid.GetRandomWalkableNode();
                if (randomNode == null) return BTState.Failure;

                _bb.TargetPosition = randomNode.WorldPosition;
                _bb.CurrentPath = AStarPathfinder.FindPath(Grid, transform.position, _bb.TargetPosition);
                _currentPathIndex = 0;
            }

            FollowPath(_bb.CurrentPath, StoppingDistance, Time.deltaTime, PatrolSpeed, true);
            return BTState.Running;
        }

        private bool FollowPath(List<Vector3> path, float acceptRadius, float deltaTime, float speed, bool clearPathAtEnd)
        {
            if (path == null || path.Count == 0) return false;
            if (_currentPathIndex >= path.Count) _currentPathIndex = 0;

            Vector3 target = path[_currentPathIndex];
            Vector3 dir = target - transform.position;
            dir.y = 0f;
            float dist = dir.magnitude;

            if (dist <= acceptRadius)
            {
                _currentPathIndex++;
                if (_currentPathIndex >= path.Count)
                {
                    if (clearPathAtEnd) path.Clear();
                    _currentPathIndex = 0;
                    return false;
                }
                return true;
            }

            Vector3 move = dir.normalized * speed * deltaTime;
            transform.position += new Vector3(move.x, 0f, move.z);

            if (dir.sqrMagnitude > 0.001f)
            {
                transform.forward = Vector3.Slerp(transform.forward, dir.normalized, 0.2f);
            }

            return true;
        }
    }

    public enum BTState
    {
        Success,
        Failure,
        Running
    }

    public abstract class BTNode
    {
        public abstract BTState Tick();
    }

    public class SelectorNode : BTNode
    {
        private readonly BTNode[] _children;

        public SelectorNode(params BTNode[] children)
        {
            _children = children;
        }

        public override BTState Tick()
        {
            foreach (BTNode child in _children)
            {
                BTState result = child.Tick();
                if (result == BTState.Success || result == BTState.Running)
                {
                    return result;
                }
            }

            return BTState.Failure;
        }
    }

    public class SequenceNode : BTNode
    {
        private readonly BTNode[] _children;

        public SequenceNode(params BTNode[] children)
        {
            _children = children;
        }

        public override BTState Tick()
        {
            foreach (BTNode child in _children)
            {
                BTState result = child.Tick();
                if (result == BTState.Failure) return BTState.Failure;
                if (result == BTState.Running) return BTState.Running;
            }

            return BTState.Success;
        }
    }

    public class ConditionNode : BTNode
    {
        private readonly System.Func<bool> _condition;

        public ConditionNode(System.Func<bool> condition)
        {
            _condition = condition;
        }

        public override BTState Tick()
        {
            return _condition() ? BTState.Success : BTState.Failure;
        }
    }

    public class ActionNode : BTNode
    {
        private readonly System.Func<BTState> _action;

        public ActionNode(System.Func<BTState> action)
        {
            _action = action;
        }

        public override BTState Tick()
        {
            return _action();
        }
    }
}
