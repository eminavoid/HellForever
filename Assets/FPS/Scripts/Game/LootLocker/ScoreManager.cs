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

        
        public void AddScore(int amount)
        {
            
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

        
        public void ProcessEndGameAndSubmit()
        {
            StartCoroutine(SubmitBestScoresRoutine());
        }

        IEnumerator SubmitBestScoresRoutine()
        {
            string playerID = PlayerPrefs.GetString("PlayerID", "Guest");
            Player[] allPlayers = PhotonNetwork.PlayerList;

            Debug.Log("📊 Calculando mejores puntajes de la sala...");

            
            var bestScorePlayer = allPlayers.OrderByDescending(p => (int)(p.CustomProperties["Score"] ?? 0)).FirstOrDefault();

            if (bestScorePlayer != null)
            {
                int bestScoreValue = (int)(bestScorePlayer.CustomProperties["Score"] ?? 0);

                
                if (bestScorePlayer == PhotonNetwork.LocalPlayer && bestScoreValue > 0)
                {
                    string name = PhotonNetwork.NickName;
                   
                    if (string.IsNullOrEmpty(name)) name = "Player " + PhotonNetwork.LocalPlayer.ActorNumber;

                    bool done = false;
                    
                    LootLockerSDKManager.SubmitScore(playerID, bestScoreValue, KEY_SCORE, name, (r) => { done = true; });
                    yield return new WaitUntil(() => done);
                    Debug.Log($"🏆 Subido TOP SCORE: {name} con {bestScoreValue}");
                }
            }

           
            var bestKillsPlayer = allPlayers.OrderByDescending(p => (int)(p.CustomProperties["Kills"] ?? 0)).FirstOrDefault();

            if (bestKillsPlayer != null)
            {
                int bestKillsValue = (int)(bestKillsPlayer.CustomProperties["Kills"] ?? 0);

                if (bestKillsPlayer == PhotonNetwork.LocalPlayer && bestKillsValue > 0)
                {
                    string name = PhotonNetwork.NickName;
                    if (string.IsNullOrEmpty(name)) name = "Player " + PhotonNetwork.LocalPlayer.ActorNumber;

                    bool done = false;
                    LootLockerSDKManager.SubmitScore(playerID, bestKillsValue, KEY_KILLS, name, (r) => { done = true; });
                    yield return new WaitUntil(() => done);
                    Debug.Log($"🔫 Subido TOP KILLS: {name} con {bestKillsValue}");
                }
            }

            
            if (PhotonNetwork.IsMasterClient && _localRound > 0)
            {
                string teamNames = string.Join(", ", allPlayers.Select(p => p.NickName));

                bool done = false;
                LootLockerSDKManager.SubmitScore(playerID, _localRound, KEY_ROUNDS, teamNames, (r) => { done = true; });
                yield return new WaitUntil(() => done);
                Debug.Log($"🛡️ Rondas subidas: {_localRound}");

            
            }
        }
    }
}