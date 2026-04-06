using UnityEngine;

namespace AI
{
    public class EnemySpawner : MonoBehaviour
    {
        public GameObject EnemyPrefab;
        public int Count = 5;
        public GridManager Grid;

        public void Spawn()
        {
            if (EnemyPrefab == null || Grid == null) return;
            for (int i = 0; i < Count; i++)
            {
                int rx = Random.Range(0, Grid.Width);
                int ry = Random.Range(0, Grid.Height);
                var node = Grid.GetNode(rx, ry);
                if (node == null) continue;
                var go = Instantiate(EnemyPrefab, node.WorldPosition + Vector3.up, Quaternion.identity);
                var ec = go.GetComponent<EnemyController>();
                if (ec == null) ec = go.AddComponent<EnemyController>();
                ec.Grid = Grid;
                var comm = FindObjectOfType<EnemyCommunicationManager>();
                if (comm != null) comm.RegisterEnemy(ec);
            }
        }
    }
}
