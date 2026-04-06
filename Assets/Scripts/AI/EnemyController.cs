using UnityEngine;
using System.Collections.Generic;

namespace AI
{
    [RequireComponent(typeof(Blackboard))]
    public class EnemyController : MonoBehaviour
    {
        public GridManager Grid;
        public float MoveSpeed = 2f;
        public float StoppingDistance = 0.2f;
        public float PursueRange = 8f;

        private Blackboard _bb;
        private int _currentPathIndex = 0;

        private void Awake()
        {
            _bb = GetComponent<Blackboard>();
            if (_bb == null) _bb = gameObject.AddComponent<Blackboard>();
        }

        private void Update()
        {
            bool shouldPursue = _bb.Player != null &&
                                Vector3.Distance(transform.position, _bb.Player.transform.position) < PursueRange;

            if (shouldPursue)
            {
                _bb.TargetPosition = _bb.Player.transform.position;
                _bb.CurrentPath = AStarPathfinder.FindPath(Grid, transform.position, _bb.TargetPosition);
            }
            else
            {
                if (_bb.CurrentPath == null || _bb.CurrentPath.Count == 0)
                {
                    // pick random point on grid
                    if (Grid != null)
                    {
                        int rx = Random.Range(0, Grid.Width);
                        int ry = Random.Range(0, Grid.Height);
                        var node = Grid.GetNode(rx, ry);
                        if (node != null) _bb.TargetPosition = node.WorldPosition;
                        _bb.CurrentPath = AStarPathfinder.FindPath(Grid, transform.position, _bb.TargetPosition);
                    }
                }
            }

            // follow path
            if (_bb.CurrentPath != null && _bb.CurrentPath.Count > 0)
            {
                FollowPath(_bb.CurrentPath, StoppingDistance, Time.deltaTime);
            }
        }

        // returns true if still moving
        public bool FollowPath(List<Vector3> path, float acceptRadius, float deltaTime)
        {
            if (path == null || path.Count == 0) return false;
            if (_currentPathIndex >= path.Count) _currentPathIndex = 0;

            Vector3 target = path[_currentPathIndex];
            Vector3 dir = (target - transform.position);
            dir.y = 0;
            float dist = dir.magnitude;
            if (dist <= acceptRadius)
            {
                _currentPathIndex++;
                if (_currentPathIndex >= path.Count)
                {
                    // reached final
                    path.Clear();
                    _currentPathIndex = 0;
                    return false;
                }
                return true;
            }

            Vector3 move = dir.normalized * MoveSpeed * deltaTime;
            transform.position += new Vector3(move.x, 0, move.z);
            if (dir.sqrMagnitude > 0.001f)
                transform.forward = Vector3.Slerp(transform.forward, dir.normalized, 0.2f);
            return true;
        }
    }
}
