using UnityEngine;
using System.Collections.Generic;

namespace AI
{
    public class GridManager : MonoBehaviour
    {
        [Header("Grid")]
        public int Width = 10;
        public int Height = 10;
        public float CellSize = 1f;

        [Header("Obstacles")]
        public List<GameObject> ObstaclePrefabs = new List<GameObject>();
        public int ObstacleCount = 10;
        public float ObstacleSpawnY = 0f;

        [Header("Enemies")]
        public List<GameObject> EnemyPrefabs = new List<GameObject>();
        public int EnemyCount = 5;
        public float EnemySpawnY = 0f;

        [Header("Runtime")]
        public bool AutoSpawnOnPlay = true;

        private Node[,] _grid;
        private Vector3 _origin;

        private void Awake()
        {
            BuildGrid();

            if (AutoSpawnOnPlay)
            {
                SpawnObstaclesAndEnemies();
            }
        }

        public void BuildGrid()
        {
            int gridWidth = Mathf.Max(1, Width);
            int gridHeight = Mathf.Max(1, Height);
            float cellSize = Mathf.Max(0.01f, CellSize);

            Width = gridWidth;
            Height = gridHeight;
            CellSize = cellSize;

            _grid = new Node[gridWidth, gridHeight];
            _origin = transform.position - new Vector3(gridWidth * cellSize / 2f, 0, gridHeight * cellSize / 2f);

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Vector3 worldPos = _origin + new Vector3((x + 0.5f) * cellSize, 0, (y + 0.5f) * cellSize);
                    _grid[x, y] = new Node(x, y, worldPos, true);
                }
            }

        }

        public Node GetNode(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return null;
            return _grid[x, y];
        }

        public Node GetNodeFromWorldPosition(Vector3 worldPos)
        {
            if (_grid == null) return null;
            Vector3 local = worldPos - _origin;
            int x = Mathf.FloorToInt(local.x / CellSize);
            int y = Mathf.FloorToInt(local.z / CellSize);
            x = Mathf.Clamp(x, 0, Width - 1);
            y = Mathf.Clamp(y, 0, Height - 1);
            return GetNode(x, y);
        }

        public void SpawnObstaclesAndEnemies()
        {
            if (_grid == null) BuildGrid();

            var freeNodes = GetWalkableNodesInArea(0, 0, Width - 1, Height - 1);
            RemoveNodesOccupiedByExistingEnemies(freeNodes);

            SpawnObstacles(freeNodes);
            SpawnEnemies(freeNodes);
        }

        private void SpawnObstacles(List<Node> freeNodes)
        {
            int count = Mathf.Max(0, ObstacleCount);
            for (int i = 0; i < count; i++)
            {
                if (freeNodes.Count == 0) return;
                var prefab = GetRandomPrefab(ObstaclePrefabs);
                if (prefab == null) return;

                int index = Random.Range(0, freeNodes.Count);
                var node = freeNodes[index];
                freeNodes.RemoveAt(index);

                var go = Instantiate(prefab, node.WorldPosition + Vector3.up * ObstacleSpawnY, Quaternion.identity);
                node.Walkable = false;
            }
        }

        private void SpawnEnemies(List<Node> freeNodes)
        {
            int count = Mathf.Max(0, EnemyCount);
            for (int i = 0; i < count; i++)
            {
                if (freeNodes.Count == 0) return;
                var prefab = GetRandomPrefab(EnemyPrefabs);
                if (prefab == null) return;

                int index = Random.Range(0, freeNodes.Count);
                var node = freeNodes[index];
                freeNodes.RemoveAt(index);

                var go = Instantiate(prefab, node.WorldPosition + Vector3.up * EnemySpawnY, Quaternion.identity);

                var ec = go.GetComponent<EnemyController>();
                if (ec == null) ec = go.AddComponent<EnemyController>();
                ec.Grid = this;
            }
        }

        private GameObject GetRandomPrefab(List<GameObject> prefabs)
        {
            if (prefabs == null || prefabs.Count == 0) return null;

            var valid = new List<GameObject>();
            foreach (var prefab in prefabs)
            {
                if (prefab != null) valid.Add(prefab);
            }

            if (valid.Count == 0) return null;
            return valid[Random.Range(0, valid.Count)];
        }

        private void RemoveNodesOccupiedByExistingEnemies(List<Node> freeNodes)
        {
            if (freeNodes == null || freeNodes.Count == 0) return;

            var occupied = new HashSet<Node>();
            var existingEnemies = FindObjectsOfType<EnemyController>();
            foreach (var enemy in existingEnemies)
            {
                if (enemy == null) continue;
                var node = GetNodeFromWorldPosition(enemy.transform.position);
                if (node != null)
                {
                    occupied.Add(node);
                }
            }

            if (occupied.Count == 0) return;
            freeNodes.RemoveAll(n => occupied.Contains(n));
        }

        public IEnumerable<Node> GetNeighbors(Node node)
        {
            int x = node.X;
            int y = node.Y;
            Node n;
            n = GetNode(x - 1, y); if (n != null && n.Walkable) yield return n;
            n = GetNode(x + 1, y); if (n != null && n.Walkable) yield return n;
            n = GetNode(x, y - 1); if (n != null && n.Walkable) yield return n;
            n = GetNode(x, y + 1); if (n != null && n.Walkable) yield return n;
        }

        public List<Node> GetWalkableNodesInArea(int minX, int minY, int maxX, int maxY)
        {
            var result = new List<Node>();
            if (_grid == null) BuildGrid();

            int fromX = Mathf.Clamp(Mathf.Min(minX, maxX), 0, Width - 1);
            int toX = Mathf.Clamp(Mathf.Max(minX, maxX), 0, Width - 1);
            int fromY = Mathf.Clamp(Mathf.Min(minY, maxY), 0, Height - 1);
            int toY = Mathf.Clamp(Mathf.Max(minY, maxY), 0, Height - 1);

            for (int x = fromX; x <= toX; x++)
            {
                for (int y = fromY; y <= toY; y++)
                {
                    var node = GetNode(x, y);
                    if (node != null && node.Walkable)
                    {
                        result.Add(node);
                    }
                }
            }

            return result;
        }

        private void OnDrawGizmosSelected()
        {
            if (_grid == null) return;
            foreach (var node in _grid)
            {
                Gizmos.color = node.Walkable ? Color.green : Color.red;
                Gizmos.DrawWireCube(node.WorldPosition, Vector3.one * (CellSize * 0.9f));
            }
        }
    }
}
