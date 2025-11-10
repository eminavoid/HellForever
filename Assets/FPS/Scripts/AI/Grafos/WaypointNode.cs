using System.Collections.Generic;
using UnityEngine;

namespace Project.Pathfinding
{
    public class WaypointNode : MonoBehaviour
    {
        public List<WaypointNode> neighbors = new List<WaypointNode>();
        public bool undirected = true;

        public void AddNeighbor(WaypointNode other, bool reciprocal = true)
        {
            if (!other || other == this) return;
            if (!neighbors.Contains(other)) neighbors.Add(other);
            if (undirected && reciprocal) other.AddNeighbor(this, false);
        }
    }
}