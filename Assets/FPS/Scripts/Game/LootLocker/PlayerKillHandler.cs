using UnityEngine;
using Photon.Pun;

namespace Unity.FPS.Game
{
    public class PlayerKillHandler : MonoBehaviourPun
    {
        
        [PunRPC]
        public void AddKillRPC()
        {
           
            if (photonView.IsMine)
            {
                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.AddKill();
                }
            }
        }
    }
}