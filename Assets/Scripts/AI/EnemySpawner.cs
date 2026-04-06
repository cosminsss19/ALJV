using UnityEngine;
using System.Collections.Generic;

namespace AI
{
    public class EnemySpawner : MonoBehaviour
    {
        [System.Serializable]
        public class EnemySpawnEntry
        {
            public GameObject Prefab;
            public int Count = 1;
        }

        [Header("Grid")]
        public GridManager Grid;

        [Header("Enemies")]
        public List<EnemySpawnEntry> EnemyEntries = new List<EnemySpawnEntry>();

        [Header("Spawn Area (Grid Coordinates)")]
        public Vector2Int SpawnMin = new Vector2Int(0, 0);
        public Vector2Int SpawnMax = new Vector2Int(9, 9);
        public float SpawnHeightOffset = 1f;

        public void Spawn()
        {
            if (Grid == null)
            {
                Debug.LogWarning("EnemySpawner: Grid is missing.", this);
                return;
            }

            if (EnemyEntries == null || EnemyEntries.Count == 0)
            {
                Debug.LogWarning("EnemySpawner: EnemyEntries list is empty.", this);
                return;
            }

            List<Node> freeNodes = Grid.GetWalkableNodesInArea(SpawnMin.x, SpawnMin.y, SpawnMax.x, SpawnMax.y);
            if (freeNodes.Count == 0)
            {
                Debug.LogWarning("EnemySpawner: No walkable nodes found in spawn area.", this);
                return;
            }

            foreach (var entry in EnemyEntries)
            {
                if (entry == null || entry.Prefab == null || entry.Count <= 0) continue;

                for (int i = 0; i < entry.Count; i++)
                {
                    if (freeNodes.Count == 0)
                    {
                        Debug.LogWarning("EnemySpawner: Not enough free walkable nodes for all requested enemies.", this);
                        return;
                    }

                    int index = Random.Range(0, freeNodes.Count);
                    Node node = freeNodes[index];
                    freeNodes.RemoveAt(index);

                    var go = Instantiate(entry.Prefab, node.WorldPosition + Vector3.up * SpawnHeightOffset, Quaternion.identity);
                    var ec = go.GetComponent<EnemyController>();
                    if (ec == null) ec = go.AddComponent<EnemyController>();
                    ec.Grid = Grid;
                }
            }
        }
    }
}
