using UnityEngine;
using Project.Pathfinding;

namespace GameAI.DebugTools
{
    [ExecuteAlways]
    public class GraphGizmos : MonoBehaviour
    {
        public NavGraph graph;
        public Color nodeColor = Color.yellow;
        public Color edgeColor = Color.cyan;
        public float nodeRadius = 0.2f;

        void OnDrawGizmos()
        {
            if (!graph || graph.Nodes == null) return;

            Gizmos.color = nodeColor;
            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                var n = graph.Nodes[i];
                if (n) Gizmos.DrawSphere(n.transform.position, nodeRadius);
            }

            Gizmos.color = edgeColor;
            for (int i = 0; i < graph.Nodes.Count; i++)
            {
                var a = graph.Nodes[i];
                if (!a || a.neighbors == null) continue;
                for (int k = 0; k < a.neighbors.Count; k++)
                {
                    var b = a.neighbors[k];
                    if (!b) continue;
                    if (a.GetInstanceID() < b.GetInstanceID())
                        Gizmos.DrawLine(a.transform.position, b.transform.position);
                }
            }
        }
    }
}