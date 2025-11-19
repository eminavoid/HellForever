using UnityEngine;
using LootLocker.Requests;
using System.Collections;

namespace Unity.FPS.Game
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        // --- TUS KEYS ---
        private const string KEY_SCORE = "total_score";
        private const string KEY_ROUNDS = "highestround";
        private const string KEY_KILLS = "top_kills"; // <--- Agregada

        // Variables locales
        private int _score = 0;
        private int _round = 0;
        private int _kills = 0; // <--- Agregada

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        public void ResetScores()
        {
            _score = 0;
            _round = 0;
            _kills = 0; // <--- Resetear
            Debug.Log("ScoreManager: Reseteado.");
        }

        public void AddScore(int amount)
        {
            _score += amount;
            Debug.Log($"Score: {_score}");
        }

        public void AddRound()
        {
            _round++;
            Debug.Log($"Ronda: {_round}");
        }

        // --- NUEVO MÉTODO ---
        public void AddKill()
        {
            _kills++;
            Debug.Log($"Kills PvP: {_kills}");
        }

        public void SubmitToLeaderboard()
        {
            StartCoroutine(SubmitRoutine());
        }

        IEnumerator SubmitRoutine()
        {
            string playerID = PlayerPrefs.GetString("PlayerID", "Guest");

            // 1. Enviar Score
            bool doneScore = false;
            LootLockerSDKManager.SubmitScore(playerID, _score, KEY_SCORE, (r) => { doneScore = true; });
            yield return new WaitUntil(() => doneScore);

            // 2. Enviar Ronda
            bool doneRound = false;
            LootLockerSDKManager.SubmitScore(playerID, _round, KEY_ROUNDS, (r) => { doneRound = true; });
            yield return new WaitUntil(() => doneRound);

            // 3. Enviar Kills (NUEVO)
            bool doneKills = false;
            LootLockerSDKManager.SubmitScore(playerID, _kills, KEY_KILLS, (r) => { doneKills = true; });
            yield return new WaitUntil(() => doneKills);

            Debug.Log("✅ Todos los puntajes (Score, Rondas, Kills) subidos.");
        }
    }
}