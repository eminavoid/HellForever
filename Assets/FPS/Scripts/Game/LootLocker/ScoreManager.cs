using UnityEngine;
using LootLocker.Requests;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Unity.FPS.Game
{
    public class ScoreManager : MonoBehaviourPunCallbacks
    {
        public static ScoreManager Instance { get; private set; }

        private const string KEY_SCORE = "top_score";
        private const string KEY_ROUNDS = "highestround";
        private const string KEY_KILLS = "top_kills";

        private int _localRound = 0;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); }
            else { Instance = this; DontDestroyOnLoad(gameObject); }
        }

        // --- MÉTODOS QUE LLAMA HEALTH.CS ---
        public void AddScore(int amount)
        {
            // Suma al jugador local
            int current = (int)(PhotonNetwork.LocalPlayer.CustomProperties["Score"] ?? 0);
            Hashtable props = new Hashtable { { "Score", current + amount } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }

        public void AddKill()
        {
            int current = (int)(PhotonNetwork.LocalPlayer.CustomProperties["Kills"] ?? 0);
            Hashtable props = new Hashtable { { "Kills", current + 1 } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }

        public void AddRound()
        {
            _localRound++;
        }

        // --- FINAL DE JUEGO: BUSCAR AL MEJOR Y SUBIR ---
        public void ProcessEndGameAndSubmit()
        {
            StartCoroutine(SubmitBestScoresRoutine());
        }

        IEnumerator SubmitBestScoresRoutine()
        {
            string playerID = PlayerPrefs.GetString("PlayerID", "Guest");
            Player[] allPlayers = PhotonNetwork.PlayerList;

            Debug.Log("📊 Calculando mejores puntajes de la sala...");

            // 1. ENCONTRAR MVP DE SCORE
            // Ordenamos jugadores de mayor a menor puntaje
            var bestScorePlayer = allPlayers.OrderByDescending(p => (int)(p.CustomProperties["Score"] ?? 0)).First();
            int bestScoreValue = (int)(bestScorePlayer.CustomProperties["Score"] ?? 0);

            // Si YO soy el mejor (o hay empate y soy yo), YO subo el puntaje
            if (bestScorePlayer == PhotonNetwork.LocalPlayer && bestScoreValue > 0)
            {
                string name = PhotonNetwork.NickName;
                bool done = false;
                LootLockerSDKManager.SubmitScore(playerID, bestScoreValue, KEY_SCORE, name, (r) => { done = true; });
                yield return new WaitUntil(() => done);
                Debug.Log($"🏆 Subido TOP SCORE: {name} con {bestScoreValue}");
            }

            // 2. ENCONTRAR MVP DE KILLS
            var bestKillsPlayer = allPlayers.OrderByDescending(p => (int)(p.CustomProperties["Kills"] ?? 0)).First();
            int bestKillsValue = (int)(bestKillsPlayer.CustomProperties["Kills"] ?? 0);

            if (bestKillsPlayer == PhotonNetwork.LocalPlayer && bestKillsValue > 0)
            {
                string name = PhotonNetwork.NickName;
                bool done = false;
                LootLockerSDKManager.SubmitScore(playerID, bestKillsValue, KEY_KILLS, name, (r) => { done = true; });
                yield return new WaitUntil(() => done);
                Debug.Log($"🔫 Subido TOP KILLS: {name} con {bestKillsValue}");
            }

            // 3. SUBIR RONDAS (Solo Master Client, nombres de todos)
            if (PhotonNetwork.IsMasterClient && _localRound > 0)
            {
                string teamNames = string.Join(", ", allPlayers.Select(p => p.NickName));
                bool done = false;
                LootLockerSDKManager.SubmitScore(playerID, _localRound, KEY_ROUNDS, teamNames, (r) => { done = true; });
                yield return new WaitUntil(() => done);
                Debug.Log($"🛡️ Rondas subidas: {_localRound}");

                // Notificar al WavesManager que termine
                WavesManager wm = FindFirstObjectByType<WavesManager>();
                if (wm != null) wm.LoadVictoryScene();
            }
        }
    }
}