using UnityEngine;
using LootLocker.Requests;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using System.Linq; // Necesario para ordenar listas
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

        public void ResetScores()
        {
            _localRound = 0;
        
            Hashtable props = new Hashtable { { "Score", 0 }, { "Kills", 0 } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }

        // --- AÑADIR PUNTOS (Se guardan en la red) ---
        public void AddScore(int amount)
        {
            int current = (int)PhotonNetwork.LocalPlayer.CustomProperties["Score"];
            Hashtable props = new Hashtable { { "Score", current + amount } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }

        public void AddKill()
        {
            int current = (int)PhotonNetwork.LocalPlayer.CustomProperties["Kills"];
            Hashtable props = new Hashtable { { "Kills", current + 1 } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }

        public void AddRound()
        {
            _localRound++;
         
        }

     
        public void SubmitGameResult()
        {
            StartCoroutine(SubmitRoutine());
        }

        IEnumerator SubmitRoutine()
        {
            string playerID = PlayerPrefs.GetString("PlayerID", "Guest");

            // Obtenemos el nombre actual de Photon para "pegarlo" al puntaje
            string myName = PhotonNetwork.NickName;
            if (string.IsNullOrEmpty(myName)) myName = "Player " + Random.Range(100, 999);

            // 1. CALCULAR MVP (Score)
            Player[] allPlayers = PhotonNetwork.PlayerList;
            var sortedByScore = allPlayers.OrderByDescending(p => (int)(p.CustomProperties["Score"] ?? 0)).ToArray();

            if (sortedByScore[0] == PhotonNetwork.LocalPlayer)
            {
                int myScore = (int)(PhotonNetwork.LocalPlayer.CustomProperties["Score"] ?? 0);
                if (myScore > 0)
                {
                    bool done = false;
                    // AQUÍ ESTÁ EL CAMBIO: Pasamos 'myName' como 4to argumento (metadata)
                    LootLockerSDKManager.SubmitScore(playerID, myScore, KEY_SCORE, myName, (r) => { done = true; });
                    yield return new WaitUntil(() => done);
                    Debug.Log("🏆 Score enviado con nombre: " + myName);
                }
            }

            // 2. CALCULAR MVP (Kills)
            var sortedByKills = allPlayers.OrderByDescending(p => (int)(p.CustomProperties["Kills"] ?? 0)).ToArray();

            if (sortedByKills[0] == PhotonNetwork.LocalPlayer)
            {
                int myKills = (int)(PhotonNetwork.LocalPlayer.CustomProperties["Kills"] ?? 0);
                if (myKills > 0)
                {
                    bool done = false;
                    // AQUÍ ESTÁ EL CAMBIO: Pasamos 'myName' como metadata también
                    LootLockerSDKManager.SubmitScore(playerID, myKills, KEY_KILLS, myName, (r) => { done = true; });
                    yield return new WaitUntil(() => done);
                    Debug.Log("🔫 Kills enviadas con nombre: " + myName);
                }
            }

            // 3. RONDAS (EQUIPO)
            if (PhotonNetwork.IsMasterClient && _localRound > 0)
            {
                string teamNames = string.Join(", ", allPlayers.Select(p => p.NickName));

                bool done = false;
                LootLockerSDKManager.SubmitScore(playerID, _localRound, KEY_ROUNDS, teamNames, (r) => { done = true; });
                yield return new WaitUntil(() => done);
                Debug.Log("🛡️ Rondas enviadas.");
            }
        }
    }
}