using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.ours;
using System;
using Hashtable = ExitGames.Client.Photon.Hashtable;

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

        public static event Action<int, int> OnWaveChanged;

        // 🔹 NUEVO: índice actual de ola y referencia al loop
        private int currentWaveIndex = 0;
        private Coroutine gameLoopCoroutine;

        // 🔹 NUEVO: clave usada en las CustomProperties de la room
        private const string ROOM_WAVE_INDEX_KEY = "CurrentWaveIndex";

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

            // 🔹 ahora usamos un método que tiene en cuenta el checkpoint
            StartGameLoopIfMaster();
        }

        // 🔹 NUEVO: lógica para arrancar el loop solo en el Master y desde la ola guardada
        private void StartGameLoopIfMaster()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            if (gameLoopCoroutine != null) return; // ya está corriendo

            // Leer ola desde las CustomProperties de la sala (si existe)
            if (PhotonNetwork.CurrentRoom != null &&
                PhotonNetwork.CurrentRoom.CustomProperties != null &&
                PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(ROOM_WAVE_INDEX_KEY, out object waveObj))
            {
                currentWaveIndex = (int)waveObj;
            }
            else
            {
                currentWaveIndex = 0;
            }

            gameLoopCoroutine = StartCoroutine(GameLoop());
        }

        // 🔹 NUEVO: cuando cambia el MasterClient, el nuevo Master reanuda las oleadas
        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            // Solo el nuevo master debe relanzar el loop
            if (!PhotonNetwork.IsMasterClient) return;

            Debug.Log("[WavesManager] Nuevo Master, reanudando oleadas desde el checkpoint...");

            if (gameLoopCoroutine != null)
            {
                StopCoroutine(gameLoopCoroutine);
                gameLoopCoroutine = null;
            }

            StartGameLoopIfMaster();
        }

        // 🔹 NUEVO: método público para forzar un reset a la última ola guardada
        public void ForceRestartFromSavedWave()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            // Releer por las dudas el índice guardado en la room
            if (PhotonNetwork.CurrentRoom != null &&
                PhotonNetwork.CurrentRoom.CustomProperties != null &&
                PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(ROOM_WAVE_INDEX_KEY, out object waveObj))
            {
                currentWaveIndex = (int)waveObj;
            }

            if (gameLoopCoroutine != null)
            {
                StopCoroutine(gameLoopCoroutine);
                gameLoopCoroutine = null;
            }

            Debug.Log($"[WavesManager] Reiniciando GameLoop desde la ola {currentWaveIndex + 1}");
            StartGameLoopIfMaster();
        }

        private IEnumerator GameLoop()
        {
            while (spawners.Count == 0)
            {
                Debug.LogWarning("[WavesManager] Esperando spawners...");
                yield return new WaitForSeconds(0.5f);
            }

            isGameActive = true;
            Debug.Log(" INICIO DEL JUEGO");

            // FASE 1: OLEADAS NORMALES
            // 🔹 IMPORTANTE: arrancamos desde currentWaveIndex (checkpoint)
            for (int i = currentWaveIndex; i < wavesList.Count; i++)
            {
                currentWaveIndex = i;

                // 🔹 Guardar el índice de la ola actual en las CustomProperties de la sala
                if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom != null)
                {
                    Hashtable props = new Hashtable
                    {
                        { ROOM_WAVE_INDEX_KEY, currentWaveIndex }
                    };
                    PhotonNetwork.CurrentRoom.SetCustomProperties(props);
                }

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

            gameLoopCoroutine = null; // por las dudas
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
