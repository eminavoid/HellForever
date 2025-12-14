using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.ours
{
    public class HistoryDisplay : MonoBehaviour
    {
        [Header("Referencias UI")]
        public GameObject rowPrefab;
        public Transform contentArea;

        void Start()
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoresUpdated += PopulateList;
            }

            PopulateList();
        }

        void OnDestroy()
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoresUpdated -= PopulateList;
            }
        }

        public void PopulateList()
        {
            foreach (Transform child in contentArea)
            {
                Destroy(child.gameObject);
            }

            if (ScoreManager.Instance == null) return;

            List<GameSessionData> fullList = new List<GameSessionData>(ScoreManager.Instance.history);

            foreach (GameSessionData data in fullList)
            {
                GameObject newRow = Instantiate(rowPrefab, contentArea);
                ScoreRowUI rowScript = newRow.GetComponent<ScoreRowUI>();

                if (rowScript != null)
                {
                    rowScript.Setup(data);
                }
            }
        }
    }
}