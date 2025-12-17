using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Pathfinding
{
    public class NavGraph : MonoBehaviour
    {
        [SerializeField] List<WaypointNode> nodes = new List<WaypointNode>();
        [SerializeField] bool autoFindNodes = true;

        [Header("Obstáculos / Agente")]
        [SerializeField] LayerMask obstacleMask;
        [SerializeField] float agentRadius = 0.3f;
        [SerializeField] float agentHeight = 1.8f;
        [SerializeField] float extraClearance = 0.02f;
        [SerializeField] bool filterEdgesByObstacles = true;

        [Header("Coste")]
        [SerializeField] float edgeCostMultiplier = 1f;

        [Header("Debug")]
        [SerializeField] bool debugConnectivity = false;
        [SerializeField] bool debugPath = false;

        readonly Dictionary<WaypointNode, int> indexOf = new Dictionary<WaypointNode, int>();
        List<int>[] adj;
        Vector3[] posCache;
        bool built;

        public IReadOnlyList<WaypointNode> Nodes => nodes;

        void Awake()
        {
            if (autoFindNodes) FindAllNodes();
            Rebuild();
        }

        [ContextMenu("Find Nodes")]
        public void FindAllNodes()
        {
            nodes.Clear();
#if UNITY_2020_1_OR_NEWER
            var found = FindObjectsOfType<WaypointNode>(true);
#else
            var found = FindObjectsOfType<WaypointNode>();
#endif
            for (int i = 0; i < found.Length; i++)
                if (found[i]) nodes.Add(found[i]);
        }

        [ContextMenu("Rebuild")]
        public void Rebuild()
        {
            nodes.RemoveAll(n => n == null);

            indexOf.Clear();
            int n = nodes.Count;
            adj = new List<int>[n];
            posCache = new Vector3[n];

            for (int i = 0; i < n; i++)
            {
                var wn = nodes[i];
                indexOf[wn] = i;
                adj[i] = new List<int>(wn.neighbors != null ? wn.neighbors.Count : 0);
                posCache[i] = wn ? wn.transform.position : Vector3.zero;
            }

            for (int i = 0; i < n; i++)
            {
                var from = nodes[i];
                if (!from || from.neighbors == null) continue;
                for (int k = 0; k < from.neighbors.Count; k++)
                {
                    var nb = from.neighbors[k];
                    if (!nb) continue;
                    if (!indexOf.TryGetValue(nb, out int j)) continue;
                    if (i == j) continue;
                    if (!filterEdgesByObstacles || HasLineOfSight(posCache[i], posCache[j]))
                        adj[i].Add(j);
                }
            }

            built = true;
            if (debugConnectivity) DumpConnectivity();
        }

        public void DumpConnectivity()
        {
            if (!built)
            {
                Debug.LogWarning("[NavGraph] Conectividad solicitada sin grafo construido.");
                return;
            }
            Debug.Log($"[NavGraph] Nodos={nodes.Count}");
            for (int i = 0; i < adj.Length; i++)
            {
                string line = $"Nodo {i} {nodes[i].name} -> ";
                if (adj[i].Count == 0) line += "(sin vecinos)";
                else
                {
                    for (int k = 0; k < adj[i].Count; k++)
                        line += $"{adj[i][k]} ";
                }
                Debug.Log(line);
            }
        }

        public bool HasLineOfSight(Vector3 a, Vector3 b)
        {
            Vector3 d = b - a;
            float dist = d.magnitude;
            if (dist <= 0.0001f) return true;
            d /= dist;

            float r = agentRadius + Mathf.Max(0f, extraClearance);
            float bottom = r;
            float top = Mathf.Max(r, agentHeight - r);
            Vector3 p0 = a + Vector3.up * bottom;
            Vector3 p1 = a + Vector3.up * top;

            return !Physics.CapsuleCast(p0, p1, r, d, dist, obstacleMask, QueryTriggerInteraction.Ignore);
        }

        public int FindClosestNodeIndex(Vector3 p, float maxDist, bool requireLoS)
        {
            if (!built || nodes.Count == 0) return -1;
            float best = maxDist * maxDist;
            int bestIdx = -1;
            for (int i = 0; i < posCache.Length; i++)
            {
                float sq = (posCache[i] - p).sqrMagnitude;
                if (sq < best)
                {
                    if (!requireLoS || HasLineOfSight(p, posCache[i]))
                    {
                        best = sq;
                        bestIdx = i;
                    }
                }
            }
            return bestIdx;
        }

        float Cost(int a, int b) => Vector3.Distance(posCache[a], posCache[b]) * edgeCostMultiplier;

        List<int> DijkstraIdx(int s, int g)
        {
            var path = new List<int>();
            if (!built || s < 0 || g < 0 || s >= nodes.Count || g >= nodes.Count) return path;

            int n = nodes.Count;
            float[] dist = new float[n];
            int[] came = new int[n];
            bool[] vis = new bool[n];
            for (int i = 0; i < n; i++) { dist[i] = float.PositiveInfinity; came[i] = -1; vis[i] = false; }

            var heap = new MinHeapInt();
            dist[s] = 0f; heap.Insert(s, 0f);

            while (heap.Count > 0)
            {
                int u = heap.ExtractMin(out float du);
                if (u == -1) break;
                if (vis[u]) continue;
                vis[u] = true;
                if (u == g) break;

                var nb = adj[u];
                for (int k = 0; k < nb.Count; k++)
                {
                    int v = nb[k];
                    if (vis[v]) continue;
                    float alt = du + Cost(u, v);
                    if (alt < dist[v])
                    {
                        dist[v] = alt;
                        came[v] = u;
                        if (!heap.TryUpdate(v, alt)) heap.Insert(v, alt);
                    }
                }
            }

            if (came[g] == -1 && s != g) return path;
            for (int u = g; u != -1; u = came[u]) path.Add(u);
            path.Reverse();
            return path;
        }


        public List<Vector3> FindPathPositionsStrict(
            Vector3 from,
            Vector3 to,
            float maxSnap,
            bool requireLoSForSnap,
            bool enforceNodeEntry,
            bool smooth,
            bool log)
        {
            var pts = new List<Vector3>();

            int s = FindClosestNodeIndex(from, maxSnap, requireLoSForSnap);
            int g = FindClosestNodeIndex(to, maxSnap, requireLoSForSnap);

            if (log)
                Debug.Log($"[NavGraph] Snap s={s} g={g} maxSnap={maxSnap} requireLoS={requireLoSForSnap}");

            if (s == -1 || g == -1)
            {
                int s2 = FindClosestNodeIndex(from, maxSnap, false);
                int g2 = FindClosestNodeIndex(to, maxSnap, false);
                if (log)
                    Debug.LogWarning($"[NavGraph] Snap fallido con LoS. Retrying sin LoS: s2={s2} g2={g2}");
                s = s2; g = g2;
                if (s == -1 || g == -1) return pts;
            }

            var idxRoute = DijkstraIdx(s, g);
            if (idxRoute.Count == 0)
            {
                if (log)
                    Debug.LogWarning("[NavGraph] Dijkstra sin ruta (grafo desconectado o filtro excesivo).");
                return pts;
            }


            if ((enforceNodeEntry || s == g) && Vector3.Distance(from, posCache[s]) > 0.25f)
                pts.Add(posCache[s]);

            for (int i = 0; i < idxRoute.Count; i++)
            {
                Vector3 p = posCache[idxRoute[i]];
                if (pts.Count == 0 || (pts[pts.Count - 1] - p).sqrMagnitude > 0.0001f)
                    pts.Add(p);
            }

            if ((pts[pts.Count - 1] - to).sqrMagnitude > 0.0001f)
                pts.Add(to);

            if (smooth)
                pts = SmoothByLoS(pts);

            if (log)
            {
                string dump = "[NavGraph] Ruta final:";
                for (int i = 0; i < pts.Count; i++)
                    dump += $"\n  {i}: {pts[i]}";
                Debug.Log(dump);
            }

            return pts;
        }

        List<Vector3> SmoothByLoS(List<Vector3> pts)
        {
            if (pts == null || pts.Count <= 2) return pts;
            var outPts = new List<Vector3>();
            int i = 0;
            outPts.Add(pts[i]);
            while (i < pts.Count - 1)
            {
                int furthest = i + 1;
                for (int j = i + 1; j < pts.Count; j++)
                {
                    if (HasLineOfSight(pts[i], pts[j])) furthest = j;
                    else break;
                }
                outPts.Add(pts[furthest]);
                i = furthest;
            }
            return outPts;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (!Application.isPlaying && nodes != null && nodes.Count > 0) Rebuild();
        }
#endif
    }
}