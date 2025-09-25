using Photon.Pun;
using Unity.FPS.Game;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Unity.FPS.ours
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] float radius = 5f;
        WavesManager m_waveManager;

        void Awake() //  registrar lo antes posible
        {
            m_waveManager = FindAnyObjectByType<WavesManager>();
            if (m_waveManager != null)
                m_waveManager.registerSpawner(this);
            else
                Debug.LogWarning("[EnemySpawner] No WavesManager found in scene.");
        }

        void OnDestroy()
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

            Vector3 pos = transform.position + new Vector3(
                Random.Range(-radius, radius),
                0f,
                Random.Range(-radius, radius)
            );

            Debug.Log($"[EnemySpawner] Spawning '{enemyPrefabName}' at {pos}");
            var go = PhotonNetwork.Instantiate(enemyPrefabName, pos, Quaternion.identity);
            go.SetActive(true);
        }
    }
}
