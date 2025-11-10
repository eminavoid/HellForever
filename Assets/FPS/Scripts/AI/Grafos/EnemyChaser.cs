using System.Collections.Generic;
using UnityEngine;
using Project.Pathfinding;
using Unity.FPS.AI;

namespace Project.AI
{
    [RequireComponent(typeof(EnemyController))]
    public class EnemyChaser : MonoBehaviour
    {
        [SerializeField] NavGraph graph;
        [SerializeField] Transform player;

        [Header("Movimiento")]
        [SerializeField] float speed = 3.5f;
        [SerializeField] float rotationSpeed = 720f;
        [SerializeField] float arriveThreshold = 0.2f;

        [Header("Pathfinding")]
        [SerializeField] float repathInterval = 0.4f;
        [SerializeField] float repathIfTargetMoved = 0.75f;
        [SerializeField] float snapMaxDistance = 120f;
        [SerializeField] bool requireLoSForSnap = false;
        [SerializeField] bool enforceNodeEntry = true;
        [SerializeField] bool smoothPath = false;

        [Header("Antioscilación")]
        [SerializeField] float skipWaypointRadius = 0.5f;
        [SerializeField] float forwardBiasDot = -0.1f;

        [Header("Debug")]
        [SerializeField] bool debugLogs = false;
        [SerializeField] bool drawPath = true;
        [SerializeField] Color pathColor = Color.green;

        List<Vector3> route;
        int index;
        float lastRepath;
        Vector3 lastTargetPos;

        CharacterController cc;
        Rigidbody rb;

        void Awake()
        {
            cc = GetComponent<CharacterController>();
            rb = GetComponent<Rigidbody>();
        }

        void Start()
        {
            if (!graph) graph = FindObjectOfType<NavGraph>();
            ForceRepath();
        }

        void Update()
        {
            if (!graph || !player) return;

            bool timeUp = Time.time - lastRepath >= repathInterval;
            bool moved = (player.position - lastTargetPos).sqrMagnitude >= repathIfTargetMoved * repathIfTargetMoved;

            if (timeUp || moved) RepathPreservandoProgreso();
            Follow();
        }


        void RepathPreservandoProgreso()
        {
            if (!graph || !player) return;

            var nueva = graph.FindPathPositionsStrict(
                transform.position,
                player.position,
                snapMaxDistance,
                requireLoSForSnap,
                enforceNodeEntry,
                smoothPath,
                debugLogs
            );


            if (nueva == null || nueva.Count == 0)
            {
                if (debugLogs) Debug.Log("[EnemyChaser] Ruta vacía al replanear, mantengo ruta anterior.");
                lastRepath = Time.time;
                lastTargetPos = player.position;
                return;
            }


            int nuevoIndex = ComputeStartIndex(nueva, transform.position);

            route = nueva;
            index = nuevoIndex;
            lastRepath = Time.time;
            lastTargetPos = player.position;

            if (debugLogs)
            {
                string log = $"[EnemyChaser] Nueva ruta (idx={index}):";
                for (int i = 0; i < route.Count; i++) log += $"\n  {i}: {route[i]}";
                Debug.Log(log);
            }
        }

        int ComputeStartIndex(List<Vector3> pts, Vector3 pos)
        {
            if (pts == null || pts.Count == 0) return 0;


            int i = 0;
            float skipSqr = skipWaypointRadius * skipWaypointRadius;
            for (; i < pts.Count; i++)
            {
                if ((pts[i] - pos).sqrMagnitude > skipSqr) break;
            }
            if (i >= pts.Count) i = pts.Count - 1;


            Vector3 dir = pts[i] - pos; dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
            {
                float dot = Vector3.Dot(dir.normalized, transform.forward);
                if (dot < forwardBiasDot && i + 1 < pts.Count) i++;
            }


            while (i < pts.Count && (pts[i] - pos).sqrMagnitude <= arriveThreshold * arriveThreshold)
                i++;

            if (i >= pts.Count) i = pts.Count - 1;
            return i;
        }

        public void ForceRepath()
        {
            lastRepath = -999f;
            lastTargetPos = player ? player.position + Vector3.up * 999f : Vector3.zero;
            RepathPreservandoProgreso();
        }

        void Follow()
        {
            if (route == null || index >= route.Count) return;

            Vector3 pos = transform.position;

            while (index < route.Count && (route[index] - pos).sqrMagnitude <= arriveThreshold * arriveThreshold)
                index++;

            if (index >= route.Count) return;

            Vector3 tgt = route[index];
            Vector3 dir = tgt - pos; dir.y = 0f;
            float dist = dir.magnitude;

            if (dist > 0.0001f)
            {
                Vector3 vel = dir.normalized * speed;
                if (cc && cc.enabled)
                    cc.SimpleMove(vel);
                else if (rb && !rb.isKinematic)
                    rb.MovePosition(pos + vel * Time.deltaTime);
                else
                    transform.position = pos + vel * Time.deltaTime;

                Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, look, rotationSpeed * Time.deltaTime);
            }
        }



        void OnDrawGizmosSelected()
        {
            if (!drawPath || route == null || route.Count == 0) return;
            Gizmos.color = pathColor;
            Vector3 prev = transform.position;
            for (int i = 0; i < route.Count; i++)
            {
                Gizmos.DrawLine(prev, route[i]);
                Gizmos.DrawSphere(route[i], 0.08f);
                prev = route[i];
            }
        }
    }
}