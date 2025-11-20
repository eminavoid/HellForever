using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.ours;
using System.Linq;
using System; // <--- 1. ESTE USING ES OBLIGATORIO PARA QUE FUNCIONE EL EVENTO

namespace Unity.FPS.Game
{
    [System.Serializable]
    public class WaveEnemyConfig
    {
        public string Name;
        public GameObject Prefab;
        [Range(0, 100)] public float DropChance;
    }

    [System.Serializable]
    public class WaveSettings
    {
        public string WaveName = "Wave 1";
        public float DurationSeconds = 30f;
        public float SpawnInterval = 4f;

        [Header("Enemigos de esta oleada")]
        public List<WaveEnemyConfig> enemiesInThisWave;
    }

    public class WavesManager : MonoBehaviourPunCallbacks
    {
        [Header("Configuración de Olas")]
        [SerializeField] private List<WaveSettings> wavesList;

        [Header("Modo Infinito")]
        [SerializeField] private bool enableEndless = false;

        [Header("Escenas")]
        [SerializeField] private string victorySceneName = "VictoryScene";

        private readonly List<EnemySpawner> spawners = new();
        private bool isGameActive = false;

        // --- 2. ESTA ES LA LÍNEA QUE TE FALTA Y CAUSA EL ERROR ---
        public static event Action<int, int> OnWaveChanged;
        // ---------------------------------------------------------

        // Compatibilidad con ObjectiveKillEnemies
        public int WavesCount => wavesList.Count;
        public void WaveExecute() { }

        public void registerSpawner(EnemySpawner s)
        {
            if (!spawners.Contains(s)) spawners.Add(s);
        }

        public void unregisterSpawner(EnemySpawner s)
        {
            spawners.Remove(s);
        }

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => PhotonNetwork.InRoom && PhotonNetwork.IsConnected);

            if (PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(GameLoop());
            }
        }

        private IEnumerator GameLoop()
        {
            while (spawners.Count == 0)
            {
                Debug.LogWarning("[WavesManager] Esperando spawners...");
                yield return new WaitForSeconds(0.5f);
            }

            isGameActive = true;
            Debug.Log("🎮 INICIO DEL JUEGO");

            // FASE 1: OLEADAS NORMALES
            for (int i = 0; i < wavesList.Count; i++)
            {
                // Disparar evento
                OnWaveChanged?.Invoke(i + 1, wavesList.Count);

                yield return StartCoroutine(RunSingleWave(wavesList[i]));
            }

            // FASE 2: ENDLESS
            if (enableEndless && wavesList.Count > 0)
            {
                Debug.Log("♾️ INICIANDO MODO ENDLESS");
                WaveSettings baseWave = wavesList[wavesList.Count - 1];
                int endlessRoundNumber = 1;

                while (isGameActive)
                {
                    WaveSettings endlessWave = new WaveSettings();
                    endlessWave.WaveName = $"{baseWave.WaveName} (Extra {endlessRoundNumber})";
                    endlessWave.SpawnInterval = baseWave.SpawnInterval;
                    endlessWave.enemiesInThisWave = baseWave.enemiesInThisWave;

                    float extraTime = baseWave.SpawnInterval * endlessRoundNumber;
                    endlessWave.DurationSeconds = baseWave.DurationSeconds + extraTime;

                    int currentDisplayRound = wavesList.Count + endlessRoundNumber;

                    // Disparar evento (Total 999 indica infinito)
                    OnWaveChanged?.Invoke(currentDisplayRound, 999);

                    yield return StartCoroutine(RunSingleWave(endlessWave));
                    endlessRoundNumber++;
                }
            }
            else
            {
                EndGame();
            }
        }

        private IEnumerator RunSingleWave(WaveSettings currentWave)
        {
            Debug.Log($"🌊 INICIANDO: {currentWave.WaveName}");

            GameObject managers = GameObject.Find("_Managers");
            if (managers != null) managers.SendMessage("AddRound", SendMessageOptions.DontRequireReceiver);

            GameObject gameHUD = GameObject.Find("GameHUD");
            if (gameHUD != null) gameHUD.SendMessage("CreateNotification", currentWave.WaveName, SendMessageOptions.DontRequireReceiver);

            float timer = 0f;
            float nextSpawnTime = 0f;

            while (timer < currentWave.DurationSeconds)
            {
                timer += Time.deltaTime;
                if (Time.time >= nextSpawnTime)
                {
                    SpawnRandomEnemy(currentWave.enemiesInThisWave);
                    nextSpawnTime = Time.time + currentWave.SpawnInterval;
                }
                yield return null;
            }

            Debug.Log($"✅ FIN DE {currentWave.WaveName}");
            yield return new WaitForSeconds(2f);
        }

        private void SpawnRandomEnemy(List<WaveEnemyConfig> enemiesAvailable)
        {
            if (spawners.Count == 0) return;
            if (enemiesAvailable == null || enemiesAvailable.Count == 0) return;

            GameObject prefabToSpawn = GetWeightedRandomEnemy(enemiesAvailable);
            if (prefabToSpawn == null) return;

            var spawner = spawners[UnityEngine.Random.Range(0, spawners.Count)];
            string cleanName = prefabToSpawn.name;
            spawner.SpawnEnemyOnRadius(cleanName);
        }

        private GameObject GetWeightedRandomEnemy(List<WaveEnemyConfig> enemies)
        {
            float totalWeight = 0f;
            foreach (var e in enemies) totalWeight += e.DropChance;

            float randomValue = UnityEngine.Random.Range(0, totalWeight);
            float currentSum = 0f;

            foreach (var e in enemies)
            {
                currentSum += e.DropChance;
                if (randomValue <= currentSum) return e.Prefab;
            }
            return null;
        }

        private void EndGame()
        {
            Debug.Log("🏆 JUEGO TERMINADO (VICTORIA)");
            isGameActive = false;

            
            GameObject managers = GameObject.Find("_Managers");
            if (managers != null)
            {
                managers.SendMessage("ProcessEndGameAndSubmit", SendMessageOptions.DontRequireReceiver);
            }

           
            StartCoroutine(WaitAndLoadVictory());
        }

        private IEnumerator WaitAndLoadVictory()
        {
            
            yield return new WaitForSeconds(3f);

            PhotonNetwork.LoadLevel(victorySceneName);
        }
    }
}