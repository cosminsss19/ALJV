using UnityEngine;

namespace AI
{
    public class Node
    {
        public int X;
        public int Y;
        public Vector3 WorldPosition;
        public bool Walkable = true;

        public int G;
        public int H;
        public Node Parent;

        public int F { get { return G + H; } }

        public Node(int x, int y, Vector3 worldPos, bool walkable = true)
        {
            X = x; Y = y; WorldPosition = worldPos; Walkable = walkable;
        }
    }
}
