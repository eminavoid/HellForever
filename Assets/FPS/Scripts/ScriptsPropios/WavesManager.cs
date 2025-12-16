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
        private List<EnemySpawner> enemySpawner;
        private List<TrapSpawner> trapSpawner;
        [SerializeField] private List<GameObject> spawnObjects;
        private QueueTDA<List<GameObject>> spawnQueue;

        private bool firstWaveStart;

        public int WavesCount { get { return wavesCount; } }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            enemySpawner = new List<EnemySpawner>();
            trapSpawner = new List<TrapSpawner>();
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
            this.enemySpawner.Add(enemySpawner);
        }

        public void registerSpawner(TrapSpawner trapSpawner)
        {
            this.trapSpawner.Add(trapSpawner);
        }

        public void unregisterSpawner(EnemySpawner enemySpawner)
        {
            this.enemySpawner.Remove(enemySpawner);
        }

        public void WavesCreation()
        {
            if (spawnQueue.ColaVacia())
            {
                for (int i = 0; i < wavesCount; i++)
                {
                    int enemyQuantity = UnityEngine.Random.Range(minEnemyCount, maxEnemyCount);
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
            
            if (!spawnQueue.ColaVacia() && enemySpawner.Count != 0)
            {
                List<GameObject> wave = spawnQueue.Primero();

                foreach (GameObject obj in wave)
                {
                    if (obj.CompareTag("Enemy")) 
                    { 
                        enemySpawner[UnityEngine.Random.Range(0, enemySpawner.Count)].SpawnEnemyOnRadius(obj); 
                    }
                    else if (obj.CompareTag("Trap")) 
                    {                         
                        trapSpawner[UnityEngine.Random.Range(0, trapSpawner.Count)].SpawnEnemyOnRadius(obj);
                    }

                }

                spawnQueue.Desacolar();
            }
        }
    }

}