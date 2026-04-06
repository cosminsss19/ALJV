using UnityEngine;
using System.Collections.Generic;

namespace AI
{
    [RequireComponent(typeof(Blackboard))]
    public class EnemyController : MonoBehaviour
    {
        public GridManager Grid;

        [Header("Movement")]
        public float PatrolSpeed = 1.4f;
        public float ChaseSpeed = 3.2f;
        public float StoppingDistance = 0.2f;

        [Header("Detection")]
        public float PursueRange = 10f;

        [Header("Pathfinding")]
        public float ChaseRepathInterval = 0.25f;

        private Blackboard _bb;
        private int _currentPathIndex = 0;
        private float _nextRepathTime = 0f;
        private BTNode _root;

        private void Awake()
        {
            _bb = GetComponent<Blackboard>();
            if (_bb == null) _bb = gameObject.AddComponent<Blackboard>();
            BuildBehaviorTree();
        }

        private void Update()
        {
            if (_root == null || Grid == null) return;
            _root.Tick();
        }

        private void BuildBehaviorTree()
        {
            _root = new SelectorNode(
                new SequenceNode(
                    new ConditionNode(IsPlayerInRange),
                    new ActionNode(ChasePlayer)
                ),
                new ActionNode(Patrol)
            );
        }

        private bool IsPlayerInRange()
        {
            if (_bb == null || _bb.Player == null) return false;
            return Vector3.Distance(transform.position, _bb.Player.transform.position) <= PursueRange;
        }

        private BTState ChasePlayer()
        {
            if (_bb == null || _bb.Player == null) return BTState.Failure;

            _bb.TargetPosition = _bb.Player.transform.position;

            if (Time.time >= _nextRepathTime || _bb.CurrentPath == null || _bb.CurrentPath.Count == 0)
            {
                _bb.CurrentPath = AStarPathfinder.FindPath(Grid, transform.position, _bb.TargetPosition);
                _currentPathIndex = 0;
                _nextRepathTime = Time.time + ChaseRepathInterval;
            }

            FollowPath(_bb.CurrentPath, StoppingDistance, Time.deltaTime, ChaseSpeed, false);
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
