using UnityEngine;
using LootLocker.Requests;
using TMPro;

namespace Unity.FPS.Game
{
    public class LeaderboardUI : MonoBehaviour
    {
        [Header("Control")]
        public GameObject targetPanel;

        [Header("Referencias")]
        public GameObject rowPrefab;
        public Transform contentParent;

        public void OpenLeaderboard()
        {
            if (targetPanel) targetPanel.SetActive(true);
            ShowTopScore();
        }

        public void CloseLeaderboard()
        {
            if (targetPanel) targetPanel.SetActive(false);
        }

        public void ShowScores(string key)
        {
            if (rowPrefab == null || contentParent == null) return;

            foreach (Transform child in contentParent) Destroy(child.gameObject);
            Debug.Log($"🔄 Cargando: {key}...");

            LootLockerSDKManager.GetScoreList(key, 10, (response) =>
            {
                if (response.success)
                {
                    if (response.items.Length == 0) Debug.Log("Tabla vacía.");

                    foreach (var item in response.items)
                    {
                        GameObject row = Instantiate(rowPrefab, contentParent);
                        var texts = row.GetComponentsInChildren<TextMeshProUGUI>();

                        if (texts.Length >= 3)
                        {
                            // 1. RANK
                            texts[0].text = item.rank + ".";

                            // 2. NOMBRE (LA LÓGICA NUEVA)
                            // Prioridad 1: Metadata (El nombre que enviamos manualmente)
                            // Prioridad 2: Player Name (El nombre de cuenta LootLocker)
                            // Prioridad 3: ID (Si todo falla)
                            string displayName = "";

                            if (!string.IsNullOrEmpty(item.metadata))
                            {
                                displayName = item.metadata; // <--- ESTO SOLUCIONA TU PROBLEMA
                            }
                            else if (item.player != null && !string.IsNullOrEmpty(item.player.name))
                            {
                                displayName = item.player.name;
                            }
                            else
                            {
                                displayName = item.member_id;
                            }

                            texts[1].text = displayName;

                            // 3. PUNTAJE
                            texts[2].text = item.score.ToString();
                        }
                    }
                }
                else Debug.LogError("Error LootLocker: " + response.errorData.message);
            });
        }

        public void ShowTopScore() { ShowScores("top_score"); }
        public void ShowHighestRound() { ShowScores("highestround"); }
        public void ShowTopKills() { ShowScores("top_kills"); }
    }
}