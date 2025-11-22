using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;

namespace Unity.FPS.Game
{
    public class PlayerStatsLocker : MonoBehaviourPun
    {
        public int CurrentScore { get; private set; }
        public int CurrentKills { get; private set; }

        void Start()
        {
            if (photonView.IsMine)
            {
                Hashtable props = new Hashtable { { "Score", 0 }, { "Kills", 0 } };
                PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            }
        }

        public void AddLocalScore(int amount)
        {
            if (!photonView.IsMine) return;

            CurrentScore += amount;
            UpdateNetworkStats();
        }

        public void AddLocalKill()
        {
            if (!photonView.IsMine) return;

            CurrentKills++;
            UpdateNetworkStats();
        }

        void UpdateNetworkStats()
        {
            Hashtable props = new Hashtable();
            props["Score"] = CurrentScore;
            props["Kills"] = CurrentKills;
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }
    }
}