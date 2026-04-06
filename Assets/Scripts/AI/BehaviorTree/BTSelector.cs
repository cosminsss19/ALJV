using UnityEngine;

namespace AI.BehaviorTree
{
    public class BTSelector : BTComposite
    {
        public override NodeStatus Tick(global::AI.Blackboard bb, float deltaTime)
        {
            foreach (var child in Children)
            {
                var status = child.Tick(bb, deltaTime);
                if (status == NodeStatus.Running) return NodeStatus.Running;
                if (status == NodeStatus.Success) return NodeStatus.Success;
            }
            return NodeStatus.Failure;
        }
    }
}
