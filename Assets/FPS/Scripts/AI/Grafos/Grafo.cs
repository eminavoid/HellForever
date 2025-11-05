using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.AI.Navigation
{
    // TDA de Grafo no dirigido con pesos (costos)
    public class Graph
    {
        public struct Edge
        {
            public int To;
            public float Cost;

            public Edge(int to, float cost)
            {
                To = to;
                Cost = cost;
            }
        }

        private readonly List<Vector3> _nodePositions = new List<Vector3>();
        private readonly List<List<Edge>> _adj = new List<List<Edge>>();

        public int NodeCount => _nodePositions.Count;

        public int AddNode(Vector3 position)
        {
            _nodePositions.Add(position);
            _adj.Add(new List<Edge>());
            return _nodePositions.Count - 1;
        }

        // Agrega arista no dirigida; si cost < 0 usa distancia euclídea.
        public void AddUndirectedEdge(int a, int b, float cost = -1f)
        {
            if (a < 0 || b < 0 || a >= NodeCount || b >= NodeCount || a == b)
                throw new ArgumentException("Índices de nodo inválidos para arista.");

            if (cost < 0f)
                cost = Vector3.Distance(_nodePositions[a], _nodePositions[b]);

            _adj[a].Add(new Edge(b, cost));
            _adj[b].Add(new Edge(a, cost));
        }

        public IReadOnlyList<Edge> GetNeighbors(int nodeIndex) => _adj[nodeIndex];

        public Vector3 GetNodePosition(int nodeIndex) => _nodePositions[nodeIndex];

        // Retorna índice del nodo más cercano a 'pos'.
        public int GetClosestNodeIndex(Vector3 pos)
        {
            if (NodeCount == 0) return -1;

            int best = 0;
            float bestDistSq = (pos - _nodePositions[0]).sqrMagnitude;

            for (int i = 1; i < _nodePositions.Count; i++)
            {
                float d = (pos - _nodePositions[i]).sqrMagnitude;
                if (d < bestDistSq)
                {
                    bestDistSq = d;
                    best = i;
                }
            }

            return best;
        }
    }
}