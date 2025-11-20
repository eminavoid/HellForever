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

                            // 2. NOMBRE (LÓGICA ESPECIAL)
                            if (key == "highestround" && !string.IsNullOrEmpty(item.metadata))
                            {
                                // Si es la tabla de Rondas y tiene metadata, mostramos LOS NOMBRES DEL EQUIPO
                                texts[1].text = item.metadata;
                                // Ajustar tamaño de fuente si son muchos nombres
                                texts[1].enableAutoSizing = true;
                            }
                            else
                            {
                                // Si es Score o Kills, mostramos UN solo nombre
                                string pName = "Unknown";
                                if (item.player != null)
                                    pName = !string.IsNullOrEmpty(item.player.name) ? item.player.name : item.player.id.ToString();
                                else
                                    pName = item.member_id;

                                texts[1].text = pName;
                            }

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