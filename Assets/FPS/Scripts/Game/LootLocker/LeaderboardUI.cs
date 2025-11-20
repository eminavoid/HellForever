using UnityEngine;
using LootLocker.Requests;
using TMPro;

namespace Unity.FPS.Game
{
    public class LeaderboardUI : MonoBehaviour
    {
        [Header("Control de Visualización")]
        [Tooltip("Arrastra aquí el Panel (GameObject) que quieres ocultar y mostrar")]
        public GameObject targetPanel;

        [Header("Referencias de Tabla")]
        public GameObject rowPrefab;
        public Transform contentParent;

        // -------------------------------------------------------
        // MÉTODOS PARA LOS BOTONES (Abrir y Cerrar)
        // -------------------------------------------------------

        // Asigna este método al botón de "Ranking" en el Menú
        public void OpenLeaderboard()
        {
            if (targetPanel != null)
            {
                targetPanel.SetActive(true); // Muestra el elemento que seleccionaste
                ShowTopScore(); // Carga los datos automáticamente
            }
            else
            {
                Debug.LogError("⚠️ No has asignado el 'Target Panel' en el Inspector de LeaderboardUI");
            }
        }

        // Asigna este método al botón "X" (Cerrar)
        public void CloseLeaderboard()
        {
            if (targetPanel != null)
            {
                targetPanel.SetActive(false); // Oculta el elemento que seleccionaste
            }
        }

        // -------------------------------------------------------
        // LÓGICA INTERNA (Carga de datos blindada)
        // -------------------------------------------------------

        public void ShowScores(string key)
        {
            if (rowPrefab == null || contentParent == null) return;

            // Limpiar tabla
            foreach (Transform child in contentParent) Destroy(child.gameObject);

            

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
                            texts[0].text = item.rank + ".";

                            // Protección contra nombres nulos
                            string pName = "Unknown";
                            if (item.player != null)
                                pName = !string.IsNullOrEmpty(item.player.name) ? item.player.name : item.player.id.ToString();
                            else
                                pName = item.member_id;

                            texts[1].text = pName;
                            texts[2].text = item.score.ToString();
                        }
                    }
                }
                else
                {
                    Debug.LogError("Error LootLocker: " + response.errorData.message);
                }
            });
        }

        public void ShowTopScore() { ShowScores("top_score"); }
        public void ShowHighestRound() { ShowScores("highestround"); }
        public void ShowTopKills() { ShowScores("top_kills"); }
    }
}