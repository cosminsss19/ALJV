using UnityEngine;

namespace AI
{
    public static class PerceptionDecisionTree
    {
        public static void Evaluate(Blackboard bb)
        {
            if (bb == null) return;

            float now = Time.time;
            bool sees = bb.CanSeePlayer && bb.Player != null;
            bool remembersSight = (now - bb.LastSeenTime) <= bb.MemoryDuration;
            bool remembersHearing = (now - bb.LastHeardTime) <= bb.MemoryDuration;

            if (sees)
            {
                bb.Mode = AIMode.Chase;
                bb.InterestPoint = bb.Player.transform.position;
            }
            else if (remembersSight)
            {
                bb.Mode = AIMode.Chase;
                bb.InterestPoint = bb.LastKnownPlayerPosition;
            }
            else if (remembersHearing)
            {
                bb.Mode = AIMode.Investigate;
                bb.InterestPoint = bb.LastHeardPosition;
            }
            else
            {
                bb.Mode = AIMode.Patrol;
            }
        }
    }
}
