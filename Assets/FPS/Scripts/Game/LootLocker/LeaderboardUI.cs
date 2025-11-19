using UnityEngine;
using LootLocker.Requests;
using TMPro;

namespace Unity.FPS.Game
{
    public class LeaderboardUI : MonoBehaviour
    {
        public GameObject rowPrefab;
        public Transform contentParent;

        public void ShowScores(string key)
        {
            foreach (Transform child in contentParent) Destroy(child.gameObject);

            LootLockerSDKManager.GetScoreList(key, 10, (response) =>
            {
                if (response.success)
                {
                    foreach (var item in response.items)
                    {
                        GameObject row = Instantiate(rowPrefab, contentParent);
                        var texts = row.GetComponentsInChildren<TextMeshProUGUI>();

                        if (texts.Length >= 3)
                        {
                            texts[0].text = item.rank + ".";
                            texts[1].text = string.IsNullOrEmpty(item.player.name) ? item.player.id.ToString() : item.player.name;
                            texts[2].text = item.score.ToString();
                        }
                    }
                }
                else Debug.LogError("Error UI: " + response.errorData.message);
            });
        }

        public void ShowTotalScore() { ShowScores("total_score"); }
        public void ShowHighestRound() { ShowScores("highestround"); }
        public void ShowTopKills() { ShowScores("top_kills"); } // <--- Nuevo botón
    }
}