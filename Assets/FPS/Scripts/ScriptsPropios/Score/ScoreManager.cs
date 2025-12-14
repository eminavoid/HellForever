using Codice.Client.BaseCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using static PlasticPipe.PlasticProtocol.Messages.Serialization.ItemHandlerMessagesSerialization;
using Random = UnityEngine.Random;


namespace Unity.FPS.ours
{
    public class ScoreManager: MonoBehaviour
    {
        public static ScoreManager Instance;

        [Header("UI References")]
        private TextMeshProUGUI scoreText;
        private const string SCORE_TEXT_TAG = "ScoreTextDisplay";

        [Header("Score Values")]
        [SerializeField] public int scorePerEnemy = 100; 
        [SerializeField] public int scoreLostOnHit = 300; 

        [Header("Datos en Tiempo Real")]
        public int currentScore = 0;
        public GameSessionData lastRunData;

        [Header("Cronómetro")]
        private float startTime;
        private bool isTimerRunning = false;

        [Header("Historial Completo")]
        public List<GameSessionData> history = new List<GameSessionData>();
        private const string HISTORY_KEY = "FullHistorySave";

        public event Action OnScoresUpdated;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadHistory();
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            int index = scene.buildIndex;

            if (index != 0 && index != 1 && index != 2)
            {
                StartTimer();
            }

            if (index == 0)  
            {
                currentScore = 0;
                isTimerRunning = false;
            }
            else if (index == 1 || index == 2)     
            {
                if (isTimerRunning)
                {
                    StopAndSaveGame(index == 1);        
                }
            }

            UpdateSimpleUI(index);
        }

        void StartTimer()
        {
            startTime = Time.time;
            isTimerRunning = true;
            currentScore = 0;      
        }

        void StopAndSaveGame(bool isVictory)
        {
            isTimerRunning = false;
            float duration = Time.time - startTime;

            GameSessionData newData = new GameSessionData();
            newData.score = currentScore;
            newData.timePlayed = duration;
            newData.isVictory = isVictory;

            lastRunData = newData;

            history.Add(newData);
            SaveHistory();

            Debug.Log($"Partida Guardada: {newData.score} pts | {(isVictory ? "WIN" : "LOSE")} | {duration}s");

            currentScore = 0;
        }

        public void AddScore(int amount)
        {
            currentScore += amount;
            UpdateSimpleUI(SceneManager.GetActiveScene().buildIndex);
        }

        public void RemoveScore(int amount)
        {
            currentScore -= amount;
            UpdateSimpleUI(SceneManager.GetActiveScene().buildIndex);
        }

        void UpdateSimpleUI(int sceneIndex)
        {
            GameObject textObj = GameObject.FindWithTag(SCORE_TEXT_TAG);
            if (textObj == null) return;
            TextMeshProUGUI txt = textObj.GetComponent<TextMeshProUGUI>();

            if (sceneIndex == 0)  
            {
                txt.text = "";            
            }
            else if (sceneIndex == 1 || sceneIndex == 2)  
            {
                string status = lastRunData.isVictory ? "¡VICTORIA!" : "DERROTA";
                string timeStr = FormatTime(lastRunData.timePlayed);
                txt.text = $"{status}\nPuntos: {lastRunData.score}\nTiempo: {timeStr}";
            }
            else  
            {
                txt.text = "Puntos: " + currentScore;
            }
        }

        public string FormatTime(float timeInSeconds)
        {
            int minutes = Mathf.FloorToInt(timeInSeconds / 60F);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60F);
            return string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        void SaveHistory()
        {
            HistoryWrapper wrapper = new HistoryWrapper { list = history };
            string json = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString(HISTORY_KEY, json);
            PlayerPrefs.Save();
        }

        void LoadHistory()
        {
            if (PlayerPrefs.HasKey(HISTORY_KEY))
            {
                string json = PlayerPrefs.GetString(HISTORY_KEY);
                HistoryWrapper wrapper = JsonUtility.FromJson<HistoryWrapper>(json);
                if (wrapper != null) history = wrapper.list;
            }
            else history = new List<GameSessionData>();
        }

        public void DeleteHistory()
        {
            history.Clear();
            PlayerPrefs.DeleteKey(HISTORY_KEY);
        }

        #region Debug Methods

        public void InjectRandomGames()
        {
            for (int i = 0; i < 15; i++)
            {
                GameSessionData randomGame = new GameSessionData();

                randomGame.score = Random.Range(0, 5001);

                randomGame.timePlayed = Random.Range(10f, 300f);

                randomGame.isVictory = (Random.value > 0.5f);

                history.Add(randomGame);
            }

            SaveHistory();

            HistoryDisplay display = FindFirstObjectByType<HistoryDisplay>();
            if (display != null) display.PopulateList();        

            Debug.Log("¡Se inyectaron 15 partidas aleatorias!");

            OnScoresUpdated?.Invoke();
        }

        public void ResetAllData()
        {
            history.Clear();
            currentScore = 0;
            lastRunData = null;

            PlayerPrefs.DeleteKey(HISTORY_KEY);

            HistoryDisplay display = FindFirstObjectByType<HistoryDisplay>();
            if (display != null) display.PopulateList();

            Debug.Log("Todos los datos han sido borrados.");

            OnScoresUpdated?.Invoke();
        }

        #endregion

        #region Sorting (QuickSort)

        public void SortTimeAscending()
        {
            QuickSort(history, 0, history.Count - 1, false, true);
            RefreshAfterSort();
        }

        public void SortTimeDescending()
        {
            QuickSort(history, 0, history.Count - 1, false, false);
            RefreshAfterSort();
        }

        public void SortScoreDescending()
        {
            QuickSort(history, 0, history.Count - 1, true, false);
            RefreshAfterSort();
        }

        public void SortScoreAscending()
        {
            QuickSort(history, 0, history.Count - 1, true, true);
            RefreshAfterSort();
        }

        private void RefreshAfterSort()
        {
            SaveHistory();     
            OnScoresUpdated?.Invoke();         
            Debug.Log("Lista ordenada con QuickSort.");
        }

        private void QuickSort(List<GameSessionData> list, int low, int high, bool sortByScore, bool ascending)
        {
            if (low < high)
            {
                int pi = Partition(list, low, high, sortByScore, ascending);

                QuickSort(list, low, pi - 1, sortByScore, ascending);
                QuickSort(list, pi + 1, high, sortByScore, ascending);
            }
        }

        private int Partition(List<GameSessionData> list, int low, int high, bool sortByScore, bool ascending)
        {
            GameSessionData pivot = list[high];
            int i = (low - 1);

            for (int j = low; j < high; j++)
            {
                bool condition = false;

                if (sortByScore)
                {
                    if (ascending) condition = list[j].score < pivot.score;
                    else condition = list[j].score > pivot.score;
                }
                else
                {
                    if (ascending) condition = list[j].timePlayed < pivot.timePlayed;
                    else condition = list[j].timePlayed > pivot.timePlayed;
                }

                if (condition)
                {
                    i++;
                    GameSessionData temp = list[i];
                    list[i] = list[j];
                    list[j] = temp;
                }
            }

            GameSessionData temp2 = list[i + 1];
            list[i + 1] = list[high];
            list[high] = temp2;

            return i + 1;
        }

        #endregion

    }

    [System.Serializable]
    public class GameSessionData
    {
        public int score;
        public float timePlayed;
        public bool isVictory;
    }

    [System.Serializable]
    public class HistoryWrapper
    {
        public List<GameSessionData> list;
    }

}
