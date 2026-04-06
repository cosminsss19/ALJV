using UnityEngine;

namespace AI.BehaviorTree
{
    [CreateAssetMenu(menuName = "AI/BT/Actions/MoveToBlackboardTarget")]
    public class BTAction_MoveToBlackboardTarget : BTNode
    {
        public float AcceptRadius = 0.2f;

        public override NodeStatus Tick(global::AI.Blackboard bb, float deltaTime)
        {
            if (bb == null) return NodeStatus.Failure;
            if (bb.CurrentPath == null || bb.CurrentPath.Count == 0)
            {
                // request path
                if (bb.Player == null) return NodeStatus.Failure;
                var grid = GameObject.FindObjectOfType<AI.GridManager>();
                if (grid == null) return NodeStatus.Failure;
                bb.CurrentPath = AI.AStarPathfinder.FindPath(grid, bb.transform.position, bb.TargetPosition);
                if (bb.CurrentPath == null || bb.CurrentPath.Count == 0) return NodeStatus.Failure;
            }

            // move along path (movement handled by EnemyController component)
            var ec = bb.GetComponent<AI.EnemyController>();
            if (ec == null) return NodeStatus.Failure;
            bool moving = ec.FollowPath(bb.CurrentPath, AcceptRadius, deltaTime);
            return moving ? NodeStatus.Running : NodeStatus.Success;
        }
    }
}
