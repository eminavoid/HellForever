using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.ours;

namespace Unity.FPS.Game
{
    public class WavesManager : MonoBehaviourPunCallbacks
    {
        [Header("Waves")]
        [SerializeField] private int wavesCount = 5;
        [SerializeField] private int minEnemyCount = 1;
        [SerializeField] private int maxEnemyCount = 10;

        public int WavesCount
        {
            get { return wavesCount; }
        }

        [Header("Tiempo entre enemigos")]
        [SerializeField] private float spawnDelayBetweenEnemies = 0.3f;

        [Header("Enemigos que pueden spawnear (PREFABS DE RESOURCES)")]
        [SerializeField] private List<GameObject> spawnObjects;

        private readonly List<EnemySpawner> spawners = new();
        private QueueTDA<List<GameObject>> spawnQueue;

        bool isLaunchingWave = false;

        public void registerSpawner(EnemySpawner s)
        {
            if (!spawners.Contains(s))
            {
                spawners.Add(s);
                Debug.Log($"[WavesManager] Spawner registrado. Total={spawners.Count}");
            }
        }

        public void unregisterSpawner(EnemySpawner s)
        {
            if (spawners.Contains(s))
            {
                spawners.Remove(s);
                Debug.Log($"[WavesManager] Spawner removido. Total={spawners.Count}");
            }
        }

        private void Awake()
        {
            spawnQueue = new QueueTDA<List<GameObject>>();
            spawnQueue.InicializarCola(wavesCount);
            CreateWaves();
        }

        private void Start()
        {
            Debug.Log($"[WavesManager] Start. InRoom={PhotonNetwork.InRoom}, IsMaster={PhotonNetwork.IsMasterClient}");

            if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
            {
                TryLaunchWave();
            }
        }

        private void CreateWaves()
        {
            if (!spawnQueue.ColaVacia())
                return;

            for (int i = 0; i < wavesCount; i++)
            {
                int count = Random.Range(minEnemyCount, maxEnemyCount + 1);
                var list = new List<GameObject>(count);

                for (int j = 0; j < count; j++)
                {
                    if (spawnObjects == null || spawnObjects.Count == 0)
                    {
                        Debug.LogError("[WavesManager] spawnObjects vacío, no puedo crear waves.");
                        return;
                    }

                    list.Add(spawnObjects[Random.Range(0, spawnObjects.Count)]);
                }

                spawnQueue.Acolar(list);
            }

            Debug.Log($"[WavesManager] Creación de waves completa. Waves={wavesCount}");
        }

        //PHOTON CALLBACKS
        public override void OnJoinedRoom()
        {
            Debug.Log("[WavesManager] OnJoinedRoom disparado.");
            TryLaunchWave();
        }

        public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log("[WavesManager] Soy nuevo MasterClient, lanzo wave.");
                TryLaunchWave();
            }
        }

        //EJECUCIÓN DE WAVES
        private void TryLaunchWave()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.Log("[WavesManager] No soy MasterClient, no lanzo waves.");
                return;
            }

            if (isLaunchingWave)
                return;

            StartCoroutine(TryLaunchWaveCoroutine());
        }

        private IEnumerator TryLaunchWaveCoroutine()
        {
            isLaunchingWave = true;

            int tries = 0;

            // wsperar a que los spawners queden registrados
            while (spawners.Count == 0 && tries < 40)
            {
                Debug.LogWarning("[WavesManager] No hay spawners aún, espero...");
                tries++;
                yield return new WaitForSeconds(0.25f);
            }

            if (spawners.Count == 0)
            {
                Debug.LogError("[WavesManager] No se encontraron spawners después de esperar.");
                isLaunchingWave = false;
                yield break;
            }

            WaveExecute();
            isLaunchingWave = false;
        }

        public void WaveExecute()
        {
            if (spawnQueue.ColaVacia())
            {
                Debug.LogWarning("[WavesManager] No hay waves en la cola.");
                return;
            }

            var wave = spawnQueue.Primero();
            Debug.Log($"[WavesManager] Ejecutando wave con {wave.Count} enemigos. Spawners={spawners.Count}");

            StartCoroutine(SpawnWaveCoroutine(wave));
            spawnQueue.Desacolar();
        }

        private IEnumerator SpawnWaveCoroutine(List<GameObject> wave)
        {
            foreach (GameObject obj in wave)
            {
                if (obj == null)
                    continue;

                string prefabName = obj.name.Split('(')[0].Trim();
                var spawner = spawners[Random.Range(0, spawners.Count)];

                Debug.Log($"[WavesManager] Spawneando '{prefabName}' usando spawner {spawner.name}");
                spawner.SpawnEnemyOnRadius(prefabName);

                yield return new WaitForSeconds(spawnDelayBetweenEnemies);
            }
        }
    }
}
