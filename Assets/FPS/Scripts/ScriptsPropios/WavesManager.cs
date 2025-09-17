using UnityEngine;
using Unity.FPS.ours;
using System.Collections.Generic;
using System.Linq;
using System;


namespace Unity.FPS.Game
{
    public class WavesManager : MonoBehaviour
    {
        [SerializeField] private int wavesCount = 5;
        [SerializeField] private int minEnemyCount = 1;
        [SerializeField] private int maxEnemyCount = 10;
        private List<EnemySpawner> spawners;
        [SerializeField] private List<GameObject> spawnObjects;
        private QueueTDA<List<GameObject>> spawnQueue;

        private bool firstWaveStart;

        public int WavesCount { get { return wavesCount; } }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            spawners = new List<EnemySpawner>();
            spawnQueue = new QueueTDA<List<GameObject>>();
            spawnQueue.InicializarCola(wavesCount);
            firstWaveStart = false;
            WavesCreation();
        }

        private void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            if (!firstWaveStart) 
            {
                WaveExecute();
                firstWaveStart=true;
            }
        }

        public void registerSpawner(EnemySpawner enemySpawner)
        {
            spawners.Add(enemySpawner);
        }

        public void unregisterSpawner(EnemySpawner enemySpawner)
        {
            spawners.Remove(enemySpawner);
        }

        public void WavesCreation()
        {
            if (spawnQueue.ColaVacia())
            {
                for (int i = 0; i < wavesCount; i++)
                {
                    int enemyQuantity = UnityEngine.Random.Range(minEnemyCount, maxEnemyCount);
                    Debug.Log(spawnObjects.Count);
                    List<GameObject> spawnObjectsList = new List<GameObject>();
                    for (int j = 0; j < enemyQuantity; j++)
                    {
                        spawnObjectsList.Add(spawnObjects[UnityEngine.Random.Range(0,spawnObjects.Count)]);
                    }
                    spawnQueue.Acolar(spawnObjectsList);
                }
            }
        }

        public void WaveExecute()
        {
            
            if (!spawnQueue.ColaVacia() && spawners.Count != 0)
            {
                List<GameObject> wave = spawnQueue.Primero();
                Debug.Log("lista de wave tiene: " + wave.Count);

                foreach (GameObject obj in wave)
                {
                    spawners[UnityEngine.Random.Range(0, spawners.Count)].SpawnEnemyOnRadius(obj);
                }

                spawnQueue.Desacolar();
            }
        }
    }

}