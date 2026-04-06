using UnityEngine;

namespace AI.DecisionTree
{
    public enum DecisionResult { Idle, Patrol, Pursue }

    public static class DecisionTree
    {
        public static DecisionResult Decide(global::AI.Blackboard bb)
        {
            if (bb == null || bb.Player == null) return DecisionResult.Patrol;
            float dist = Vector3.Distance(bb.transform.position, bb.Player.transform.position);
            if (dist < 8f) return DecisionResult.Pursue;
            return DecisionResult.Patrol;
        }
    }
}
