using UnityEngine;
using System.Collections.Generic;

namespace AI
{
    public class EnemyCommunicationManager : MonoBehaviour
    {
        private List<EnemyController> _enemies = new List<EnemyController>();

        public static EnemyCommunicationManager Instance;

        private void Awake()
        {
            Instance = this;
        }

        public void RegisterEnemy(EnemyController e)
        {
            if (e == null) return;
            if (!_enemies.Contains(e)) _enemies.Add(e);
        }

        public void Alert(Vector3 position, float radius = 5f)
        {
            foreach (var e in _enemies)
            {
                if (e == null) continue;
                if (Vector3.Distance(e.transform.position, position) <= radius)
                {
                    var bb = e.GetComponent<global::AI.Blackboard>();
                    if (bb != null)
                    {
                        bb.Alerted = true;
                        bb.LastAlertTime = Time.time;
                        bb.TargetPosition = position;
                        bb.CurrentPath = AStarPathfinder.FindPath(e.Grid, e.transform.position, position);
                    }
                }
            }
        }
    }
}
