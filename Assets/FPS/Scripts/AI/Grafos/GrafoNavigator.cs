using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.AI.Navigation
{
    // Implementación de INavigator usando un grafo y Dijkstra.
    public class GraphNavigator : INavigator
    {
        readonly Graph _graph;

        // Configuración de movimiento
        readonly float _maxSpeed;
        readonly float _rotationSpeedDegPerSec;
        readonly float _acceleration; // opcional, suavizado de velocidad
        readonly float _waypointTolerance;

        // Estado de navegación
        readonly List<Vector3> _path = new List<Vector3>();
        int _pathIndex = 0;

        // Target dinámico
        Func<Vector3> _dynamicTarget;
        float _replanInterval;
        float _replanTimer;

        // Propiedades
        public bool HasPath => _pathIndex < _path.Count;
        public IReadOnlyList<Vector3> Path => _path;
        public Vector3 Velocity { get; private set; }
        public float MaxSpeed => _maxSpeed;

        public GraphNavigator(
            Graph graph,
            float maxSpeed = 3.5f,
            float rotationSpeedDegPerSec = 540f,
            float acceleration = 100f,
            float waypointTolerance = 0.15f)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
            _maxSpeed = Mathf.Max(0.01f, maxSpeed);
            _rotationSpeedDegPerSec = Mathf.Max(1f, rotationSpeedDegPerSec);
            _acceleration = Mathf.Max(1f, acceleration);
            _waypointTolerance = Mathf.Max(0.01f, waypointTolerance);
        }

        public void SetDestination(Vector3 target)
        {
            _dynamicTarget = null; // se vuelve estático
            _replanInterval = 0f;
            RecomputePathFromToWorldPositions(_lastAgentPos, target);
        }

        public void ClearDestination()
        {
            _dynamicTarget = null;
            _replanInterval = 0f;
            _path.Clear();
            _pathIndex = 0;
            Velocity = Vector3.zero;
        }

        public void SetDynamicTarget(Func<Vector3> targetProvider, float replanningIntervalSeconds = 0f)
        {
            _dynamicTarget = targetProvider;
            _replanInterval = Mathf.Max(0f, replanningIntervalSeconds);
            _replanTimer = 0f;
        }

        Vector3 _lastAgentPos;

        public void Tick(Transform agentTransform, float deltaTime)
        {
            if (agentTransform == null) return;
            _lastAgentPos = agentTransform.position;

            // Replanificación si hay target dinámico
            if (_dynamicTarget != null)
            {
                _replanTimer -= deltaTime;
                if (_replanInterval == 0f || _replanTimer <= 0f)
                {
                    var targetPos = _dynamicTarget.Invoke();
                    RecomputePathFromToWorldPositions(agentTransform.position, targetPos);
                    _replanTimer = _replanInterval;
                }
            }

            if (!HasPath)
            {
                // Frenado suave
                if (Velocity.sqrMagnitude > 0.0001f)
                {
                    Velocity = Vector3.MoveTowards(Velocity, Vector3.zero, _acceleration * deltaTime);
                }
                return;
            }

            Vector3 currentPos = agentTransform.position;
            Vector3 waypoint = _path[_pathIndex];

            // Avanzar hacia el waypoint
            Vector3 to = waypoint - currentPos;
            Vector3 desiredVel = to.normalized * _maxSpeed;

            // Llegada al waypoint
            if (to.sqrMagnitude <= _waypointTolerance * _waypointTolerance)
            {
                _pathIndex++;
                if (!HasPath)
                {
                    Velocity = Vector3.zero;
                    return;
                }
                waypoint = _path[_pathIndex];
                to = waypoint - currentPos;
                desiredVel = to.normalized * _maxSpeed;
            }

            // Acelerar hacia la velocidad deseada
            Velocity = Vector3.MoveTowards(Velocity, desiredVel, _acceleration * deltaTime);

            // Mover el agente
            Vector3 delta = Velocity * deltaTime;
            agentTransform.position = currentPos + delta;

            // Rotar hacia la dirección de movimiento
            Vector3 flatVel = new Vector3(Velocity.x, 0f, Velocity.z);
            if (flatVel.sqrMagnitude > 1e-4f)
            {
                Quaternion targetRot = Quaternion.LookRotation(flatVel.normalized, Vector3.up);
                agentTransform.rotation = Quaternion.RotateTowards(
                    agentTransform.rotation,
                    targetRot,
                    _rotationSpeedDegPerSec * deltaTime);
            }
        }

        void RecomputePathFromToWorldPositions(Vector3 fromWorld, Vector3 toWorld)
        {
            if (_graph.NodeCount == 0)
            {
                _path.Clear();
                _pathIndex = 0;
                return;
            }

            int startIdx = _graph.GetClosestNodeIndex(fromWorld);
            int goalIdx = _graph.GetClosestNodeIndex(toWorld);

            if (startIdx < 0 || goalIdx < 0)
            {
                _path.Clear();
                _pathIndex = 0;
                return;
            }

            var nodes = Dijkstra(startIdx, goalIdx);
            _path.Clear();
            _pathIndex = 0;

            // Convertimos a posiciones del mundo
            foreach (int n in nodes)
            {
                _path.Add(_graph.GetNodePosition(n));
            }

            // Forzar el último punto al target real (suaviza el final)
            if (_path.Count > 0)
            {
                _path[_path.Count - 1] = toWorld;
            }
        }

        // Dijkstra clásico
        List<int> Dijkstra(int start, int goal)
        {
            int N = _graph.NodeCount;
            float[] dist = new float[N];
            int[] prev = new int[N];
            bool[] visited = new bool[N];

            for (int i = 0; i < N; i++)
            {
                dist[i] = float.PositiveInfinity;
                prev[i] = -1;
                visited[i] = false;
            }

            dist[start] = 0f;

            var pq = new MinPriorityQueue();
            pq.Push(start, 0f);

            while (pq.Count > 0)
            {
                var (u, _) = pq.Pop();
                if (visited[u]) continue;
                visited[u] = true;

                if (u == goal) break;

                var neighbors = _graph.GetNeighbors(u);
                for (int i = 0; i < neighbors.Count; i++)
                {
                    int v = neighbors[i].To;
                    float w = neighbors[i].Cost;

                    if (visited[v]) continue;

                    float alt = dist[u] + w;
                    if (alt < dist[v])
                    {
                        dist[v] = alt;
                        prev[v] = u;
                        pq.Push(v, alt);
                    }
                }
            }

            var path = new List<int>();
            if (prev[goal] == -1 && goal != start)
            {
                // No hay camino
                path.Add(start);
                return path;
            }

            // Reconstrucción
            int cur = goal;
            while (cur != -1)
            {
                path.Add(cur);
                cur = prev[cur];
            }
            path.Reverse();
            return path;
        }

        // Cola de prioridad mínima simple
        class MinPriorityQueue
        {
            // Binary heap
            readonly List<(int node, float prio)> _heap = new List<(int, float)>();

            public int Count => _heap.Count;

            public void Push(int node, float prio)
            {
                _heap.Add((node, prio));
                SiftUp(_heap.Count - 1);
            }

            public (int node, float prio) Pop()
            {
                var top = _heap[0];
                int last = _heap.Count - 1;
                _heap[0] = _heap[last];
                _heap.RemoveAt(last);
                if (_heap.Count > 0) SiftDown(0);
                return top;
            }

            void SiftUp(int i)
            {
                while (i > 0)
                {
                    int p = (i - 1) / 2;
                    if (_heap[p].prio <= _heap[i].prio) break;
                    (_heap[p], _heap[i]) = (_heap[i], _heap[p]);
                    i = p;
                }
            }

            void SiftDown(int i)
            {
                int n = _heap.Count;
                while (true)
                {
                    int l = 2 * i + 1;
                    int r = 2 * i + 2;
                    int smallest = i;

                    if (l < n && _heap[l].prio < _heap[smallest].prio) smallest = l;
                    if (r < n && _heap[r].prio < _heap[smallest].prio) smallest = r;
                    if (smallest == i) break;
                    (_heap[smallest], _heap[i]) = (_heap[i], _heap[smallest]);
                    i = smallest;
                }
            }
        }
    }
}