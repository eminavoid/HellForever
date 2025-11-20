using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;

namespace Unity.FPS.Game
{
    public class PlayerStatsLocker : MonoBehaviourPun
    {
        // Variables locales para acceso rápido
        public int CurrentScore { get; private set; }
        public int CurrentKills { get; private set; }

        void Start()
        {
            if (photonView.IsMine)
            {
                // Inicializar stats en la red al nacer
                Hashtable props = new Hashtable { { "Score", 0 }, { "Kills", 0 } };
                PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            }
        }

        // Llamar a esto cuando matas a un enemigo
        public void AddLocalScore(int amount)
        {
            if (!photonView.IsMine) return;

            CurrentScore += amount;
            UpdateNetworkStats();
        }

        // Llamar a esto cuando matas a un jugador
        public void AddLocalKill()
        {
            if (!photonView.IsMine) return;

            CurrentKills++;
            UpdateNetworkStats();
        }

        void UpdateNetworkStats()
        {
            // Subimos los datos a la nube de Photon
            Hashtable props = new Hashtable();
            props["Score"] = CurrentScore;
            props["Kills"] = CurrentKills;
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }
    }
}