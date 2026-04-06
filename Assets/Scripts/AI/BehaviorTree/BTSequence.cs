using UnityEngine;

namespace AI.BehaviorTree
{
    public class BTSequence : BTComposite
    {
        public override NodeStatus Tick(global::AI.Blackboard bb, float deltaTime)
        {
            foreach (var child in Children)
            {
                var status = child.Tick(bb, deltaTime);
                if (status == NodeStatus.Running) return NodeStatus.Running;
                if (status == NodeStatus.Failure) return NodeStatus.Failure;
            }
            return NodeStatus.Success;
        }
    }
}
