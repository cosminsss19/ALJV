using UnityEngine;
using System.Collections.Generic;

namespace AI
{
    public enum AIMode
    {
        Patrol,
        Investigate,
        Chase
    }

    public class Blackboard : MonoBehaviour
    {
        public GameObject Player;
        public Vector3 TargetPosition;
        public List<Vector3> CurrentPath = new List<Vector3>();

        [Header("Decision State")]
        public AIMode Mode = AIMode.Patrol;
        public Vector3 InterestPoint;

        [Header("Perception State")]
        public bool CanSeePlayer;
        public float LastSeenTime = float.NegativeInfinity;
        public Vector3 LastKnownPlayerPosition;
        public float LastHeardTime = float.NegativeInfinity;
        public Vector3 LastHeardPosition;

        [Header("Memory")]
        public float MemoryDuration = 5f;

        private void Awake()
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) Player = p;
        }
    }
}
