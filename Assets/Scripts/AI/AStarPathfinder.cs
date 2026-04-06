using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace AI
{
    public static class AStarPathfinder
    {
        public static List<Vector3> FindPath(GridManager grid, Vector3 startWorld, Vector3 targetWorld)
        {
            if (grid == null) return new List<Vector3>();
            Node startNode = grid.GetNodeFromWorldPosition(startWorld);
            Node targetNode = grid.GetNodeFromWorldPosition(targetWorld);
            if (startNode == null || targetNode == null) return new List<Vector3>();

            var openSet = new List<Node> { startNode };
            var closedSet = new HashSet<Node>();

            startNode.G = 0;
            startNode.H = Heuristic(startNode, targetNode);
            startNode.Parent = null;

            while (openSet.Count > 0)
            {
                Node current = openSet.OrderBy(n => n.F).First();
                if (current == targetNode) return RetracePath(startNode, targetNode);

                openSet.Remove(current);
                closedSet.Add(current);

                foreach (var neighbor in grid.GetNeighbors(current))
                {
                    if (closedSet.Contains(neighbor)) continue;
                    int tentativeG = current.G + 1;
                    if (!openSet.Contains(neighbor) || tentativeG < neighbor.G)
                    {
                        neighbor.G = tentativeG;
                        neighbor.H = Heuristic(neighbor, targetNode);
                        neighbor.Parent = current;
                        if (!openSet.Contains(neighbor)) openSet.Add(neighbor);
                    }
                }
            }

            return new List<Vector3>();
        }

        private static int Heuristic(Node a, Node b)
        {
            return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
        }

        private static List<Vector3> RetracePath(Node start, Node end)
        {
            var path = new List<Vector3>();
            Node current = end;
            while (current != null && current != start)
            {
                path.Add(current.WorldPosition);
                current = current.Parent;
            }
            path.Reverse();
            return path;
        }
    }
}
