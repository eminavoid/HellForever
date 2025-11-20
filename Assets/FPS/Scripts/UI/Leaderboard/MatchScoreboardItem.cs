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
            // 1. Nombre
            nameText.text = player.NickName;

            // Color especial si soy YO
            if (player == PhotonNetwork.LocalPlayer)
            {
                nameText.color = Color.yellow;
            }

            // 2. Score (Leemos de la nube de Photon)
            if (player.CustomProperties.TryGetValue("Score", out object scoreObj))
            {
                scoreText.text = scoreObj.ToString();
            }
            else
            {
                scoreText.text = "0";
            }

            // 3. Kills (Leemos de la nube de Photon)
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