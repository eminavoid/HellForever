using UnityEngine;
using Photon.Pun;
using Unity.FPS.Game;

public class StormManager : MonoBehaviourPun, IPunObservable
{
    public static StormManager Instance { get; private set; }

    [Header("Fase 1: Achicamiento y Escalada")]
    public float maxMapRadius = 50f;
    public float minStormRadius = 5f;
    public float shrinkDuration = 60f;       

    [Header("Fase 2: Movimiento Final")]
    public float lateGameMoveSpeed = 1.5f;

    [Header("Daño")]
    public float initialDamage = 1f;
    public float maxDamage = 15f;

    [Header("Visuals")]
    public Transform visualStormTransform;

    [Header("Estado Actual (Solo Lectura)")]
    public float currentRadius;
    public Vector3 currentCenter;
    public float currentDamage;

    private float m_NetworkRadius;
    private Vector3 m_NetworkCenter;
    private float m_NetworkDamage;

    private float m_StartTime;
    private Vector3 m_StartCenter;
    private Vector3 m_TargetShrinkCenter;

    private Vector3 m_WanderTarget;
    private bool m_IsWandering = false;

    private float m_LastDamageTime;
    private GameObject m_LocalPlayer;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        currentRadius = maxMapRadius;
        currentCenter = transform.position;
        currentDamage = initialDamage;

        if (PhotonNetwork.IsMasterClient)
        {
            m_StartTime = Time.time;
            m_StartCenter = transform.position;
            m_TargetShrinkCenter = GetRandomPointInsideMap();
        }

        m_NetworkRadius = maxMapRadius;
        m_NetworkCenter = transform.position;
        m_NetworkDamage = initialDamage;
    }

    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            float timeSinceStart = Time.time - m_StartTime;

            if (timeSinceStart < shrinkDuration)
            {
                float t = timeSinceStart / shrinkDuration;    

                currentRadius = Mathf.Lerp(maxMapRadius, minStormRadius, t);

                currentCenter = Vector3.Lerp(m_StartCenter, m_TargetShrinkCenter, t);

                currentDamage = Mathf.Lerp(initialDamage, maxDamage, t);
            }
            else
            {
                currentDamage = maxDamage;

                currentRadius = minStormRadius;

                if (!m_IsWandering)
                {
                    m_IsWandering = true;
                    m_WanderTarget = GetRandomPointInsideMap();
                }

                if (Vector3.Distance(currentCenter, m_WanderTarget) < 0.5f)
                {
                    m_WanderTarget = GetRandomPointInsideMap();
                }
                currentCenter = Vector3.MoveTowards(currentCenter, m_WanderTarget, lateGameMoveSpeed * Time.deltaTime);
            }
        }
        else
        {
            currentRadius = Mathf.Lerp(currentRadius, m_NetworkRadius, Time.deltaTime * 5f);
            currentCenter = Vector3.Lerp(currentCenter, m_NetworkCenter, Time.deltaTime * 5f);
            currentDamage = Mathf.Lerp(currentDamage, m_NetworkDamage, Time.deltaTime * 5f);
        }

        if (visualStormTransform != null)
        {
            visualStormTransform.position = currentCenter;
            float diameter = currentRadius * 2f;
            visualStormTransform.localScale = new Vector3(diameter, 50f, diameter);
        }

        CheckStormDamage();
    }

    Vector3 GetRandomPointInsideMap()
    {
        float safeMaxRadius = Mathf.Max(0, maxMapRadius - minStormRadius);
        Vector2 randomPoint = Random.insideUnitCircle * safeMaxRadius;
        return new Vector3(randomPoint.x, 0, randomPoint.y);
    }

    void CheckStormDamage()
    {
        if (m_LocalPlayer == null)
        {
            foreach (var player in FindObjectsOfType<PlayerNetworkLife>())
            {
                if (player.photonView.IsMine) { m_LocalPlayer = player.gameObject; break; }
            }
        }

        if (m_LocalPlayer != null)
        {
            float distanceToCenter = Vector3.Distance(
                new Vector3(m_LocalPlayer.transform.position.x, 0, m_LocalPlayer.transform.position.z),
                new Vector3(currentCenter.x, 0, currentCenter.z)
            );

            if (distanceToCenter > currentRadius)
            {
                if (Time.time > m_LastDamageTime + 1f)
                {
                    Health playerHealth = m_LocalPlayer.GetComponent<Health>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(currentDamage, gameObject);
                    }
                    m_LastDamageTime = Time.time;
                }
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(currentRadius);
            stream.SendNext(currentCenter);
            stream.SendNext(currentDamage);
        }
        else
        {
            m_NetworkRadius = (float)stream.ReceiveNext();
            m_NetworkCenter = (Vector3)stream.ReceiveNext();
            m_NetworkDamage = (float)stream.ReceiveNext();
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Vector3.zero, maxMapRadius);

        if (Application.isPlaying && m_IsWandering)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(currentCenter, m_WanderTarget);
            Gizmos.DrawWireSphere(m_WanderTarget, 1f);
        }

        Vector3 center = Application.isPlaying ? currentCenter : transform.position;
        float radius = Application.isPlaying ? currentRadius : maxMapRadius;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, radius);
        Gizmos.color = new Color(0, 1, 1, 0.1f);
        Gizmos.DrawSphere(center, radius);
    }
}