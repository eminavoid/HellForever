using UnityEngine;
using TMPro;
using Photon.Realtime;
using Photon.Pun;

namespace Unity.FPS.UI
{
    public class MatchScoreboardItem : MonoBehaviour
    {
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI killsText;
        public TextMeshProUGUI scoreText;

        public void Initialize(Player player)
        {
            nameText.text = player.NickName;

            if (player == PhotonNetwork.LocalPlayer)
            {
                nameText.color = Color.yellow;
            }

            if (player.CustomProperties.TryGetValue("Score", out object scoreObj))
            {
                scoreText.text = scoreObj.ToString();
            }
            else
            {
                scoreText.text = "0";
            }

            if (player.CustomProperties.TryGetValue("Kills", out object killsObj))
            {
                killsText.text = killsObj.ToString();
            }
            else
            {
                killsText.text = "0";
            }
        }
    }
}