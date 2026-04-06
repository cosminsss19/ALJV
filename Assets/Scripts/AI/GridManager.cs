using UnityEngine;
using System.Collections.Generic;

namespace AI
{
    public class GridManager : MonoBehaviour
    {
        public int Width = 10;
        public int Height = 10;
        public float CellSize = 1f;

        private Node[,] _grid;

        private void Awake()
        {
            BuildGrid();
        }

        public void BuildGrid()
        {
            _grid = new Node[Width, Height];
            Vector3 origin = transform.position - new Vector3(Width * CellSize / 2f, 0, Height * CellSize / 2f);
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Vector3 worldPos = origin + new Vector3((x + 0.5f) * CellSize, 0, (y + 0.5f) * CellSize);
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
            Vector3 local = worldPos - (transform.position - new Vector3(Width * CellSize / 2f, 0, Height * CellSize / 2f));
            int x = Mathf.FloorToInt(local.x / CellSize);
            int y = Mathf.FloorToInt(local.z / CellSize);
            x = Mathf.Clamp(x, 0, Width - 1);
            y = Mathf.Clamp(y, 0, Height - 1);
            return GetNode(x, y);
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
