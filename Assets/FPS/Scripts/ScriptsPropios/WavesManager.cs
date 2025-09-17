using UnityEngine;
using Unity.FPS.ours;
using System.Collections.Generic;
using System.Linq;


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



        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            spawners = new List<EnemySpawner>();
            spawnQueue = new QueueTDA<List<GameObject>>();
            spawnQueue.InicializarCola(wavesCount);
        }

        // Update is called once per frame
        void Update()
        {

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

            for (int i = 0; i < wavesCount; i++)
            {
                int enemyQuantity = Random.Range(minEnemyCount, maxEnemyCount);
                List<GameObject> spawnObjects = new List<GameObject>();
                for (int j = 0; j < enemyQuantity; j++)
                {
                    spawnObjects.Add(spawnObjects[Random.Range(0, spawnObjects.Count)]);
                }
                spawnQueue.Acolar(spawnObjects);
            }
        }

        public void WaveExecute()
        {
            if (!spawnQueue.ColaVacia())
            {
                List<GameObject> wave = spawnQueue.Primero();

                foreach (GameObject obj in wave)
                {
                    spawners[Random.Range(0, spawners.Count)].SpawnEnemyOnRadius(obj);
                }

                spawnQueue.Desacolar();
            }
        }
    }

}