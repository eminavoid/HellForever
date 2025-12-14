using TMPro;
using UnityEngine;


namespace Unity.FPS.ours 
{
    public class ScoreRowUI : MonoBehaviour
    {
        public TextMeshProUGUI resultText;   
        public TextMeshProUGUI timeText;     
        public TextMeshProUGUI scoreText;    

        public void Setup(GameSessionData data)
        {
            if (data.isVictory)
            {
                resultText.text = "VICTORIA";
                resultText.color = Color.green;     
            }
            else
            {
                resultText.text = "DERROTA";
                resultText.color = Color.red;       
            }

            if (ScoreManager.Instance != null)
                timeText.text = ScoreManager.Instance.FormatTime(data.timePlayed);
            else
                timeText.text = data.timePlayed.ToString("F0") + "s";

            scoreText.text = data.score.ToString();
        }
    }
}

