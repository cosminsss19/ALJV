using UnityEngine;
using System.Collections.Generic;

namespace AI.BehaviorTree
{
    public abstract class BTComposite : BTNode
    {
        public List<BTNode> Children = new List<BTNode>();
    }
}
