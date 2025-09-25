using Photon.Pun;
using System.Collections.Generic;
using Unity.FPS.ours;
using UnityEngine;

namespace Unity.FPS.Game
{
    public class WavesManager : MonoBehaviourPunCallbacks
    {
        [SerializeField] private int wavesCount = 5;
        [SerializeField] private int minEnemyCount = 1;
        [SerializeField] private int maxEnemyCount = 10;

        private readonly List<Unity.FPS.ours.EnemySpawner> spawners = new();
        [SerializeField] private List<GameObject> spawnObjects;

        private QueueTDA<List<GameObject>> spawnQueue;

        public int WavesCount { get { return wavesCount; } }


        public void registerSpawner(Unity.FPS.ours.EnemySpawner s)
        {
            if (!spawners.Contains(s)) spawners.Add(s);
            Debug.Log($"[WavesManager] Spawner registrado. Total={spawners.Count}");
        }

        public void unregisterSpawner(Unity.FPS.ours.EnemySpawner s)
        {
            spawners.Remove(s);
            Debug.Log($"[WavesManager] Spawner removido. Total={spawners.Count}");
        }

        void Awake()
        {
            spawnQueue = new QueueTDA<List<GameObject>>();
            spawnQueue.InicializarCola(wavesCount);
            CreateWaves();
        }

        void CreateWaves()
        {
            if (spawnQueue.ColaVacia())
            {
                for (int i = 0; i < wavesCount; i++)
                {
                    int count = Random.Range(minEnemyCount, maxEnemyCount + 1);
                    var list = new List<GameObject>(count);
                    for (int j = 0; j < count; j++)
                        list.Add(spawnObjects[Random.Range(0, spawnObjects.Count)]);
                    spawnQueue.Acolar(list);
                }
                Debug.Log($"[WavesManager] Creación de waves completa. Waves={wavesCount}");
            }
        }

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

        private void TryLaunchWave()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            if (spawners.Count == 0)
            {
                Debug.LogWarning("[WavesManager] No hay spawners aún, no puedo lanzar wave.");
                return;
            }

            WaveExecute();
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

            foreach (GameObject obj in wave)
            {
                string prefabName = obj.name;
                var spawner = spawners[Random.Range(0, spawners.Count)];
                spawner.SpawnEnemyOnRadius(prefabName);
            }

            spawnQueue.Desacolar();
        }
    }
}