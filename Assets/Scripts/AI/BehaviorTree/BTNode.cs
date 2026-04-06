using UnityEngine;

namespace AI.BehaviorTree
{
    public enum NodeStatus { Success, Failure, Running }

    public abstract class BTNode : ScriptableObject
    {
        public abstract NodeStatus Tick(AI.Blackboard bb, float deltaTime);
    }
}
