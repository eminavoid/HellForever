using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;     

namespace Unity.FPS.UI
{
    public class MatchScoreboardUI : MonoBehaviourPunCallbacks
    {
        [Header("Referencias UI")]
        public GameObject scoreboardPanel;     
        public Transform container;            
        public GameObject rowPrefab;             

        [Header("Configuración")]
        public KeyCode openKey = KeyCode.Tab;

        void Start()
        {
            if (scoreboardPanel) scoreboardPanel.SetActive(false);
        }

        void Update()
        {
            if (Input.GetKeyDown(openKey))
            {
                scoreboardPanel.SetActive(true);
                RefreshBoard();
            }

            if (Input.GetKeyUp(openKey))
            {
                scoreboardPanel.SetActive(false);
            }
        }

        void RefreshBoard()
        {
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }

            var players = PhotonNetwork.PlayerList.OrderByDescending(p =>
                p.CustomProperties.ContainsKey("Score") ? (int)p.CustomProperties["Score"] : 0
            ).ToList();

            foreach (Player p in players)
            {
                GameObject newRow = Instantiate(rowPrefab, container);
                MatchScoreboardItem item = newRow.GetComponent<MatchScoreboardItem>();
                if (item != null)
                {
                    item.Initialize(p);
                }
            }
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
        {
            if (scoreboardPanel.activeSelf)
            {
                RefreshBoard();
            }
        }

        public override void OnPlayerEnteredRoom(Player newPlayer) { if (scoreboardPanel.activeSelf) RefreshBoard(); }
        public override void OnPlayerLeftRoom(Player otherPlayer) { if (scoreboardPanel.activeSelf) RefreshBoard(); }
    }
}