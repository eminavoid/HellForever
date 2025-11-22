using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Unity.FPS.ours
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Radio alrededor del player")]
        [SerializeField] private float minRadius = 10f;
        [SerializeField] private float maxRadius = 20f;

        private Unity.FPS.Game.WavesManager m_waveManager;

        private void Awake()
        {
            m_waveManager = FindAnyObjectByType<Unity.FPS.Game.WavesManager>();
            if (m_waveManager != null)
            {
                m_waveManager.registerSpawner(this);
            }
            else
            {
                Debug.LogWarning("[EnemySpawner] No WavesManager found in scene.");
            }
        }

        private void OnDestroy()
        {
            if (m_waveManager != null)
                m_waveManager.unregisterSpawner(this);
        }

        public void SpawnEnemyOnRadius(string enemyPrefabName)
        {
            if (!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient)
            {
                Debug.Log($"[EnemySpawner] Skip spawn. InRoom={PhotonNetwork.InRoom}, IsMaster={PhotonNetwork.IsMasterClient}");
                return;
            }

            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            if (players == null || players.Length == 0)
            {
                Debug.LogWarning("[EnemySpawner] No encontré players con tag 'Player'.");
                return;
            }

            GameObject targetPlayer = players[Random.Range(0, players.Length)];
            Vector3 center = targetPlayer.transform.position;

            float r = Random.Range(minRadius, maxRadius);
            Vector2 offset2D = Random.insideUnitCircle.normalized * r;
            Vector3 spawnPos = center + new Vector3(offset2D.x, 0f, offset2D.y);

            Debug.Log($"[EnemySpawner] Spawning '{enemyPrefabName}' cerca de {targetPlayer.name} en {spawnPos}");

            GameObject go = PhotonNetwork.Instantiate(enemyPrefabName, spawnPos, Quaternion.identity);

            if (go == null)
            {
                Debug.LogError($"[EnemySpawner] NO se pudo instanciar '{enemyPrefabName}'. ¿Está en Resources y el nombre coincide?");
            }
        }

        private void OnDrawGizmosSelected()
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            if (players == null || players.Length == 0)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, minRadius);

                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, maxRadius);
                return;
            }

            Transform player = players[0].transform;
            Vector3 center = player.position;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(center, minRadius);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(center, maxRadius);
        }

    }

}
