using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Pathfinding
{
    public class WaypointNode : MonoBehaviour
    {
        public List<WaypointNode> neighbors = new List<WaypointNode>();
        public bool undirected = true;

        [Header("Detección Automática")]
        public float detectionRadius = 10f;

        public void AddNeighbor(WaypointNode other, bool reciprocal = true)
        {
            if (!other || other == this) return;
            if (!neighbors.Contains(other)) neighbors.Add(other);
            if (undirected && reciprocal) other.AddNeighbor(this, false);
        }

        [ContextMenu("Buscar Vecinos en Radio")]
        public void FindNeighborsInRadius()
        {
            neighbors.Clear();

            WaypointNode[] allNodes = FindObjectsOfType<WaypointNode>();

            foreach (WaypointNode otherNode in allNodes)
            {
                if (otherNode == this) continue;

                float distance = Vector3.Distance(transform.position, otherNode.transform.position);

                if (distance <= detectionRadius)
                {
                    AddNeighbor(otherNode, true);
                }
            }

            Debug.Log($"Waypoint {gameObject.name} encontró {neighbors.Count} vecinos.");
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.25f);
            Gizmos.DrawSphere(transform.position, detectionRadius);

            Gizmos.color = Color.green;
            foreach (WaypointNode neighbor in neighbors)
            {
                if (neighbor != null)
                {
                    Gizmos.DrawLine(transform.position, neighbor.transform.position);
                }
            }
        }
    }
}