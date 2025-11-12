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

        /// <summary>
        /// Esta función se puede llamar desde el menú contextual del componente en el Inspector.
        /// </summary>
        [ContextMenu("Buscar Vecinos en Radio")]
        public void FindNeighborsInRadius()
        {
            // Borra la lista actual para empezar de cero
            neighbors.Clear();

            // Encuentra todos los WaypointNodes en la escena
            WaypointNode[] allNodes = FindObjectsOfType<WaypointNode>();

            foreach (WaypointNode otherNode in allNodes)
            {
                // No te conectes contigo mismo
                if (otherNode == this) continue;

                // Calcula la distancia
                float distance = Vector3.Distance(transform.position, otherNode.transform.position);

                // Si está dentro del radio, añádelo como vecino
                if (distance <= detectionRadius)
                {
                    // Usamos tu función original para mantener la lógica de "undirected"
                    AddNeighbor(otherNode, true);
                }
            }

            Debug.Log($"Waypoint {gameObject.name} encontró {neighbors.Count} vecinos.");
        }


        /// <summary>
        /// Dibuja ayudas visuales en el Editor de Unity cuando el objeto está seleccionado.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            // Dibuja una esfera transparente amarilla para mostrar el radio de detección
            Gizmos.color = new Color(1f, 1f, 0f, 0.25f);
            Gizmos.DrawSphere(transform.position, detectionRadius);

            // Dibuja líneas verdes hacia los vecinos ya conectados
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