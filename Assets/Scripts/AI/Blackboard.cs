using UnityEngine;
using System.Collections.Generic;

namespace AI
{
    public class Blackboard : MonoBehaviour
    {
        public GameObject Player;
        public Vector3 TargetPosition;
        public List<Vector3> CurrentPath = new List<Vector3>();

        private void Awake()
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) Player = p;
        }
    }
}
